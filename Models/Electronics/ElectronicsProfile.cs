using ClerkBot.Models.Electronics.Mobile;

namespace ClerkBot.Models.Electronics
{
    public class ElectronicsProfile
    {
        public ElectronicsProfile()
        {
            MobileProfile = new MobileProfile();
        }

        public MobileProfile MobileProfile { get; set; }
    }
}
