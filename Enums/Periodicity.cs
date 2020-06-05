using Ardalis.SmartEnum;

namespace ClerkBot.Enums
{
    public sealed class Periodicity: SmartEnum<Periodicity>
    {
        public static readonly Periodicity Unknown = new Periodicity(nameof(Unknown), 0);
        public static readonly Periodicity Occasionally = new Periodicity(nameof(Occasionally), 1);
        public static readonly Periodicity Monthly = new Periodicity(nameof(Monthly), 2);
        public static readonly Periodicity Weekly = new Periodicity(nameof(Weekly), 3);
        public static readonly Periodicity Daily = new Periodicity(nameof(Daily), 4);
        public static readonly Periodicity Hourly = new Periodicity(nameof(Hourly), 5);

        private Periodicity(string name, int value) : base(name, value)
        {
        }
    }
}
