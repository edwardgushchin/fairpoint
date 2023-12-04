#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fairpoint
{
    public class PlatformManager
    {
        public PlatformManager()
        {
            new Thread(CheckAndSendProject).Start(Sites.Avito);
            new Thread(CheckAndSendProject).Start(Sites.Cian);
        }

        private void CheckAndSendProject(object? o)
        {
            var platformObj = new object();
            var site = (Sites) (o ?? throw new ArgumentNullException(nameof(o)));

            switch (site)
            {
                case Sites.Avito:
                    platformObj = new Avito();
                    ((Avito) platformObj).Initialize();
                    break;
                
                case Sites.Cian:
                    platformObj = new Cian();
                    ((Cian) platformObj).Initialize();
                    break;
            }

            while (true)
            {
                try
                {
                    var projects = new List<Advert>();
                    
                    switch (site)
                    {
                        case Sites.Avito:
                            projects = ((Avito)platformObj).Update().Distinct().ToList();
                            break;
                        
                        case Sites.Cian:
                            projects = ((Cian)platformObj).Update().Distinct().ToList();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    foreach (var advert in projects)
                    {
                        Kernel.Telegram.SendAdvert(advert);
                        
                    }

                    Thread.Sleep(60000);
                }
                catch (Exception e)
                {
                    switch (site)
                    {
                        case Sites.Avito:
                            Debug.Log(e.ToString(), Debug.Sender.Avito, Debug.MessageStatus.Error);
                            break;
                        
                        case Sites.Cian:
                            Debug.Log(e.ToString(), Debug.Sender.Cian, Debug.MessageStatus.Error);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}