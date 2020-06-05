using ClerkBot.Enums;
using Newtonsoft.Json;
using ClerkBot.Helpers.SmartEnum.JsonNet;

namespace ClerkBot.Models.Electronics.Mobile.Features
{
    public class CameraFeatureMobile: IMobileFeature, ICardAction<bool>
    {
        public int PriorityOrder { get; set; }
        public bool Action { get; set; }
        public bool SelfieMode { get; set; }
        public bool Print { get; set; }
        public bool KResolution { get; set; }

        [JsonConverter(typeof(SmartEnumNameConverter<Intensity, int>))]
        public Intensity RecordTypes { get; set; } = Intensity.Unknown;

        [JsonConverter(typeof(SmartEnumNameConverter<Periodicity, int>))]
        public Periodicity UsedFrequency { get; set; } = Periodicity.Unknown;

        [JsonConverter(typeof(SmartEnumNameConverter<Periodicity, int>))]
        public Periodicity NightMode { get; set; } = Periodicity.Unknown;
    }
}
