using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile.Enrichment;
using Nest;

namespace ClerkBot.Models.Electronics.Mobile
{
    public class MobileFiltersBuilder<P, C> : IFiltersBuilder<P, C> 
        where P : MobileProfile
        where C : MobileContract
    {
        public P Profile { get; set; }

        private List<Func<QueryContainerDescriptor<C>, QueryContainer>> MustCollection { get; }
        private List<Func<QueryContainerDescriptor<C>, QueryContainer>> MustNotCollection { get; }
        private List<Func<TermQueryDescriptor<C>, ITermQuery>> TermsCollection { get; }

        public MobileFiltersBuilder(P profile)
        {
            Profile = profile;
            MustCollection = new List<Func<QueryContainerDescriptor<C>, QueryContainer>>();
            MustNotCollection = new List<Func<QueryContainerDescriptor<C>, QueryContainer>>();
            TermsCollection = new List<Func<TermQueryDescriptor<C>, ITermQuery>>();
        }

        public Func<QueryContainerDescriptor<C>, QueryContainer> BuildQuery()
        {
            BudgetBuilder();
            //BrandBuilder();
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
            var (minBuget, maxBuget) = new MobileBugetRange
            {
                Budgets = Profile.BugetRanges
            }.GetBudgetRangeInEuro();

            if (minBuget.Equals(0) && maxBuget.Equals(0))
            {
                return;
            }

            var bugetCriteria = new List<Func<NumericRangeQueryDescriptor<C>, INumericRangeQuery>>
            {
                device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).GreaterThan(minBuget),
                device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).LessThan(maxBuget)
            };

            MustCollection.AddRange(bugetCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<C>, QueryContainer>)(buget => buget.Range(filter));
            }).ToList());
        }

        private void BrandBuilder()
        {
            var brand = Profile.Brands.First();

            if (!brand.Equals("None"))
            {
                var brandCriteria = new List<Func<MatchQueryDescriptor<C>, IMatchQuery>>
                {
                    device => device.Field(f => f.Name.Brand).Query(brand)
                };

                MustCollection.AddRange(brandCriteria.Select(filter =>
                {
                    return (Func<QueryContainerDescriptor<C>, QueryContainer>)(b => b.Match(filter));
                }).ToList());
            }
        }

        private void DurabilityBuilder()
        {
            var durability = Profile.Durability;

            var matchCriteria = new List<Func<MatchQueryDescriptor<C>, IMatchQuery>>();
            var matchNotCriteria = new List<Func<MatchPhraseQueryDescriptor<C>, IMatchPhraseQuery>>();

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
                return (Func<QueryContainerDescriptor<C>, QueryContainer>)(b => b.Match(filter));
            }).ToList());

            MustNotCollection.AddRange(matchNotCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<C>, QueryContainer>)(b => b.MatchPhrase(filter));
            }).ToList());
        }
    }
}
