using System;
using System.Collections.Generic;
using ClerkBot.Models.Database;
using Nest;

namespace ClerkBot.Builders
{
    public interface IFiltersBuilder<P, C>
        where P: class, IUserProfile
        where C: class, IElasticContract
    {
        public P Profile { get; set; }
        public Func<QueryContainerDescriptor<C>, QueryContainer> GetQuery();
        public Func<SortDescriptor<C>, IPromise<IList<ISort>>> GetSort();
    }
}
