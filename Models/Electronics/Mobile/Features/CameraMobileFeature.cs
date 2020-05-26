using System.Collections.Generic;
using ClerkBot.Enums;
using ClerkBot.Helpers.JsonConverter;
using Newtonsoft.Json;
using ClerkBot.Helpers.SmartEnum.JsonNet;

namespace ClerkBot.Models.Electronics.Mobile.Features
{
    public class CameraMobileFeature: IMobileFeature
    {
        public bool SelfieMode { get; set; }
        public bool Print { get; set; }
        public bool FourK { get; set; }
        public PhotoSaturation Saturation { get; set; }

        [JsonConverter(typeof(CommaListConverter))]
        public List<string> RecordTypes { get; set; }
        [JsonConverter(typeof(SmartEnumNameConverter<Periodicity, int>))]
        public Periodicity UsedFrequency { get; set; }
        [JsonConverter(typeof(SmartEnumNameConverter<Periodicity, int>))]
        public Periodicity NightMode { get; set; }
    }

    public enum PhotoSaturation
    {
        natural,
        beautify
    }
}
