using System.Collections.Generic;

namespace ClerkBot.Enums
{
    public interface IBugetRange
    {
        public List<BugetRanges> Budgets { get; set; }

        public (int, int) GetBudgetRangeInEuro();
    }
}
