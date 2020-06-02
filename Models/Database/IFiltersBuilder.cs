using System;
using Nest;

namespace ClerkBot.Models.Database
{
    public interface IFiltersBuilder<P, C>
        where P: class, IUserProfile
        where C: class, IElasticContract
    {
        public P Profile { get; set; }
        public Func<QueryContainerDescriptor<C>, QueryContainer> BuildQuery();
    }
}
