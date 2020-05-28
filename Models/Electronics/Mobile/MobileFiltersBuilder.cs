using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile.Enrichment;
using Nest;

namespace ClerkBot.Models.Electronics.Mobile
{
    public class MobileFiltersBuilder<TP, TC> : IFiltersBuilder<TP, TC> 
        where TP : MobileProfile
        where TC : MobileContract
    {
        public TP Profile { get; set; }

        private List<Func<QueryContainerDescriptor<TC>, QueryContainer>> MustCollection { get; }
        private List<Func<QueryContainerDescriptor<TC>, QueryContainer>> MustNotCollection { get; }
        private List<Func<TermQueryDescriptor<TC>, ITermQuery>> TermsCollection { get; }

        public MobileFiltersBuilder(TP profile)
        {
            Profile = profile;
            MustCollection = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
            MustNotCollection = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
            TermsCollection = new List<Func<TermQueryDescriptor<TC>, ITermQuery>>();
        }

        public Func<QueryContainerDescriptor<TC>, QueryContainer> BuildQuery()
        {
            BudgetBuilder();
            ReliableBuilder();
            DurabilityBuilder();

            return q => q
                .Bool(bq => bq
                    .MustNot(MustNotCollection)
                    .Must(MustCollection));
        }

        public Func<SortDescriptor<MobileContract>, IPromise<IList<ISort>>> BuildSort()
        {
            return s => s.Descending(d => d.Display.Protection.Value);
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
                device => device.Field(f => f.Price.First().Value).GreaterThan(minBudget),
                device => device.Field(f => f.Price.First().Value).LessThan(maxBudget),
            };
            var budgetTypeCriteria = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>
            {
                device => device.Field(f => f.Price.First().Type).Query(nameof(CurrencyType.EUR))
            };

            MustCollection.AddRange(budgetRangeCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Range(filter));
            }).ToList());
            MustCollection.AddRange(budgetTypeCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(b => b.Match(filter));
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
    }
}
