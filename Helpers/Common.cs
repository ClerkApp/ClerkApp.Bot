using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using AdaptiveCards;
using ClerkBot.Models.Dialog;
using ClerkBot.Resources;
using Nest;
using Newtonsoft.Json;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace ClerkBot.Helpers
{
    public static class Common
    {
        public static readonly List<string> BugTypes = new List<string>() { "Security", "Crash", "Power", "Performance", "Usability", "Serious Bug", "Other" };

        public static string BuildDialogId()
        {
            var methodInfo = new StackTrace().GetFrame(1)?.GetMethod();
            
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

        public static Attachment CreateAdaptiveCardAttachment(string jsonData)
        {
            //var path = Path.Combine(".", "Resources", "Cards", "AdaptiveCards", "ChoiceSet", "PhoneWantedFeatures", "json");
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = AdaptiveCard.FromJson(jsonData).Card
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

        //https://stackoverflow.com/a/50128943
        public static bool ArePropertiesNotNull<T>(this T obj)
        {
            return PropertyCache<T>.PublicProperties.All(propertyInfo => propertyInfo.GetValue(obj) != null);
        }

        public static class PropertyCache<T>
        {
            private static readonly Lazy<IReadOnlyCollection<PropertyInfo>> publicPropertiesLazy
                = new Lazy<IReadOnlyCollection<PropertyInfo>>(() => typeof(T).GetProperties());

            public static IReadOnlyCollection<PropertyInfo> PublicProperties => publicPropertiesLazy.Value;
        }

        // https://stackoverflow.com/a/4489031
        public static List<string> SplitCamelCase(this string source) {
            return Regex.Split(source, @"(?<!^)(?=[A-Z])").ToList();
        }

        public static string GetCardName(this string className, [CallerMemberName]string name = "")
        {
            var cardName = string.Join("", name.SplitCamelCase().SkipLast(1));
            return $"{cardName}{className}";
        }

        public static string GetDialogType(this string dialogClassName)
        {
            return dialogClassName.SplitCamelCase()[^2];
        }
        
        public static string GetCallerMethodName([CallerMemberName]string name = "")
        {
            return name;
        }

        public static string TryGetRootDialog(this string dialog)
        {
            return GetAllTypes(typeof(IRootDialog)).Find(x => x.Equals(dialog));
        }

        public static string TryGetSpecificDialog(this string dialog)
        {
            return GetAllTypes(typeof(ISpecificDialog)).Find(x => x.Equals(dialog));
        }

        public static List<string> TryGetAllSpecificDialog()
        {
            return GetAllTypes(typeof(ISpecificDialog));
        }

        public static string TryParseEnum<TEnum>(string value, out TEnum result) where TEnum: struct
        {
            Enum.TryParse(value, out result);
            return result.ToTitleCase();
        }

        public static string ToJson<T>(this ISearchResponse<T> response) where T : class
        {
            return Encoding.UTF8.GetString(response.ApiCall.RequestBodyInBytes);
        }
    }
}
