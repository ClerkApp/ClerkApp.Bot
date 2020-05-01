using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ClerkBot.Contracts;
using ClerkBot.Resources;
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

        public static Attachment CreateAdaptiveCardAttachment(this EmbeddedResourceReader embeddedResource, string name = null)
        {
            //var path = Path.Combine(".", "Resources", "Cards", "AdaptiveCards", "ChoiceSet", "PhoneWantedFeatures", "json");
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(embeddedResource.GetJson()),
                Name = name
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

        // https://stackoverflow.com/questions/4135317/make-first-letter-of-a-string-upper-case-with-maximum-performance
        public static string ToTitleCase(this object input)
        {
            return input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.ToString().First().ToString().ToUpper() + input.ToString().Substring(1).ToLower()
            };
        }

        // https://garywoodfine.com/get-c-classes-implementing-interface/
        public static List<string> GetAllTypes(Type typeToSearch)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeToSearch.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => x.Name).ToList();
        }


        public static string TryGetRootDialog(this string dialog)
        {
            return GetAllTypes(typeof(IRootDialog)).Find(x => x.Contains(dialog));
        }

        public static string TryGetSpecificDialog(this string dialog)
        {
            return GetAllTypes(typeof(ISpecificDialog)).Find(x => x.Contains(dialog));
        }

        public static string TryParseEnum<TEnum>(string value, out TEnum result) where TEnum: struct
        {
            Enum.TryParse(value, out result);
            return result.ToTitleCase();
        }
    }
}
