using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using Tesseract;

namespace Fairpoint
{
    public struct Advert
    {
        public Advert(string title, string imageUrl,  string description, string cost, string url, string telephone, string newDesk, Sites site)
        {
            Title = title;
            ImageUrl = imageUrl;
            Description = description;
            Cost = cost;
            Url = url;
            _tel = telephone;
            NewDescription = newDesk;
            Site = site;
            telUrl = null;
        }
        
        public Advert(string title, string imageUrl,  string description, string cost, string url, string newDesk, Sites site)
        {
            Title = title;
            ImageUrl = imageUrl;
            Description = description;
            Cost = cost;
            Url = url;
            NewDescription = newDesk;
            _tel = null;
            Site = site;
            telUrl = null;
        }
        
        public Advert(string title, string imageUrl,  string description, string cost, string url, string newDesk, Sites site, string telurl)
        {
            Title = title;
            ImageUrl = imageUrl;
            Description = description;
            Cost = cost;
            Url = url;
            NewDescription = newDesk;
            _tel = null;
            Site = site;
            telUrl = telurl;
        }


        private string telUrl;

        public Sites Site { get; }
        
        public string Title { get; }

        public string Url { get; }

        public string Description { get; }

        public string Cost { get; }
        
        public string ImageUrl { get; }

        private string _tel;

        public string Telephone
        {
            get
            {
                
                if (Site == Sites.Cian) 
                {
                    if (_tel != null) return _tel;

                    var adhtml = new HtmlDocument();
                        
                    var service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;
                    
                    try
                    {
                        var options = new ChromeOptions();
                        options.AddArgument("--window-position=-32000,-32000");
                        //options.AddArgument("headless");
                        options.AddExtension("yourExt.zip");

                        var chrome = new ChromeDriver(service, options);
                        chrome.Navigate().GoToUrl(Url);
                        Thread.Sleep(5000);
                        
                        adhtml.LoadHtml(chrome.PageSource);
                        chrome.Quit();
                        
                        var tels = adhtml.DocumentNode.SelectSingleNode("//div[@data-name=\"OfferContactsAside\"]/div[2]/a");
                    
                        if(tels != null) _tel = tels.InnerText.Replace("-", "").Replace(" ", "");
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    return _tel;
                }
                else
                {
                    Thread.Sleep(60000);

                    try
                    {
                        ChromeDriver chrome; 
                        if (!Kernel.Proxy.IsDebug)
                        {
                            var service = ChromeDriverService.CreateDefaultService();
                            service.HideCommandPromptWindow = true;

                            var options = new ChromeOptions();
                            options.AddArgument("--window-position=-32000,-32000");
                            //options.AddArgument("headless"); 
                            options.AddExtension("yourExt.zip");

                            chrome = new ChromeDriver(service, options);
                        }
                        else
                        {
                            var service = ChromeDriverService.CreateDefaultService();
                            service.HideCommandPromptWindow = true;
                            var options = new ChromeOptions();
                            options.AddArgument("--window-position=-32000,-32000");
                            options.AddExtension("yourExt.zip");
                            chrome = new ChromeDriver(service, options);
                        }
                        chrome.Navigate().GoToUrl(telUrl);
                        Thread.Sleep(5000);
                        
                        var adhtml = new HtmlDocument();
                        adhtml.LoadHtml(chrome.PageSource);
                        chrome.Quit();

                        var doc = JsonDocument.Parse(adhtml.DocumentNode.InnerText);
                        string base64;

                        if (doc.RootElement.TryGetProperty("isAnonym", out var isAnonym))
                        {
                            if (!bool.Parse(isAnonym.ToString()!))
                            {
                                base64 = doc.RootElement.GetProperty("image64").ToString()
                                    ?.Replace("data:image/png;base64,", "");
                            }
                            else
                            {
                                base64 = doc.RootElement.GetProperty("anonymImage64").ToString()
                                    ?.Replace("data:image/png;base64,", "");
                            }
                        }
                        else return null;
                        
                        var imageBytes = Convert.FromBase64String(base64 ?? string.Empty);
                        var ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

                        using var engine = new TesseractEngine(@"tessdata", "eng", EngineMode.Default);
                        engine.SetVariable("user_defined_dpi", "300");
                        using var imag = Pix.LoadFromMemory(ms.ToArray());
                        using var recognizedPage = engine.Process(imag);
                        _tel = recognizedPage.GetText().Replace("\n", "");
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "The given key was not present in the dictionary.")
                        {
                            Console.WriteLine(e);
                        }
                        else
                        {
                            Debug.Log(e.Message, Debug.Sender.Avito, Debug.MessageStatus.Warning);
                            if (e.InnerException != null) Debug.Log(e.InnerException.ToString(), Debug.MessageStatus.Warning);
                        }
                        return null;
                    }
                    return _tel;
                }
            }
        }
        
        public string NewDescription { get; }

        public int Hash => HashCode.Combine(Url);

        public override int GetHashCode()
        {
            return Hash;
        }

        public override bool Equals(object obj)
        {
            return obj != null && Url == ((Advert) obj).Url;
        }

        public override string ToString() => $"{Title} ({Telephone})";
    }
}