using System.Collections.Generic;
using Nest;

namespace ClerkBot.Helpers.ElasticHelpers
{
    public static class ElasticAggregationHelper
    {
        public static AggregationDictionary AggregationBuilder(List<string> stringPropertyFieldNames,
            List<string> numericPropertyFieldNames)
        {
            var aggregationDict = new AggregationDictionary();
            if (stringPropertyFieldNames != null && stringPropertyFieldNames.Count > 0)
                StringAggregations(aggregationDict, stringPropertyFieldNames);
            if (numericPropertyFieldNames != null && numericPropertyFieldNames.Count > 0)
                NumericAggregations(aggregationDict, numericPropertyFieldNames);
            return aggregationDict;
        }

        private static void StringAggregations(AggregationDictionary aggregationDict,
            IEnumerable<string> propertyFieldNames)
        {
            foreach (var propertyFieldName in propertyFieldNames)
            {
                aggregationDict.Add(propertyFieldName, new TermsAggregation(propertyFieldName)
                {
                    Field = string.Format("properties.{0}.keyword", propertyFieldName)
                });
            }
        }

        private static void NumericAggregations(AggregationDictionary aggregationDict,
            IEnumerable<string> propertyFieldNames)
        {
            foreach (var propertyFieldName in propertyFieldNames)
            {
                aggregationDict.Add(propertyFieldName + ".max",
                    new MaxAggregation(propertyFieldName, string.Format("properties.{0}", propertyFieldName)));
                aggregationDict.Add(propertyFieldName + ".min",
                    new MinAggregation(propertyFieldName, string.Format("properties.{0}", propertyFieldName)));
                aggregationDict.Add(propertyFieldName + ".avg",
                    new AverageAggregation(propertyFieldName, string.Format("properties.{0}", propertyFieldName)));
            }
        }
    }
}
