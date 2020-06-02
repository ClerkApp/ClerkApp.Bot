using ClerkBot.Enums;
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

        [JsonConverter(typeof(SmartEnumNameConverter<Intensity, int>))]
        public Intensity RecordTypes { get; set; }

        [JsonConverter(typeof(SmartEnumNameConverter<Periodicity, int>))]
        public Periodicity UsedFrequency { get; set; }

        [JsonConverter(typeof(SmartEnumNameConverter<Periodicity, int>))]
        public Periodicity NightMode { get; set; }
    }
}
