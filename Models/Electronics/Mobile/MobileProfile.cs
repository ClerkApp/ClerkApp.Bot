using System;
using System.Collections.Generic;
using ClerkBot.Enums;

namespace ClerkBot.Models.Electronics.Mobile
{
    public class MobileProfile
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

        public DropDurability Durability { get; set; }

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

        public enum DropDurability
        {
            often,
            rare,
            hardly
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
