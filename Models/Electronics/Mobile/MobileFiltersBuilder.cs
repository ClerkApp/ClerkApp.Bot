using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile.Enrichment;
using ClerkBot.Models.Electronics.Mobile.Features;
using Nest;

namespace ClerkBot.Models.Electronics.Mobile
{
    public class MobileFiltersBuilder<TP, TC> : IFiltersBuilder<TP, TC> 
        where TP : MobileProfile
        where TC : MobileContract
    {
        public TP Profile { get; set; }

        private bool BuildersExecuted;
        private SortDescriptor<TC> SortCollection { get; }
        private List<Func<QueryContainerDescriptor<TC>, QueryContainer>> MustCollection { get; }
        private List<Func<QueryContainerDescriptor<TC>, QueryContainer>> MustNotCollection { get; }

        public MobileFiltersBuilder(TP profile)
        {
            Profile = profile;
            MustCollection = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
            MustNotCollection = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
            SortCollection = new SortDescriptor<TC>();
        }

        public Func<QueryContainerDescriptor<TC>, QueryContainer> BuildQuery()
        {
            if (!BuildersExecuted)
            {
                BuildersExecuted = ExecuteBuilders();
            }

            return q => q
                .Bool(bq => bq
                    .MustNot(MustNotCollection)
                    .Must(MustCollection));
        }

        public Func<SortDescriptor<MobileContract>, IPromise<IList<ISort>>> BuildSort()
        {
            if (!BuildersExecuted)
            {
                BuildersExecuted = ExecuteBuilders();
            }

            SortCollection.Descending(de => de.Display.Protection.Value);
            SortCollection.Descending(des => des.Features.DustResistant);
            SortCollection.Descending(desc => desc.Battery.Capacity);

            SortCollection.Descending(SortSpecialField.Score);
            return s => SortCollection;
        }

        private bool ExecuteBuilders()
        {
            BudgetBuilder();
            ReliableBuilder();
            DurabilityBuilder();
            CameraBuilder();
            CallBuilder();

            return true;
        }

        private void CallBuilder()
        {
            SortCollection.Descending(d => d.Camera.Selfie.Lens.First().Size);
        }

        private void BudgetBuilder()
        {
            var (minBudget, maxBudget) = new MobileBudgetRange
            {
                Budgets = Profile.BudgetRanges
            }.GetBudgetRangeInEuro();

            if (minBudget.Equals(0) && maxBudget.Equals(0))
            {
                return;
            }

            var budgetRangeCriteria = new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>
            {
                device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).GreaterThan(minBudget),
                device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).LessThan(maxBudget)
            };

            MustCollection.AddRange(budgetRangeCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>) (budget => budget.Range(filter));
            }).ToList());
        }

        private void ReliableBuilder()
        {
            var reliable = Profile.ReliableBrands;

            if (!reliable) return;

            var brands = Enum.GetNames(typeof(ReliableBrands)).ToList();
            var brandCriteria = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>
            {
                device => device.Terms(t => t.Field(p => p.Name.Brand).Terms(brands))
            };

            MustCollection.AddRange(brandCriteria);
        }

        private void DurabilityBuilder()
        {
            var durability = Profile.Durability;

            var matchCriteria = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>();
            var matchNotCriteria = new List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>>();

            switch (durability.Name)
            {
                case nameof(Intensity.Medium):
                    break;
                case nameof(Intensity.Hard):
                    matchCriteria.Add(device => device.Field(f => f.Features.DustResistant).Query(bool.TrueString.ToLower()));
                    matchNotCriteria.Add(device => device.Field(f => f.Body.Build).Query("glass back"));
                    break;
                case nameof(Intensity.ExtraHard):
                    matchCriteria.Add(device => device.Field(f => f.Features.DustResistant).Query(bool.TrueString.ToLower()));
                    //matchCriteria.Add(device => device.Field(f => f.Body.Build).Query("steel").Operator(Operator.Or).Query("aluminum"));
                    matchNotCriteria.Add(device => device.Field(f => f.Body.Build).Query("glass back"));
                    break;
            }

            MustCollection.AddRange(matchCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(b => b.Match(filter));
            }).ToList());

            MustNotCollection.AddRange(matchNotCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(b => b.MatchPhrase(filter));
            }).ToList());
        }

        private void CameraBuilder()
        {
            var matchCriteria = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>();
            var matchPhraseCriteria = new List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>>();
            var rangeCriteria = new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>();

            var mobileFeature = Profile.Features.FirstOrDefault(f => f.GetType().Name.ToLower().Contains(nameof(MobileProfile.PhoneFeatures.camera)));

            if (mobileFeature is null)
            {
                return;
            }

            var camera = mobileFeature as CameraMobileFeature;

            if (camera.SelfieMode)
            {
                matchPhraseCriteria.Add(device => device.Field(f => f.Camera.Selfie.Features).Query("hdr"));

                SortCollection.Descending(d => d.Camera.Selfie.Lens.First().Size);
                SortCollection.Ascending(d => d.Camera.Selfie.Lens.First().Aperture);
            }

            if (camera.NightMode.Value >= Periodicity.Occasionally.Value)
            {
                matchPhraseCriteria.Add(device => device.Field(f => f.Camera.Main.Features).Query("hdr"));

                if(camera.NightMode.Value >= Periodicity.Weekly.Value)
                {
                    SortCollection.Ascending(d => d.Camera.Main.Lens.First().Aperture);

                    if (camera.NightMode.Value >= Periodicity.Daily.Value)
                    {
                        SortCollection.Descending(d => d.Camera.Main.Lens.First().Micro);
                        SortCollection.Descending(d => d.Camera.Main.Lens.First().Size);
                    }
                }
            }

            if (camera.Print)
            {
                SortCollection.Descending(d => d.Camera.Main.Lens.First().Megapixels);
            }

            if (camera.KResolution)
            {
                rangeCriteria.Add(
                    device => device.Field(f => f.Camera.Main.Videos.First().Value).GreaterThan(0));
                rangeCriteria.Add(
                    device => device.Field(f => f.Camera.Main.Videos.First().Value).LessThan(100));
            }

            if(camera.RecordTypes.Value >= Intensity.Medium.Value)
            {
                matchCriteria.Add(device => device.Field(f => f.Features.Gyro).Query(bool.TrueString.ToLower()));
                matchCriteria.Add(device => device.Field(f => f.Memory.CardSlot).Query(bool.TrueString.ToLower()));

                if (camera.NightMode.Value >= Intensity.ExtraHard.Value)
                {
                    matchPhraseCriteria.Add(device => device.Field(f => f.Camera.Main.Videos.First().Name).Query("60"));
                }
            }
            
            MustCollection.AddRange(matchPhraseCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.MatchPhrase(filter));
            }).ToList());

            MustCollection.AddRange(matchCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Match(filter));
            }).ToList());

            MustCollection.AddRange(rangeCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Range(filter));
            }).ToList());
        }
    }
}
