using System;
using System.Collections.Generic;
using System.Linq;

namespace ClerkBot.Helpers.temp
{
    public class DialogRepository<T, TMetaDataView> : IAmADialogRepository<T>
          where TMetaDataView : IAmDialogTypeMetaData
    {
        private static readonly IEnumerable<Lazy<T, TMetaDataView>> allDialogs;

        static DialogRepository()
        {
            allDialogs = DialogContainerHelper.Container.GetExports<T, TMetaDataView>();
        }

        public IEnumerable<T> FindDialogsByType(string type)
        {
            List<T> results;

            try
            {
                results = allDialogs
                   .Where(p => p.Metadata.DialogType.Split(',').Any(c => string.Equals(c.Trim(), type,
                      StringComparison.CurrentCultureIgnoreCase)))
                   .Select(lazy => lazy.Value).ToList();
            }
            catch (Exception ex)
            {
                throw new DllNotFoundException(
                   $"Dialog container is not initialized. DialogType '{type}' Exception: '{ex.Message}'", ex);
            }

            var duplicateTypes = results.GroupBy(x => x.GetType().FullName)
               .Where(g => g.Count() > 1)
               .Select(y => y.Key)
               .ToList();

            if (duplicateTypes.Any())
            {
                var duplicateDialogs =
                   (from ep in results where duplicateTypes.Contains(ep.GetType().FullName) select ep).ToList();

                List<string> dupes = (from dupe in duplicateDialogs
                                      select $"Type:'{dupe.GetType().FullName}' Location:'{dupe.GetType().Assembly.Location}'").ToList();

                throw new Exception(
                   $"{nameof(FindDialogsByType)}:type returned duplicate Dialogs: {string.Join(",", dupes)} ");
            }

            return results;

        }

        public T TryFindDialogByType(string type)
        {
            var results = FindDialogsByType(type).ToList();
            if (results.Count > 1)
            {
                var multiResults = (from r in results select r.GetType().FullName).ToList();
                throw new Exception($"Found multiple Dialogs for Type:{type}: {string.Join(",", multiResults)}");
            }

            return results.FirstOrDefault();
        }

        public T FindDialogByType(string type)
        {
            var result = TryFindDialogByType(type);

            if (result == null)
            {
                throw new Exception($"Failed to find a Dialog for '{typeof(T).Name}' matching type:'{type}'. " +
                                    $"Please ensure '{type}' is defined in a Component or Service or in an assembly containing the terms 'Beazley' and 'Dialogs' in the assembly name.");
            }

            return result;
        }

        public IEnumerable<T> GetAllDialogs()
        {
            try
            {
                var exportedDialogs = allDialogs
                   .Select(lazy => lazy.Value);
                return exportedDialogs;
            }
            catch (Exception ex)
            {
                throw new DllNotFoundException($"Dialog container is not initialized. Exception: {ex.Message}", ex);
            }
        }
    }
}
