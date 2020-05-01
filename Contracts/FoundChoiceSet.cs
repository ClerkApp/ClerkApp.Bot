using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ClerkBot.Contracts
{
    public static class FoundChoiceSet
    {
        /// <summary>
        /// Represents a result from matching user input against a list of choice set from an adaptive card prompt.
        /// </summary>
        public static List<string> TryGetChoiceSet(this IDictionary<string, object> selectedChoises, string propertyKey)
        {
            selectedChoises.TryGetValue(propertyKey, out var choises);

            if (choises != null)
            {
                var formatData = choises.ToString();
                return formatData.TryGetValues(propertyKey);
            }

            return new List<string>();
        }

        public static List<string> TryGetValues(this string choises, string propertyKey)
        {
            var list = new List<string>();

            try
            {
                var jsonObject = JToken.Parse(choises);
                list.AddRange(jsonObject[propertyKey].ToString().Split(','));
            }
            catch (Exception)
            {
                list.Add(choises);
            }

            return list;
        }
    }
}
