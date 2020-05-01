using System.Collections.Generic;
using ClerkBot.Enums;

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
}
