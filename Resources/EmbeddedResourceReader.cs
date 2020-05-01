using System;
using System.IO;
using System.Reflection;

namespace ClerkBot.Resources
{
    public class EmbeddedResourceReader
    {
        private readonly string resourseName;

        public EmbeddedResourceReader(string name)
        {
            resourseName = name;
        }

        public string GetJson()
        {
            return GetData($"{GetType().Namespace}.{resourseName}.json");
        }

        private static string GetData(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream ?? throw new InvalidOperationException());
            var fileContents = reader.ReadToEnd();
            return fileContents;
        }
    }
}
