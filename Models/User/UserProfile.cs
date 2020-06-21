using ClerkBot.Models.Electronics;

namespace ClerkBot.Models.User
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {
        public UserProfile()
        {
            ElectronicsProfile = new ElectronicsProfile();
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public int Age { get; set; }

        public string LanguagePreference { get; set; }

        public ElectronicsProfile ElectronicsProfile { get; set; }
    }
}
