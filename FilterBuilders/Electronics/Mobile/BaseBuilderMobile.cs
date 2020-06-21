using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClerkBot.Models.Electronics.Mobile;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class BaseBuilderMobile<TP, TC> : BaseBuilder<TP, TC> 
        where TP: MobileProfile
        where TC: MobileContract
    {
        public BaseBuilderMobile(TP profile)
        {
            ChildBuilders = new List<IChildBuilder>
            {
                new CameraBuilderMobile<TP, TC>(profile, FiltersCollections),
                new GamingBuilderMobile<TP, TC>(profile, FiltersCollections),
                new CallBuilderMobile<TP, TC>(profile, FiltersCollections),
                new DurabilityBuilderMobile<TP, TC>(profile, FiltersCollections),
                new ReliableBuilderMobile<TP, TC>(profile, FiltersCollections),
                new BudgetBuilderMobile<TP, TC>(profile, FiltersCollections),
                new AspectBuilderMobile<TP, TC>(profile, FiltersCollections)
            };

            ExecuteBuilders();
        }

        public override Func<QueryContainerDescriptor<TC>, QueryContainer> GetQuery()
        {
            return query => query
                .Bool(boolQuery => boolQuery
                    .MustNot(FiltersCollections.MustNot)
                    .Must(FiltersCollections.Must));
        }

        public override Func<SortDescriptor<TC>, IPromise<IList<ISort>>> GetSort()
        {
            ApplyBaseSorting();
            return sort => FiltersCollections.Sort;
        }

        private void ExecuteBuilders()
        {
            Parallel.ForEach(ChildBuilders, childBuilder =>
            {
                childBuilder.Build();
            });
        }

        private void ApplyBaseSorting()
        {
            FiltersCollections.Sort.Descending(SortSpecialField.Score);
            
            FiltersCollections.Sort.Descending(desc => desc.Display.Protection.Value);
            FiltersCollections.Sort.Descending(desc => desc.Features.DustResistant);
            FiltersCollections.Sort.Descending(desc => desc.Battery.Capacity);
        }
    }
}
