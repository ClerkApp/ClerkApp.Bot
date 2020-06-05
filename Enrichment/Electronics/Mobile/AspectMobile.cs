using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile.Features;

namespace ClerkBot.Enrichment.Electronics.Mobile
{
    public class AspectMobile: IObjectDimensions
    {
        public List<Dimensions> Dimensions { get; set; }
        private AspectFeatureMobile AspectFeature { get; }

        public AspectMobile(AspectFeatureMobile aspectFeature)
        {
            AspectFeature = aspectFeature;
            Dimensions = new List<Dimensions>
            {
                GetThicknessSizeInCm(),
                GetWeightSizeInCm(),
                GetHeightSizeInCm()
            };
        }

        public bool TryGetDimension(Dimension type, out Dimensions dimensionInList)
        {
            dimensionInList = Dimensions.FirstOrDefault(x => x.Type.Equals(type));

            return dimensionInList != null && dimensionInList.IsSet;
        }

        private Dimensions GetHeightSizeInCm()
        {
            var values = new List<int>();

            if (AspectFeature.SizeSmall)
            {
                values.Add(80);
                values.Add(120);
            }
            if (AspectFeature.SizeMedium)
            {
                values.Add(110);
                values.Add(150);
            }
            if (AspectFeature.SizeBig)
            {
                values.Add(140);
                values.Add(220);
            }

            if (values.Count.Equals(0))
            {
                return new Dimensions
                {
                    IsSet = false,
                    Type = Dimension.height
                };
            }

            return new Dimensions
            {
                IsSet = true,
                Type = Dimension.height,
                Max = values.Max(),
                Min = values.Min()
            };
        }

        private Dimensions GetWeightSizeInCm()
        {            
            var values = new List<int>();

            if (AspectFeature.SizeSmall)
            {
                values.Add(40);
                values.Add(60);
            }
            if (AspectFeature.SizeMedium)
            {
                values.Add(50);
                values.Add(70);
            }
            if (AspectFeature.SizeBig)
            {
                values.Add(70);
                values.Add(150);
            }

            if (values.Count.Equals(0))
            {
                return new Dimensions
                {
                    IsSet = false,
                    Type = Dimension.weight
                };
            }

            return new Dimensions
            {
                IsSet = true,
                Type = Dimension.weight,
                Max = values.Max(),
                Min = values.Min()
            };
        }

        private Dimensions GetThicknessSizeInCm()
        {
            var values = new List<int>();

            switch(AspectFeature.Thickness.Name)
            {
                case nameof(Intensity.Medium):
                    values.Add(10);
                    values.Add(18);
                    break;
                case nameof(Intensity.ExtraHard):
                    values.Add(5);
                    values.Add(10);
                    break;
            }

            if (values.Count.Equals(0))
            {
                return new Dimensions
                {
                    IsSet = false,
                    Type = Dimension.thickness
                };
            }

            return new Dimensions
            {
                IsSet = true,
                Type = Dimension.thickness,
                Max = values.Max(),
                Min = values.Min()
            };
        }
    }
}
