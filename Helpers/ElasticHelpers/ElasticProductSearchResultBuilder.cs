using System.Collections.Generic;
using System.Linq;
using ClerkBot.Helpers.ElasticHelpers.Models;
using ClerkBot.Models.Electronics.Mobile;
using Nest;

namespace ClerkBot.Helpers.ElasticHelpers
{
    public static class ElasticProductSearchResultBuilder
    {
        public static ProductSearchResult BuildProductSearchResult(ISearchResponse<MobileElastic> searchResponse)
        {
            var result = new ProductSearchResult()
            {
                TotalItems = searchResponse.Total,
                Products = searchResponse.Documents.ToList(),
            };
            ExtractAggregations(result, searchResponse.Aggregations);
            return result;
        }

        private static void ExtractAggregations(ProductSearchResult result, IReadOnlyDictionary<string, IAggregate> aggregates)
        {
            result.StringAggregations = new Dictionary<string, Dictionary<string, long>>();
            result.NumericAggregations = new Dictionary<string, NumericAggregation>();
            foreach (var y in aggregates)
            {
                if (y.Value.GetType() == typeof(Nest.BucketAggregate))
                {
                    ExtractStringAggregation(result, y);
                }
                else if (y.Value.GetType() == typeof(Nest.ValueAggregate))
                {
                    var aggName = y.Key.Split('.')[0];
                    if (!result.NumericAggregations.ContainsKey(aggName))
                    {
                        result.NumericAggregations.Add(aggName, new NumericAggregation());
                    }
                    var agg = result.NumericAggregations[aggName];
                    switch (y.Key.Split('.')[1])
                    {
                        case "min":
                            agg.Min = ((ValueAggregate)y.Value).Value;
                            break;
                        case "max":
                            agg.Max = ((ValueAggregate)y.Value).Value;
                            break;
                        case "avg":
                            agg.Avg = ((ValueAggregate)y.Value).Value;
                            break;
                    }
                }
            }
        }

        private static void ExtractStringAggregation(ProductSearchResult result, KeyValuePair<string, IAggregate> y)
        {
            var agg = new Dictionary<string, long>();

            foreach (var bucket in ((Nest.BucketAggregate)y.Value).Items)
            {
                if (bucket.GetType() != typeof(KeyedBucket<object>)) continue;
                var docCount = ((KeyedBucket<object>)bucket).DocCount;
                if (docCount == null) continue;
                agg.Add((string)((KeyedBucket<object>)bucket).Key, docCount.Value);
            }
            result.StringAggregations.Add(y.Key, agg);
        }
    }
}
