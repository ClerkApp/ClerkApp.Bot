using System;
using System.Collections.Generic;
using ClerkBot.Enums;

namespace ClerkBot.Models.Electronics.Phone
{
    public class PhoneProfile
    {
        public PhoneProfile()
        {
            BugetRanges = new List<BugetRanges>();
            FeaturesList = new List<PhoneFeatures>();
            Features = new List<IPhoneFeature>();
        }

        public List<BugetRanges> BugetRanges { get; set; }

        public List<PhoneFeatures> FeaturesList { get; set; }

        public List<IPhoneFeature> Features { get; }

        public List<string> Brands { get; set; }

        public List<string> Colors { get; set; }

        public int Carefulness { get; set; }

        public bool TryAddFeature(IPhoneFeature feature)
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
