using System.Collections.Generic;
using ClerkBot.Enums;

namespace ClerkBot.Enrichment
{
    public interface IBudgetRange
    {
        public List<BudgetRanges> Budgets { get; set; }

        public (int, int) GetBudgetRangeInEuro();
    }
}
