using Ardalis.SmartEnum;

namespace ClerkBot.Enums
{
    public sealed class Intensity: SmartEnum<Intensity>
    {
        public static readonly Intensity Unknown = new Intensity(nameof(Unknown), 0);
        public static readonly Intensity ExtraSoft = new Intensity(nameof(ExtraSoft), 1);
        public static readonly Intensity Soft = new Intensity(nameof(Soft), 2);
        public static readonly Intensity Medium = new Intensity(nameof(Medium), 3);
        public static readonly Intensity Hard = new Intensity(nameof(Hard), 4);
        public static readonly Intensity ExtraHard = new Intensity(nameof(ExtraHard), 5);

        private Intensity(string name, int value) : base(name, value)
        {
        }
    }
}
