using System;
using System.IO;
using System.Reflection;

namespace ClerkBot.Resources
{
    public class EmbeddedResourceReader
    {
        public string GetJson(string name)
        {
            return GetData($"{GetType().Namespace}.{name}.json");
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
