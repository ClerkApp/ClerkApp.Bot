using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClerkBot.Helpers.temp
{
    internal static class PluginContainerHelper
    {
        static PluginContainerHelper()
        {
            var catalog = new SafeDirectoryCatalog();
            Container = new CompositionContainer(catalog, true);
            if (Container == null)
            {
                throw new Exception("Could not initialize plugin container");
            }
        }

        public static CompositionContainer Container { get; }
    }
}
