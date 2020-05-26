using System;
using Nest;

namespace ClerkBot.Models
{
    public interface IFiltersBuilder<P, C>
        where P: class, IUserProfile
        where C: class, IElasticContract
    {
        public P Profile { get; set; }
        public Func<QueryContainerDescriptor<C>, QueryContainer> BuildQuery();
    }
}
