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
    public class BudgetBuilderMobile<TP, TC> where TP : MobileProfile where TC : MobileContract
    {
        private readonly BaseBuilderMobile<TP, TC> BaseBuilder;

        private readonly List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>> RangeCriteria;

        public BudgetBuilderMobile(BaseBuilderMobile<TP, TC> baseBuilder)
        {
            BaseBuilder = baseBuilder;

            RangeCriteria = new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>();
        }

        public void Build()
        {
            GenerateCriteria();

            BaseBuilder.MustCollection.AddRange(RangeCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>) (budget => budget.Range(filter));
            }).ToList());
        }

        private void GenerateCriteria()
        {
            if (BaseBuilder.Profile.BudgetRanges.Contains(BudgetRanges.custom))
            {
                var customBudget = BaseBuilder.Profile.Features.FirstOrDefault(f => 
                    f.GetType().Name.Contains(nameof(MobileProfile.PhoneFeatures.Budget))) as BudgetFeatureMobile;

                if (customBudget is null)
                {
                    return;
                }

                RangeCriteria.AddRange(new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>
                {
                    device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).GreaterThan(customBudget.MinBudget),
                    device => device.Field(f => f.Price[nameof(CurrencyType.EUR)]).LessThan(customBudget.MaxBudget)
                });
            }
            else
            {
                var (minBudget, maxBudget) = new BudgetRangeMobile
                {
                    Budgets = BaseBuilder.Profile.BudgetRanges
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