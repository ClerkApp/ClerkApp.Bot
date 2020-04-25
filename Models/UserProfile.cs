using System;

namespace ClerkBot.Models
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public int Age { get; set; }

        public string Description { get; set; }

        public DateTime CallbackTime { get; set; }

        public string PhoneNumber { get; set; }

        public string Bug { get; set; }

        public string LanguagePreference { get; set; }
    }
}
