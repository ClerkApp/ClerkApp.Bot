using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Features;
using Nest;

namespace ClerkBot.Builders.Electronics.Mobile
{
    public class GamingBuilderMobile<TP, TC> where TP : MobileProfile where TC : MobileContract
    {
        private readonly BaseBuilderMobile<TP, TC> BaseBuilder;

        private readonly List<Func<MatchQueryDescriptor<TC>, IMatchQuery>> MatchCriteria;
        private readonly List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>> MatchPhraseCriteria;
        private readonly List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>> RangeCriteria;

        public GamingBuilderMobile(BaseBuilderMobile<TP, TC> baseBuilder)
        {
            BaseBuilder = baseBuilder;

            MatchCriteria = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>();
            MatchPhraseCriteria = new List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>>();
            RangeCriteria = new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>();
        }

        public void Build()
        {
            var feature = BaseBuilder.Profile.Features.FirstOrDefault(f => 
                f.GetType().Name.Contains(nameof(MobileProfile.PhoneFeatures.Gaming))) as GamingFeatureMobile;

            if (feature is null)
            {
                return;
            }

            GenerateCriteria(feature);

            BaseBuilder.MustCollection.AddRange(MatchPhraseCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.MatchPhrase(filter));
            }).ToList());

            BaseBuilder.MustCollection.AddRange(MatchCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Match(filter));
            }).ToList());

            BaseBuilder.MustCollection.AddRange(RangeCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Range(filter));
            }).ToList());
        }

        private void GenerateCriteria(GamingFeatureMobile feature)
        {
            if (feature.GamingIntensity.Value >= Intensity.Soft.Value)
            {
                BaseBuilder.SortCollection.Descending(d => d.Platform.Cpu["global"].First().Cores);

                if (feature.GamingIntensity.Value >= Intensity.Medium.Value)
                {
                    BaseBuilder.SortCollection.Descending(d => d.Platform.Tests["antutu"]);

                    if (feature.GamingIntensity.Value >= Intensity.Hard.Value)
                    {
                        BaseBuilder.SortCollection.Ascending(d => d.Platform.Chipset["global"].First().Size);

                        BaseBuilder.SortCollection.Descending(d => d.Memory.Internals.First().Ram);
                        BaseBuilder.SortCollection.Descending(d => d.Memory.Internals.First().Size);
                        BaseBuilder.SortCollection.Descending(d => d.Display.RefreshRate.First());
                        BaseBuilder.SortCollection.Descending(d => d.Display.Resolution.Density);
                        BaseBuilder.SortCollection.Descending(d => d.Display.Resolution.Height);
                        BaseBuilder.SortCollection.Descending(d => d.Display.Resolution.Weight);

                        BaseBuilder.SortCollection.Descending(d => d.Platform.Chipset["global"].First().Generation);
                    }
                }
            }
        }
    }
}