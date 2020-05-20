using System.Collections.Generic;
using System.Linq;
using ClerkBot.Helpers.ElasticHelpers.Models;
using Nest;

namespace ClerkBot.Helpers.ElasticHelpers
{
    public static class ElasticQueryHelper
    {
        public static QueryContainer QueryBuilder(string queryString,
            string lang,
            Dictionary<string, string> stringProperties,
            Dictionary<string, NumericQuery> numericProperties,
            string category)
        {
            var qc = new QueryContainer();
            qc = qc &= TextQuery(queryString, lang);
            if (stringProperties != null && stringProperties.Count > 0)
            {
                qc = qc &= StringPropertiesQuery(stringProperties);
            }
            if (numericProperties != null && numericProperties.Count > 0)
            {
                qc = qc &= NumericPropertiesQuery(numericProperties);
            }
            if (category != null)
            {
                qc &= CategoryQuery(category);
            }
            return qc;
        }

        private static MultiMatchQuery TextQuery(string queryString, string lang)
        {
            return new MultiMatchQuery()
            {
                Fields = new List<string>()
                {
                    "id",
                    string.Format("shortDescription.{0}", lang),
                    string.Format("longDescription.{0}", lang)
                }.ToArray(),
                Query = queryString,
                Operator = Operator.Or,
                Fuzziness = Fuzziness.Auto
            };
        }

        private static BoolQuery StringPropertiesQuery(Dictionary<string, string> stringProperties)
        {
            return new BoolQuery()
            {
                Must = stringProperties.Select(p => StringFieldMatch(p.Key, p.Value)).ToArray()
            };
        }

        private static BoolQuery NumericPropertiesQuery(Dictionary<string, NumericQuery> numericProperties)
        {
            return new BoolQuery()
            {
                Must = numericProperties.Select(p => NumericFieldMatch(p.Key, p.Value)).ToArray()
            };
        }

        private static BoolQuery CategoryQuery(string categoryId)
        {
            return new BoolQuery()
            {
                Must = new List<QueryContainer>()
                {
                    StringFieldMatch("categories", categoryId)
                }
            };
        }

        private static QueryContainer StringFieldMatch(string field, string value)
        {
            return new MatchQuery()
            {
                Field = "properties." + field,
                Query = value
            };
        }

        private static QueryContainer NumericFieldMatch(string field, NumericQuery value)
        {
            return new NumericRangeQuery()
            {
                Field = "properties." + field,
                GreaterThanOrEqualTo = value.GreaterThanOrEqual,
                LessThanOrEqualTo = value.LessThanOrEqual
            };
        }
    }
}
