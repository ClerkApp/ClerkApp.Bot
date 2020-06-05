using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enrichment.Electronics.Mobile.Enums;
using ClerkBot.Models.Electronics.Mobile;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class ReliableBuilderMobile<TP, TC> where TP : MobileProfile where TC : MobileContract
    {
        private readonly BaseBuilderMobile<TP, TC> BaseBuilder;

        private readonly List<Func<QueryContainerDescriptor<TC>, QueryContainer>> QueryCriteria;

        public ReliableBuilderMobile(BaseBuilderMobile<TP, TC> baseBuilder)
        {
            BaseBuilder = baseBuilder;

            QueryCriteria = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
        }

        public void Build()
        {
            GenerateCriteria();

            BaseBuilder.MustCollection.AddRange(QueryCriteria);
        }

        private void GenerateCriteria()
        {
            var reliable = BaseBuilder.Profile.ReliableBrands;

            if (reliable)
            {
                var brands = Enum.GetNames(typeof(ReliableBrands)).ToList();
                QueryCriteria.Add(device => device.Terms(t => 
                    t.Field(p => p.Name.Brand).Terms(brands)));
            }
        }
    }
}