using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fairpoint
{
    public class Proxy
    {
        public Proxy()
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("proxy.json"));

                var address = obj["address"].ToString();
                var port = obj["port"].ToObject<int>();
                var login = obj["login"].ToString();
                var password = obj["password"].ToString();
                var debug = obj["debug"].ToObject<bool>();

                if (address != string.Empty && port != 0 && login != string.Empty && password != string.Empty)
                {
                    Initialized = true;
                    Address = address;
                    Port = port;
                    Login = login;
                    Password = password;
                    IsDebug = debug;
                }
                    
                else
                {
                    Debug.Log("Error. The proxy.json file has errors: values cannot equal 0 and be empty.", Debug.Sender.Proxy, Debug.MessageStatus.Error);
                    Initialized = false;
                }
            }
            catch (FileNotFoundException)
            {
                Debug.Log("Error. The proxy settings file proxy.json was not found. Continuation is impossible.", Debug.Sender.Proxy, Debug.MessageStatus.Error);
                Initialized = false;
            }

            
        }
        
        public string Address { get; }
        
        public int Port { get; }
        
        public string Login { get; }
        
        public string Password { get; }
        
        public bool Initialized { get;  }
        
        public bool IsDebug { get;  }
    }
}