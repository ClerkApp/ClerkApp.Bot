using System;
using System.Collections.Generic;
using ClerkBot.Models.Database;
using Nest;

namespace ClerkBot.FilterBuilders
{
    public interface IFiltersBuilder<C> where C: class, IElasticContract
    {
        Func<QueryContainerDescriptor<C>, QueryContainer> GetQuery();
        Func<SortDescriptor<C>, IPromise<IList<ISort>>> GetSort();
    }
}
