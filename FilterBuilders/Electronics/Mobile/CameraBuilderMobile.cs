using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Features;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class CameraBuilderMobile<TP, TC> where TP : MobileProfile where TC : MobileContract
    {
        private readonly BaseBuilderMobile<TP, TC> BaseBuilder;

        private readonly List<Func<MatchQueryDescriptor<TC>, IMatchQuery>> MatchCriteria;
        private readonly List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>> MatchPhraseCriteria;
        private readonly List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>> RangeCriteria;

        public CameraBuilderMobile(BaseBuilderMobile<TP, TC> baseBuilder)
        {
            BaseBuilder = baseBuilder;

            MatchCriteria = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>();
            MatchPhraseCriteria = new List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>>();
            RangeCriteria = new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>();
        }

        public void Build()
        {
            var feature = BaseBuilder.Profile.Features.FirstOrDefault(f => 
                f.GetType().Name.Contains(nameof(MobileProfile.PhoneFeatures.Camera))) as CameraFeatureMobile;

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

        private void GenerateCriteria(CameraFeatureMobile feature)
        {
            if (feature.SelfieMode)
            {
                MatchPhraseCriteria.Add(device => device.Field(f => f.Camera.Selfie.Features).Query("hdr"));

                BaseBuilder.SortCollection.Descending(d => d.Camera.Selfie.Lens.First().Size);
                BaseBuilder.SortCollection.Ascending(d => d.Camera.Selfie.Lens.First().Aperture);
            }

            if (feature.NightMode.Value >= Periodicity.Occasionally.Value)
            {
                MatchPhraseCriteria.Add(device => device.Field(f => f.Camera.Main.Features).Query("hdr"));

                if(feature.NightMode.Value >= Periodicity.Weekly.Value)
                {
                    BaseBuilder.SortCollection.Ascending(d => d.Camera.Main.Lens.First().Aperture);

                    if (feature.NightMode.Value >= Periodicity.Daily.Value)
                    {
                        BaseBuilder.SortCollection.Descending(d => d.Camera.Main.Lens.First().Micro);
                        BaseBuilder.SortCollection.Descending(d => d.Camera.Main.Lens.First().Size);
                    } 
                }
            }

            if (feature.Print)
            {
                BaseBuilder.SortCollection.Descending(d => d.Camera.Main.Lens.First().Megapixels);
            }

            if (feature.KResolution)
            {
                RangeCriteria.Add(
                    device => device.Field(f => f.Camera.Main.Videos.First().Value).GreaterThan(0));
                RangeCriteria.Add(
                    device => device.Field(f => f.Camera.Main.Videos.First().Value).LessThan(50));
            }

            if(feature.RecordTypes.Value >= Intensity.Medium.Value)
            {
                if (feature.KResolution)
                {
                    MatchCriteria.Add(device => device.Field(f => f.Memory.CardSlot).Query(bool.TrueString.ToLower()));
                }

                MatchCriteria.Add(device => device.Field(f => f.Features.Gyro).Query(bool.TrueString.ToLower()));

                if (feature.NightMode.Value >= Intensity.ExtraHard.Value)
                {
                    MatchPhraseCriteria.Add(device => device.Field(f => f.Camera.Main.Videos.First().Name).Query("60"));
                }
            }
        }
    }
}