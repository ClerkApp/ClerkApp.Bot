using ClerkBot.Enums;

namespace ClerkBot.Models.Electronics.Mobile
{
    public class MobileBugetRange: IBugetRange
    {
        public BugetRanges Budget { get; set; }
        public (int, int) GetBudgetRangeInEuro()
        {
            return Budget switch
            {
                BugetRanges.lowbudget => (50, 200),
                BugetRanges.midrange => (200, 400),
                BugetRanges.highend => (600, 800),
                BugetRanges.flagship => (800, 0),
                _ => (0, 0)
            };
        }
    }
}
