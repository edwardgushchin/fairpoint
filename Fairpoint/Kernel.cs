using System;
using System.Diagnostics;
using System.Net;
using System.Net.Security;

namespace Fairpoint
{
    class Kernel
    {
        public static TelegramBot Telegram;
        public static Proxy Proxy;
        public static string ProxyArgument;

        public const string USER_AGENT = "Mozilla/6.0 (X11; CrOS i686 2268.111.0) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.57 Safari/536.11";
        public const string ACCEPT = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        private static void Main(string[] args)
        {
            Debug.Message("Fairpoint version 1.0");
            Debug.NewLine();
            Debug.Message("Copyright © 2021 Edward Gushchin. All rights reserved.");
            Debug.NewLine();
            Debug.Message("Need a bot? For you here: https://t.me/eduardgushchin");
            Debug.NewLine();
            
            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
            //{
                // local dev, just approve all certs
             //   if (!Debugger.IsAttached) return true;
             //   return errors == SslPolicyErrors.None ;
            //};
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            
            
            if(Debugger.IsAttached)
            {
                Debug.Log("The Fairpoint is running in DEBUG mode.", Debug.Sender.Main, Debug.MessageStatus.Warning);
                Debug.NewLine();
            }

            Debug.Log("Bot telegram initialization...", Debug.Sender.Main, Debug.MessageStatus.Warning);
            Telegram = new TelegramBot();
            Debug.Log("Bot telegram were initialized successfully.", Debug.Sender.Main, Debug.MessageStatus.Success);
            
            //Telegram.SendMessage("Тест");
            
            Debug.Log("Proxy subsystem initialization...", Debug.Sender.Main, Debug.MessageStatus.Warning);
            Proxy = new Proxy();
            ProxyArgument = $"--proxy-server=http://{Proxy.Address}:{Proxy.Port}";
            if (!Proxy.Initialized) return;
            Debug.Log("Proxy subsystem were initialized successfully.", Debug.Sender.Main,  Debug.MessageStatus.Success);
                
            Debug.Log("Initializing ad space manager...", Debug.Sender.Main, Debug.MessageStatus.Warning);
            var platformManager = new PlatformManager();
            Debug.Log("The ad space manager has been successfully initialized.", Debug.Sender.Main, Debug.MessageStatus.Success);

            
            //--proxy-server=http://user:password@208.xx.xx.xx:yyyy
            //--proxy-server=http://Max24Bro_pub:YKf7Lpr0e@s03.trueproxy.cc:1248
            //--proxy-server=http://{0}:{1};https://{0}:{1}"
        }
    }
}