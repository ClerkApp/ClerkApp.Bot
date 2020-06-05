using System;
using System.Collections.Generic;
using ClerkBot.Enums;
using ClerkBot.Helpers.SmartEnum.JsonNet;
using ClerkBot.Models.Database;
using ClerkBot.Models.Electronics.Mobile.Features;
using Newtonsoft.Json;

namespace ClerkBot.Models.Electronics.Mobile
{
    public class MobileProfile: IUserProfile
    {
        public MobileProfile()
        {
            BudgetRanges = new List<BudgetRanges>();
            Features = new List<IMobileFeature>();
            WantedFeatures = new PhoneFeatures();
        }

        public PhoneFeatures WantedFeatures { get; set; }

        public List<BudgetRanges> BudgetRanges { get; set; }

        public List<IMobileFeature> Features { get; }

        public bool ReliableBrands { get; set; } = false;

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

        public class PhoneFeatures
        {
            public bool Camera { get; set; } = false;
            public bool Gaming { get; set; } = false;
            public bool Call { get; set; } = false;
            public bool Browsing { get; set; } = false;
            public bool Aspect { get; set; } = false;
            public bool Budget { get; set; } = false;
        }
    }
}
