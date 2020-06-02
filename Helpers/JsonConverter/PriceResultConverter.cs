using System;
using System.Collections.Generic;
using ClerkBot.Models.Electronics.Mobile.Enrichment;
using Newtonsoft.Json;

namespace ClerkBot.Helpers.JsonConverter
{
    public class PriceResultConverter: Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<object>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var prices = value as Dictionary<string, double>;
            writer.WriteValue(prices[nameof(CurrencyType.EUR)]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return (double)reader.Value;
        }
    }
}
