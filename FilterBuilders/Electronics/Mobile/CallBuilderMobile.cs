using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Features;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class CallBuilderMobile<TP, TC>: IChildBuilder
        where TP : MobileProfile
        where TC : MobileContract
    {
        private readonly CallMobileFeature CallMobile;
        private readonly FiltersCollections<TC> FiltersBuilder;

        private readonly List<Func<MatchQueryDescriptor<TC>, IMatchQuery>> MatchCriteria;

        public CallBuilderMobile(TP profile, FiltersCollections<TC> filtersBuilder)
        {
            FiltersBuilder = filtersBuilder;
            CallMobile = profile.Features.FirstOrDefault(f => 
                f.GetType().Name.Contains(nameof(MobileProfile.PhoneFeatures.Call))) as CallMobileFeature;

            MatchCriteria = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>();
        }

        public void Build()
        {
            if (CallMobile is null)
            {
                return;
            }

            GenerateCriteria();

            FiltersBuilder.Must.AddRange(MatchCriteria.Select(filter =>
                (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Match(filter))
            ).ToList());
        }

        private void GenerateCriteria()
        {
            FiltersBuilder.Sort.Descending(d => d.Network.Band.TotalBands);
            FiltersBuilder.Sort.Descending(d => d.Network.Technology.TotalTechs);
            FiltersBuilder.Sort.Descending(d => d.Battery.Capacity);

            MatchCriteria.Add(device => device.Field(f => f.Battery.Fast).Query(bool.TrueString.ToLower()));
        }
    }
}