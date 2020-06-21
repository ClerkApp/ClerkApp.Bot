using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enrichment.Electronics.Mobile.Enums;
using ClerkBot.Models.Electronics.Mobile;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class ReliableBuilderMobile<TP, TC>: IChildBuilder
        where TP : MobileProfile
        where TC : MobileContract
    {
        private readonly bool ReliableBrands;
        private readonly FiltersCollections<TC> FiltersBuilder;

        private readonly List<Func<QueryContainerDescriptor<TC>, QueryContainer>> QueryCriteria;

        public ReliableBuilderMobile(TP profile, FiltersCollections<TC> filtersBuilder)
        {
            FiltersBuilder = filtersBuilder;
            ReliableBrands = profile.ReliableBrands;

            QueryCriteria = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
        }

        public void Build()
        {
            GenerateCriteria();

            FiltersBuilder.Must.AddRange(QueryCriteria);
        }

        private void GenerateCriteria()
        {
            if (ReliableBrands)
            {
                var brands = Enum.GetNames(typeof(ReliableBrands)).ToList();
                QueryCriteria.Add(device => device.Terms(t => 
                    t.Field(p => p.Name.Brand).Terms(brands)));
            }
        }
    }
}