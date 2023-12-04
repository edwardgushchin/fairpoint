using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Fairpoint
{
    public class Cian : Platform
    {
        public override string BaseUrl => "https://www.cian.ru";

        public ChromeDriver Chrome1;
        
        private List<Advert> _projectsHeap;
        
        public Cian()
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            
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
        }
        
        public override void Initialize()
        {
            Debug.Log("Initializing the Cian subsystem...", Debug.Sender.Cian, Debug.MessageStatus.Warning);
            _projectsHeap = GetProjectsHeap().Result;
            Debug.Log("The Cian subsystem has been successfully initialized!", Debug.Sender.Cian, Debug.MessageStatus.Success);
        }
        
        private async Task<List<Advert>> GetProjectsHeap()
        {
            var list = new List<Advert>();

            var taskList = Task.WhenAll(
                GetProjectList("/cat.php?deal_type=sale&engine_version=2&is_by_homeowner=1&object_type%5B0%5D=1&object_type%5B1%5D=2&object_type%5B2%5D=3&object_type%5B3%5D=4&offer_type=suburban&region=4593&sort=creation_date_desc&wp=1")
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

                var orderNodes = htmlDoc.DocumentNode.SelectNodes("//article[@data-name=\"CardComponent\"]");

                if (orderNodes == null)
                {
                    Debug.Log("Protection against DDOS attacks has been triggered. Intervention required.",
                        Debug.Sender.Cian, Debug.MessageStatus.Warning);
                    return projects;
                }

                orderNodes.Remove(orderNodes.Count - 1);

                foreach (var order in orderNodes)
                {
                    var title = order.SelectSingleNode(".//span[@data-mark=\"OfferTitle\"]").InnerText
                        .Replace("&nbsp;", " ");

                    var imageUrl = order.SelectSingleNode(".//img").Attributes["src"].Value;

                    var desk = order.SelectSingleNode(".//div[@data-name=\"ContentRow\"][1]/div[2]");

                    string description = null;
                    if (desk != null) description = desk.InnerText;

                    var cost = order.SelectSingleNode(".//div[@data-name=\"ContentRow\"][2]").InnerText;

                    var uri = order.SelectSingleNode(".//div[@data-name=\"LinkArea\"]/a").Attributes["href"].Value;

                    var newDesc = order.SelectSingleNode(".//div[@data-name=\"LinkArea\"]/div[4]").InnerText;

                    var project = new Advert(title, imageUrl, description, cost, uri, newDesc, Sites.Cian);

                    projects.Add(project);
                }
            }
            catch (WebException e)
            {
                var res = (HttpWebResponse) e.Response;
                if (res is {StatusCode: HttpStatusCode.TooManyRequests})
                {
                    Debug.Log("Protection against DDOS attacks has been triggered. Retry after 2 minutes.",
                        Debug.Sender.Cian, Debug.MessageStatus.Warning);
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
                    Debug.Log($"{e.Message}", Debug.Sender.Cian, Debug.MessageStatus.Warning); 
                }
                else Debug.Log($"{e}", Debug.Sender.Cian, Debug.MessageStatus.Error); 
                return await GetProjectList(url);
            }
            
            return await Task.FromResult(projects);
        }
        
        public override List<Advert> Update()
        {
            var newProjectList = GetProjectsHeap().Result;

            var projectsIsNew = newProjectList.Where(x=> _projectsHeap.All(y => y.Hash != x.Hash)).ToList();

            if(_projectsHeap.Count > 0) 
            {
                projectsIsNew.ForEach(x =>
                {
                    
                    if (x.Hash == _projectsHeap[^1].Hash)
                        projectsIsNew.Remove(x);
                });
            }
            
            if (projectsIsNew.Count > 0) 
                _projectsHeap = newProjectList;
            return projectsIsNew;
        }
    }
}