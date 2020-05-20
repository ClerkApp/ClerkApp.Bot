namespace ClerkBot.Enums
{
    public interface IBugetRange
    {
        public BugetRanges Budget { get; set; }

        public (int, int) GetBudgetRangeInEuro();
    }
}
