using System;
using System.Collections.Generic;
using ClerkBot.Models.Database;
using Nest;

namespace ClerkBot.FilterBuilders
{
    public class FiltersCollections<TC>
        where TC: class, IElasticContract
    {
        public FiltersCollections()
        {
            Must = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
            MustNot = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
            Sort = new SortDescriptor<TC>();
        }

        public SortDescriptor<TC> Sort { get; }
        public List<Func<QueryContainerDescriptor<TC>, QueryContainer>> Must { get; }
        public List<Func<QueryContainerDescriptor<TC>, QueryContainer>> MustNot { get; }
    }
}
