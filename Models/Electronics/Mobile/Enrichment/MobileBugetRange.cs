using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;

namespace ClerkBot.Models.Electronics.Mobile.Enrichment
{
    public class MobileBudgetRange: IBudgetRange
    {
        public List<BudgetRanges> Budgets { get; set; }

        public (int, int) GetBudgetRangeInEuro()
        {
            var min = new List<int>();
            var max = new List<int>();
            foreach (var budget in Budgets)
            {
                switch(budget)
                {
                    case BudgetRanges.lowbudget:
                        min.Add(50);
                        max.Add(200);
                        break;
                    case BudgetRanges.midrange:
                        min.Add(200);
                        max.Add(400);
                        break;
                    case BudgetRanges.highend:
                        min.Add(600);
                        max.Add(800);
                        break;
                    case BudgetRanges.flagship:
                        min.Add(800);
                        max.Add(0);
                        break;
                    default:
                        min.Add(0);
                        max.Add(0);
                        break;
                };
            }

            return (min.Min(), max.Max());
        }
    }
}
