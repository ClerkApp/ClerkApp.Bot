using System;
using System.Collections.Generic;
using ClerkBot.Enums;
using Newtonsoft.Json;

namespace ClerkBot.Models.Electronics.Phone.Features
{
    public class CameraPhoneFeature: IPhoneFeature
    {
        public Frequency UsedFrequency { get; set; }
        public Frequency NightMode { get; set; }

        public bool SelfieMode { get; set; }
        public bool Print { get; set; }
        public bool FourK { get; set; }

        [JsonConverter(typeof(CommaListConverter))]
        public List<string> RecordTypes { get; set; }
        [JsonConverter(typeof(CommaListConverter))]
        public List<string> Platforms { get; set; }
    }

    // https://stackoverflow.com/a/45284854
    public class CommaListConverter : JsonConverter
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

    public enum Frequency
    {
        hour,
        day,
        week,
        month,
        ocasions
    }
}
