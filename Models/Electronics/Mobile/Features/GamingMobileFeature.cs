using ClerkBot.Enums;
using ClerkBot.Helpers.SmartEnum.JsonNet;
using Newtonsoft.Json;

namespace ClerkBot.Models.Electronics.Mobile.Features
{
    public class GamingMobileFeature: IMobileFeature
    {
        public int PriorityOrder { get; set; }

        [JsonConverter(typeof(SmartEnumNameConverter<Intensity, int>))]
        public Intensity GamingIntensity { get; set; }

    }
}
