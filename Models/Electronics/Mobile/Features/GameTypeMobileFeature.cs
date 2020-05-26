using ClerkBot.Enums;
using ClerkBot.Helpers.SmartEnum.JsonNet;
using Newtonsoft.Json;

namespace ClerkBot.Models.Electronics.Mobile.Features
{
    public class GameTypeMobileFeature: IMobileFeature
    {
        [JsonConverter(typeof(SmartEnumNameConverter<Intensity, int>))]
        public Intensity GamingIntensity { get; set; }
    }
}
