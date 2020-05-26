using System;
using System.Collections.Generic;
using ClerkBot.Enums;
using ClerkBot.Helpers.SmartEnum.JsonNet;
using ClerkBot.Models.Electronics.Mobile.Features;
using Newtonsoft.Json;

namespace ClerkBot.Models.Electronics.Mobile
{
    public class MobileProfile: IUserProfile
    {
        public MobileProfile()
        {
            BugetRanges = new List<BugetRanges>();
            FeaturesList = new List<PhoneFeatures>();
            Features = new List<IMobileFeature>();
        }

        public List<BugetRanges> BugetRanges { get; set; }

        public List<PhoneFeatures> FeaturesList { get; set; }

        public List<IMobileFeature> Features { get; }

        public List<string> Brands { get; set; }

        public List<string> Colors { get; set; }

        [JsonConverter(typeof(SmartEnumValueConverter<Intensity, int>))]
        public Intensity Durability { get; set; } = Intensity.Unknown;

        public bool TryAddFeature(IMobileFeature feature)
        {
            try
            {
                Features.Add(feature);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public enum PhoneFeatures
        {
            camera,
            gaming,
            agnostic,
            socialMedia,
            browsing,
            streaming,
            calls
        }
    }
}
