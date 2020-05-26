using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;

namespace ClerkBot.Models.Electronics.Mobile.Enrichment
{
    public class MobileBugetRange: IBugetRange
    {
        public List<BugetRanges> Budgets { get; set; }

        public (int, int) GetBudgetRangeInEuro()
        {
            var min = new List<int>();
            var max = new List<int>();
            foreach (var budget in Budgets)
            {
                switch(budget)
                {
                    case BugetRanges.lowbudget:
                        min.Add(50);
                        max.Add(200);
                        break;
                    case BugetRanges.midrange:
                        min.Add(200);
                        max.Add(400);
                        break;
                    case BugetRanges.highend:
                        min.Add(600);
                        max.Add(800);
                        break;
                    case BugetRanges.flagship:
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
