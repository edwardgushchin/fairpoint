using System;
using System.Net;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Fairpoint
{
    public class TelegramBot
    {
        private const string TOKEN = "1811378594:AAESg0Hr3taUIOACcaYVMio1Rj5bb2u3yS8";
        private const string ERROR_SENDER = "@eduardgushchin";
        private const string CHANNEL_SENDER = "-1001439782164";

        private readonly TelegramBotClient _client;

        public TelegramBot()
        {
            _client = new TelegramBotClient(TOKEN);
        }

        public void SendMessage(string message)
        {
            _client.SendTextMessageAsync(CHANNEL_SENDER, message);
        }

        public void SendAdvert(Advert advert)
        {
            try
            {
                if (advert.Telephone == null) 
                    return;
            
                var telephone = advert.Telephone;
            
                if (telephone != null && telephone.StartsWith("8"))
                {
                    var strB = new StringBuilder(telephone);
                    strB[0] = '7';
                    telephone = strB.ToString().Insert(0, "+").Replace("-", "").Replace(" ", "");
                }

                var newDesk = advert.NewDescription != null
                    ? advert.NewDescription + Environment.NewLine + Environment.NewLine
                    : string.Empty;

                var desk = advert.Description != null
                    ? advert.Description + Environment.NewLine + Environment.NewLine
                    : string.Empty;
            

                var text = $"<b>{advert.Title} ({advert.Cost.Replace("&nbsp;", " ")})</b>" +
                           $"{Environment.NewLine}" +
                           $"{Environment.NewLine}" +
                           $"{desk}" +
                           $"{TruncateLongStringAtWord(newDesk, 350)}" +
                           $"Телефон: {telephone}";
            
                _client.SendPhotoAsync(CHANNEL_SENDER, advert.ImageUrl, text, ParseMode.Html);
            
                Debug.Log($"[{advert.Site}] {advert.Title}", Debug.Sender.Platform, Debug.MessageStatus.Log);
                Thread.Sleep(10000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Debug.Log($"{e.Message}", Debug.Sender.TelegramBot, Debug.MessageStatus.Warning); 
            }
            
        }

        private static string TruncateLongStringAtWord(string inputString, int maxChars, string postfix = "...")
        {
            if (maxChars <= 0)
                return inputString;
            if (inputString == null || inputString.Length < maxChars)
                return inputString;
            var lastSpaceIndex = inputString.LastIndexOf(" ", maxChars, StringComparison.Ordinal);
            var substringLength = (lastSpaceIndex > 0) ? lastSpaceIndex : maxChars;
            var truncatedString = inputString[..substringLength].Trim() + postfix + Environment.NewLine + Environment.NewLine;
            return truncatedString.Replace("&nbsp;", " ");
        }

    }
}