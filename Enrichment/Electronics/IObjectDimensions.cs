using System.Collections.Generic;

namespace ClerkBot.Enrichment.Electronics
{
    interface IObjectDimensions
    {
        public List<Dimensions> Dimensions { get; set; }
    }

    public class Dimensions
    {
        public bool IsSet { get; set; }
        public Dimension Type { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }

    public enum Dimension
    {
        height,
        weight,
        thickness
    }
}
