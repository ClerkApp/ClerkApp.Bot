namespace ClerkBot.Models.Electronics.Mobile.Features
{
    public class BudgetFeatureMobile: IMobileFeature
    {
        public int PriorityOrder { get; set; }

        public int MinBudget { get; set; }
        public int MaxBudget { get; set; }
    }
}
