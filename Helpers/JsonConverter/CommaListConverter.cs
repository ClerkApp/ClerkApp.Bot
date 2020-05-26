using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClerkBot.Helpers.JsonConverter
{
    // https://stackoverflow.com/a/45284854
    public class CommaListConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<string>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(string.Join(",", (List<string>)value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new List<string>(((string)reader.Value)?.Split(',') ?? Array.Empty<string>());
        }
    }
}
