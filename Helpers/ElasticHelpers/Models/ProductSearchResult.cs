using System.Collections.Generic;
using ClerkBot.Models.Electronics.Mobile;

namespace ClerkBot.Helpers.ElasticHelpers.Models
{
    public class ProductSearchResult
    {
        public long TotalItems { get; set; }
        public List<MobileElastic> Products { get; set; }
        public Dictionary<string, Dictionary<string, long>> StringAggregations { get; set; }
        public Dictionary<string, NumericAggregation> NumericAggregations { get; set; }
    }

    public class NumericAggregation
    {
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Avg { get; set; }
    }
}
