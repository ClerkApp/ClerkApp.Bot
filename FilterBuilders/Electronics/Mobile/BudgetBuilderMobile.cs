using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enrichment.Electronics.Mobile;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Features;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class BudgetBuilderMobile<TP, TC>: IChildBuilder
        where TP : MobileProfile
        where TC : MobileContract
    {
        private readonly List<BudgetRanges> Budget;
        private readonly BudgetFeatureMobile BudgetFeature;
        private readonly FiltersCollections<TC> FiltersBuilder;

        private readonly List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>> RangeCriteria;

        public BudgetBuilderMobile(TP profile, FiltersCollections<TC> filtersBuilder)
        {
            FiltersBuilder = filtersBuilder;
            Budget = profile.BudgetRanges;
            BudgetFeature = profile.Features.FirstOrDefault(f => 
                f.GetType().Name.Contains(nameof(MobileProfile.PhoneFeatures.Budget))) as BudgetFeatureMobile;

            RangeCriteria = new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>();
        }

        public void Build()
        {
            GenerateCriteria();

            FiltersBuilder.Must.AddRange(RangeCriteria.Select(filter =>
                (Func<QueryContainerDescriptor<TC>, QueryContainer>) (budget => budget.Range(filter))
            ).ToList());
        }

        private void GenerateCriteria()
        {
            if (Budget.Contains(BudgetRanges.custom))
            {
                if (BudgetFeature is null)
                {
                    return;
                }

                RangeCriteria.AddRange(new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>
                {
                    device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).GreaterThan(BudgetFeature.MinBudget),
                    device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).LessThan(BudgetFeature.MaxBudget)
                });
            }
            else
            {
                var (minBudget, maxBudget) = new BudgetRangeMobile
                {
                    Budgets = Budget
                }.GetBudgetRangeInEuro();

                if (minBudget.Equals(0) && maxBudget.Equals(0))
                {
                    return;
                }

                RangeCriteria.AddRange(new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>
                {
                    device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).GreaterThan(minBudget),
                    device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).LessThan(maxBudget)
                });
            }
        }
    }
}