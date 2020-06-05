using ClerkBot.Enums;
using ClerkBot.Helpers.SmartEnum.JsonNet;
using Newtonsoft.Json;

namespace ClerkBot.Models.Electronics.Mobile.Features
{
    public class AspectFeatureMobile: IMobileFeature
    {
        public int PriorityOrder { get; set; }

        public bool SizeSmall { get; set; }
        public bool SizeMedium { get; set; }
        public bool SizeBig { get; set; }

        [JsonConverter(typeof(SmartEnumNameConverter<Intensity, int>))]
        public Intensity Thickness { get; set; } = Intensity.Unknown;
    }
}
