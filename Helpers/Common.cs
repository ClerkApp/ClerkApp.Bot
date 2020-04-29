using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ClerkBot.Helpers
{
    public static class Common
    {
        public static readonly List<string> BugTypes = new List<string>() { "Security", "Crash", "Power", "Performance", "Usability", "Serious Bug", "Other" };

        public static string BuildDialogId()
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            
            if (methodInfo.ReflectedType == null)
            {
                throw new Exception("Cannot get caller class name for building the DialogId.");
            }

            return methodInfo.ReflectedType.Name;
        }

        public static string BuildDialogId(string dialogId)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();

            if (methodInfo.ReflectedType == null)
            {
                throw new Exception("Cannot get caller class name for building the DialogId.");
            }

            return $"{methodInfo.ReflectedType.Name}.{dialogId}";
        }

        public static Attachment CreateAdaptiveCardAttachment(string adaptiveCardJson)
        {
            //var path = Path.Combine(".", "Resources", "Cards", "AdaptiveCards", "ChoiceSet", "PhoneWantedFeatures", "json");
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        public static T CreateInstance<T>(params object[] args)
        {
            var type = typeof(T);
            var instance = type.Assembly.CreateInstance(
                type.FullName ?? throw new InvalidOperationException(), false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);
            return (T)instance;
        }
    }
}
