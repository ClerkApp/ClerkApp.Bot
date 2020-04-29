using System.Collections.Generic;

namespace ClerkBot.Models
{
    public class PhoneProfile
    {
        public PhoneProfile()
        {
            BugetRanges = new List<BugetRanges>();
            Features = new List<Features>();
        }

        public List<BugetRanges> BugetRanges { get; set; }

        public List<Features> Features { get; set; }

        public List<string> Brands { get; set; }

        public List<string> Colors { get; set; }

        public int Carefulness { get; set; }
    }

    public enum BugetRanges
    {
        lowBudget,
        midRange,
        highEnd,
        flagship
    }

    public enum Features
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
