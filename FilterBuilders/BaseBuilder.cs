using System;
using System.Collections.Generic;
using ClerkBot.Models.Database;
using Nest;

namespace ClerkBot.FilterBuilders
{
    public abstract class BaseBuilder<TP, TC> : IFiltersBuilder<TC> 
        where TP: class, IUserProfile
        where TC: class, IElasticContract
    {
        protected BaseBuilder()
        {
            ChildBuilders = new List<IChildBuilder>();
            FiltersCollections = new FiltersCollections<TC>();
        }

        internal List<IChildBuilder> ChildBuilders;
        internal FiltersCollections<TC> FiltersCollections;

        public abstract Func<QueryContainerDescriptor<TC>, QueryContainer> GetQuery();
        public abstract Func<SortDescriptor<TC>, IPromise<IList<ISort>>> GetSort();
    }
}