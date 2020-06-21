using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Features;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class CameraBuilderMobile<TP, TC>: IChildBuilder
        where TP: MobileProfile
        where TC: MobileContract
    {
        private readonly CameraFeatureMobile CameraFeature;
        private readonly FiltersCollections<TC> FiltersBuilder;

        private readonly List<Func<MatchQueryDescriptor<TC>, IMatchQuery>> Match;
        private readonly List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>> MatchPhrase;
        private readonly List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>> Range;

        public CameraBuilderMobile(TP profile, FiltersCollections<TC> filtersBuilder)
        {
            FiltersBuilder = filtersBuilder;
            CameraFeature = profile.Features.FirstOrDefault(f => 
                f.GetType().Name.Contains(nameof(MobileProfile.PhoneFeatures.Camera))) as CameraFeatureMobile;

            Match = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>();
            MatchPhrase = new List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>>();
            Range = new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>();
        }

        public void Build()
        {
            if (CameraFeature is null)
            {
                return;
            }

            GenerateCriteria();

            FiltersBuilder.Must.AddRange(MatchPhrase.Select(filter => 
                (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.MatchPhrase(filter))
            ).ToList());

            FiltersBuilder.Must.AddRange(Match.Select(filter =>
                (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Match(filter))
            ).ToList());

            FiltersBuilder.Must.AddRange(Range.Select(filter =>
                (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Range(filter))
            ).ToList());
        }

        private void GenerateCriteria()
        {
            if (CameraFeature.SelfieMode)
            {
                MatchPhrase.Add(device => device.Field(f => f.Camera.Selfie.Features).Query("hdr"));

                FiltersBuilder.Sort.Descending(d => d.Camera.Selfie.Lens.First().Size);
                FiltersBuilder.Sort.Ascending(d => d.Camera.Selfie.Lens.First().Aperture);
            }

            if (CameraFeature.NightMode.Value >= Periodicity.Occasionally.Value)
            {
                MatchPhrase.Add(device => device.Field(f => f.Camera.Main.Features).Query("hdr"));

                if(CameraFeature.NightMode.Value >= Periodicity.Weekly.Value)
                {
                    FiltersBuilder.Sort.Ascending(d => d.Camera.Main.Lens.First().Aperture);

                    if (CameraFeature.NightMode.Value >= Periodicity.Daily.Value)
                    {
                        FiltersBuilder.Sort.Descending(d => d.Camera.Main.Lens.First().Micro);
                        FiltersBuilder.Sort.Descending(d => d.Camera.Main.Lens.First().Size);
                    } 
                }
            }

            if (CameraFeature.Print)
            {
                FiltersBuilder.Sort.Descending(d => d.Camera.Main.Lens.First().Megapixels);
            }

            if (CameraFeature.KResolution)
            {
                Range.Add(
                    device => device.Field(f => f.Camera.Main.Videos.First().Value).GreaterThan(0));
                Range.Add(
                    device => device.Field(f => f.Camera.Main.Videos.First().Value).LessThan(50));
            }

            if (CameraFeature.RecordTypes.Value >= Intensity.Medium.Value)
            {
                if (CameraFeature.KResolution)
                {
                    Match.Add(device => device.Field(f => f.Memory.CardSlot).Query(bool.TrueString.ToLower()));
                }

                Match.Add(device => device.Field(f => f.Features.Gyro).Query(bool.TrueString.ToLower()));

                if (CameraFeature.NightMode.Value >= Intensity.ExtraHard.Value)
                {
                    MatchPhrase.Add(device => device.Field(f => f.Camera.Main.Videos.First().Name).Query("60"));
                }
            }
        }
    }
}