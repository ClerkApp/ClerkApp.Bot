using ClerkBot.Models.Electronics.Phone;

namespace ClerkBot.Models.Electronics
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
