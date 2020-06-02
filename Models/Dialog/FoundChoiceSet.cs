using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json.Linq;

namespace ClerkBot.Models.Dialog
{
    public static class FoundChoiceSet
    {
        /// <summary>
        /// Represents a result from matching user input against a list of choice set from an adaptive card prompt.
        /// </summary>
        public static bool TryGetChoiceSet(this IDictionary<string, object> selectedChoices, string propertyKey, out List<string> resultList)
        {
            resultList = new List<string>();

            selectedChoices.TryGetValue(propertyKey, out var choices);

            if (choices != null)
            {
                var formatData = choices.ToString();
                resultList.AddRange(formatData.TryGetValues(propertyKey));
                return true;
            }

            return false;
        }

        public static bool TryGetFoundChoice(this IDictionary<string, object> selectedChoices, string propertyKey, out FoundChoice foundChoice)
        {
            foundChoice = new FoundChoice();
            selectedChoices.TryGetValue(propertyKey, out var choice);

            if (choice != null)
            {
                foundChoice = choice as FoundChoice;
                return true;
            }

            return false;
        }

        public static List<string> TryGetValues(this string choices, string propertyKey)
        {
            var list = new List<string>();

            try
            {
                var jsonObject = JToken.Parse(choices);
                list.AddRange(jsonObject[propertyKey].ToString().Split(','));
            }
            catch (Exception)
            {
                list.AddRange(choices.Split(','));
            }

            return list;
        }
    }
}
