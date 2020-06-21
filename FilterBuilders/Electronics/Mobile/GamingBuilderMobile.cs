using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Features;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class GamingBuilderMobile<TP, TC>: IChildBuilder
        where TP : MobileProfile
        where TC : MobileContract
    {
        private readonly GamingFeatureMobile GamingFeature;
        private readonly FiltersCollections<TC> FiltersBuilder;

        public GamingBuilderMobile(TP profile, FiltersCollections<TC> filtersBuilder)
        {
            FiltersBuilder = filtersBuilder;
            GamingFeature = profile.Features.FirstOrDefault(f => 
                f.GetType().Name.Contains(nameof(MobileProfile.PhoneFeatures.Gaming))) as GamingFeatureMobile;

        }

        public void Build()
        {
            if (GamingFeature is null)
            {
                return;
            }

            GenerateCriteria();
        }

        private void GenerateCriteria()
        {
            if (GamingFeature.GamingIntensity.Value >= Intensity.Soft.Value)
            {
                FiltersBuilder.Sort.Descending(d => d.Platform.Cpu["global"].First().Cores);

                if (GamingFeature.GamingIntensity.Value >= Intensity.Medium.Value)
                {
                    FiltersBuilder.Sort.Descending(d => d.Platform.Tests["antutu"]);

                    if (GamingFeature.GamingIntensity.Value >= Intensity.Hard.Value)
                    {
                        FiltersBuilder.Sort.Ascending(d => d.Platform.Chipset["global"].First().Size);

                        FiltersBuilder.Sort.Descending(d => d.Memory.Internals.First().Ram);
                        FiltersBuilder.Sort.Descending(d => d.Memory.Internals.First().Size);
                        FiltersBuilder.Sort.Descending(d => d.Display.RefreshRate.First());
                        FiltersBuilder.Sort.Descending(d => d.Display.Resolution.Density);
                        FiltersBuilder.Sort.Descending(d => d.Display.Resolution.Height);
                        FiltersBuilder.Sort.Descending(d => d.Display.Resolution.Weight);

                        FiltersBuilder.Sort.Descending(d => d.Platform.Chipset["global"].First().Generation);
                    }
                }
            }
        }
    }
}