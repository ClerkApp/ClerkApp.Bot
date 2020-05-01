using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClerkBot.Helpers.temp
{
    public interface IAmADialogRepository<T>
    {
        IEnumerable<T> FindDialogsByType(string type);

        T FindDialogByType(string type);

        T TryFindDialogByType(string type);

        IEnumerable<T> GetAllDialogs();
    }
}
