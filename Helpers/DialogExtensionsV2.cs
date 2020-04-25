using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;

namespace ClerkBot.Helpers
{
    public static class DialogExtensionsV2
    {
        private const string PersistedValues = "values";

        /// <summary>
        /// Helper function to deal with the persisted values we collect in this dialog.
        /// </summary>
        /// <param name="dialogInstance">A handle on the runtime instance associated with this dialog, the State is a property.</param>
        /// <returns>A dictionary representing the current state or a new dictionary if we have none.</returns>
        public static IDictionary<string, object> GetPersistedValues(this DialogInstance dialogInstance)
        {
            if (!dialogInstance.State.TryGetValue(PersistedValues, out var obj))
            {
                obj = new Dictionary<string, object>();
                dialogInstance.State.Add(PersistedValues, obj);
            }

            return (IDictionary<string, object>)obj;
        }
    }
}
