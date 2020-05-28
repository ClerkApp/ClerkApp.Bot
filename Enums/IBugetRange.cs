using System.Collections.Generic;

namespace ClerkBot.Enums
{
    public interface IBudgetRange
    {
        public List<BudgetRanges> Budgets { get; set; }

        public (int, int) GetBudgetRangeInEuro();
    }
}
