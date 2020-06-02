using System;
using System.IO;
using System.Reflection;

namespace ClerkBot.Resources
{
    public class EmbeddedResourceReader
    {
        private readonly string resourceName;
        private const string cardsFolder = "Cards";

        public EmbeddedResourceReader(string name)
        {
            resourceName = name;
        }

        public string GetJson()
        {
            return GetData($"{GetType().Namespace}.{cardsFolder}.{resourceName}.json");
        }

        private static string GetData(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream ?? throw new InvalidOperationException($"Can't load {resourceName}"));
            var fileContents = reader.ReadToEnd();
            return fileContents;
        }
    }
}
