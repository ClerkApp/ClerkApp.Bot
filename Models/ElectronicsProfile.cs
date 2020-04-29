namespace ClerkBot.Models
{
    public class ElectronicsProfile
    {
        public ElectronicsProfile()
        {
            PhoneProfile = new PhoneProfile();
        }

        public PhoneProfile PhoneProfile { get; set; }
    }
}
