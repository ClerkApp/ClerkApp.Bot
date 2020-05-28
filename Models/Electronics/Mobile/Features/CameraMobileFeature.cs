using System.Collections.Generic;
using ClerkBot.Enums;
using ClerkBot.Helpers.JsonConverter;
using Newtonsoft.Json;
using ClerkBot.Helpers.SmartEnum.JsonNet;

namespace ClerkBot.Models.Electronics.Mobile.Features
{
    public class CameraMobileFeature: IMobileFeature, ICardAction<bool>
    {
        public int PriorityOrder { get; set; }
        public bool Action { get; set; }
        public bool SelfieMode { get; set; }
        public bool Print { get; set; }
        public bool KResolution { get; set; }
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
