using System;
using System.IO;
using Tesseract;
using System.Net;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Security;
using OpenQA.Selenium.Chrome;


namespace Fairpoint
{
    public class Avito : Platform
    {
        public override string BaseUrl => "https://www.avito.ru";

        private List<Advert> _projectsHeap;
        private Random _random;
        public ChromeDriver Chrome1;

        public Avito()
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            _projectsHeap = new List<Advert>();
            if (!Kernel.Proxy.IsDebug)
            {
                var service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;

                var options = new ChromeOptions();
                options.AddArgument("--window-position=-32000,-32000");
                //options.AddArgument("headless"); 
                options.AddExtension("yourExt.zip");
                Chrome1 = new ChromeDriver(service, options);
            }
            else
            {
                var service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                var options = new ChromeOptions();
                options.AddExtension("yourExt.zip");
                Chrome1 = new ChromeDriver(service, options);
            }
            _random = new Random();
        }
        
        public override void Initialize()
        {
            Debug.Log("Initializing the Avito subsystem...", Debug.Sender.Avito, Debug.MessageStatus.Warning);
            _projectsHeap = GetProjectsHeap().Result;
            Debug.Log("The Avito subsystem has been successfully initialized!", Debug.Sender.Avito, Debug.MessageStatus.Success);
        }
        
        private async Task<List<Advert>> GetProjectsHeap()
        {
            var list = new List<Advert>();

            var taskList = Task.WhenAll(
                GetProjectList("/moskovskaya_oblast/zemelnye_uchastki/prodam-ASgBAgICAUSWA9oQ?cd=1&f=ASgBAQICAUSWA9oQAUCmCCToVeZV&i=1&s=104&user=1"),
                
                GetProjectList("/moskovskaya_oblast/doma_dachi_kottedzhi/prodam-ASgBAgICAUSUA9AQ?cd=1&s=104&user=1")
            );

            var resultList = await taskList;

            foreach (var projectList in resultList)
                list.AddRange(projectList);

            return list;
        }
        
        public override async Task<List<Advert>> GetProjectList(string url)
        {

            var projects = new List<Advert>();
            try
            {
                Chrome1.Navigate().GoToUrl(BaseUrl + url);
                
                Thread.Sleep(5000);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(Chrome1.PageSource);

                var removeNodes = htmlDoc.DocumentNode.SelectNodes("//div[starts-with(@class, \"items-vip\")]");
                if (removeNodes != null)
                    foreach (var node in removeNodes.ToList())
                        node.Remove();

                var orderNodes = htmlDoc.DocumentNode.SelectNodes("//div[@data-marker=\"item\"]");
                if (orderNodes == null)
                {
                    Debug.Log("Fatal error: Security system triggered. Intervention required.", Debug.Sender.Avito, Debug.MessageStatus.Error);
                    return projects;
                }
                orderNodes.Remove(orderNodes.Count - 1);

                foreach (var order in orderNodes)
                {
                    string img;

                    var getImg = order.SelectSingleNode(".//*[@itemprop=\"image\"]");

                    if (getImg != null)
                        img = getImg.Attributes["src"].Value;
                    else break;
                    
                    var title = order.SelectSingleNode(".//*[@data-marker=\"item-title\"]").Attributes["title"].Value;
                    var href = order.SelectSingleNode(".//*[@data-marker=\"item-title\"]").Attributes["href"].Value;

                    var imageId = order.Attributes["data-item-id"].Value;
                    
                    var imgUrl = $"https://www.avito.ru/web/1/items/phone/{imageId}";

                    var desc1Node = order.SelectSingleNode(".//*[@data-marker=\"item-address\"]/div/span/span");
                    var desc2Node = order.SelectSingleNode(".//*[@data-marker=\"item-address\"]/div/div/span");

                    var description = string.Empty;

                    if (desc1Node != null && desc2Node != null)
                    {
                        description = $"{desc1Node.InnerText}, {desc2Node.InnerText}";
                    }

                    if (desc1Node != null && desc2Node == null)
                    {
                        description = $"{desc1Node.InnerText}";
                    }

                    if (desc1Node == null && desc2Node != null)
                    {
                        description = $"{desc2Node.InnerText}";
                    }

                    //
                    var newDesk = order.SelectSingleNode(".//div[starts-with(@class, \"iva-item-description\")]");
                    string newDescription = null;
                    if (newDesk != null) newDescription = newDesk.InnerText;

                    var desc = description.Replace("&nbsp;", " ");

                    var cost = order.SelectSingleNode(".//*[@data-marker=\"item-price\"]").InnerText
                        .Replace("&nbsp;", " ");

                    var project = new Advert(title, img, desc, cost, href, newDescription, Sites.Avito, imgUrl);

                    projects.Add(project);
                }
            }
            catch (WebException e)
            {
                var res = (HttpWebResponse) e.Response;
                if (res is {StatusCode: HttpStatusCode.TooManyRequests})
                {
                    Debug.Log("Protection against DDOS attacks has been triggered. Retry after 2 minutes.", Debug.Sender.Avito, Debug.MessageStatus.Warning);
                    Thread.Sleep(120000);
                    return await GetProjectList(url);
                }
                else
                {
                    Debug.Log($"{e.Message}", Debug.Sender.Cian, Debug.MessageStatus.Warning); 
                    return await GetProjectList(url);
                }
            }
            catch (OpenQA.Selenium.WebDriverException)
            {
                if (!Kernel.Proxy.IsDebug)
                {
                    var service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;

                    var options = new ChromeOptions();
                    options.AddArgument("--window-position=-32000,-32000");
                    //options.AddArgument("headless");
                    options.AddExtension("yourExt.zip");
                    //options.AddArguments(Kernel.ProxyArgument);

                    Chrome1 = new ChromeDriver(service, options);
                }
                else
                {
                    var service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;
                    var options = new ChromeOptions();
                    options.AddExtension("yourExt.zip");
                    Chrome1 = new ChromeDriver(service, options);
                    
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Received an unexpected EOF or 0 bytes from the transport stream.")
                {
                    Debug.Log($"{e.Message}", Debug.Sender.Avito, Debug.MessageStatus.Warning); 
                }
                else Debug.Log($"{e}", Debug.Sender.Avito, Debug.MessageStatus.Error); 
                return await GetProjectList(url);
            }

            return await Task.FromResult(projects);
        }
        
        public override List<Advert> Update()
        {
            var newProjectList = GetProjectsHeap().Result;

            var projectsIsNew = newProjectList.Where(x=> _projectsHeap.All(y => y.Hash != x.Hash)).ToList();
            projectsIsNew.ForEach(x =>
            {
                try
                {
                    if (x.Hash == _projectsHeap[^1].Hash)
                        projectsIsNew.Remove(x);
                }
                catch (ArgumentOutOfRangeException)
                {
                    
                    //Debugger.Break();
                    //throw;
                }
                
            });
            
            _projectsHeap = newProjectList;
            return projectsIsNew;
        }
    }
}