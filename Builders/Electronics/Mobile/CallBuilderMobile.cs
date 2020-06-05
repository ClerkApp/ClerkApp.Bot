using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Features;
using Nest;

namespace ClerkBot.Builders.Electronics.Mobile
{
    public class CallBuilderMobile<TP, TC> where TP : MobileProfile where TC : MobileContract
    {
        private readonly BaseBuilderMobile<TP, TC> BaseBuilder;

        private readonly List<Func<MatchQueryDescriptor<TC>, IMatchQuery>> MatchCriteria;

        public CallBuilderMobile(BaseBuilderMobile<TP, TC> baseBuilder)
        {
            BaseBuilder = baseBuilder;

            MatchCriteria = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>();
        }

        public void Build()
        {
            var feature = BaseBuilder.Profile.Features.FirstOrDefault(f => 
                f.GetType().Name.Contains(nameof(MobileProfile.PhoneFeatures.Call))) as CallMobileFeature;

            if (feature is null)
            {
                return;
            }

            GenerateCriteria(feature);

            BaseBuilder.MustCollection.AddRange(MatchCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Match(filter));
            }).ToList());
        }

        private void GenerateCriteria(CallMobileFeature feature)
        {
            BaseBuilder.SortCollection.Descending(d => d.Network.Band.TotalBands);
            BaseBuilder.SortCollection.Descending(d => d.Network.Technology.TotalTechs);
            BaseBuilder.SortCollection.Descending(d => d.Battery.Capacity);

            MatchCriteria.Add(device => device.Field(f => f.Battery.Fast).Query(bool.TrueString.ToLower()));
        }
    }
}