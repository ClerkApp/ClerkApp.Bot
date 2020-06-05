using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enrichment.Electronics;
using ClerkBot.Enrichment.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Features;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class AspectBuilderMobile<TP, TC> where TP : MobileProfile where TC : MobileContract
    {
                private readonly BaseBuilderMobile<TP, TC> BaseBuilder;

        private readonly List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>> RangeCriteria;

        public AspectBuilderMobile(BaseBuilderMobile<TP, TC> baseBuilder)
        {
            BaseBuilder = baseBuilder;

            RangeCriteria = new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>();
        }

        public void Build()
        {
            var feature = BaseBuilder.Profile.Features.FirstOrDefault(f => 
                f.GetType().Name.Contains(nameof(MobileProfile.PhoneFeatures.Aspect))) as AspectFeatureMobile;

            if (feature is null)
            {
                return;
            }

            GenerateCriteria(feature);

            BaseBuilder.MustCollection.AddRange(RangeCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(budget => budget.Range(filter));
            }).ToList());
        }

        private void GenerateCriteria(AspectFeatureMobile feature)
        {
            var aspectMobile = new AspectMobile(feature);

            if (aspectMobile.TryGetDimension(Dimension.thickness, out var thickness))
            {
                RangeCriteria.AddRange(new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>
                {
                    device => device.Field(f => f.Body.Dimensions.Cm[nameof(Dimension.thickness)]).GreaterThan(thickness.Min),
                    device => device.Field(f => f.Body.Dimensions.Cm[nameof(Dimension.thickness)]).LessThan(thickness.Max)
                });
            }

            if (aspectMobile.TryGetDimension(Dimension.height, out var height) &&
                aspectMobile.TryGetDimension(Dimension.weight, out var weight))
            {
                RangeCriteria.AddRange(new List<Func<NumericRangeQueryDescriptor<TC>, INumericRangeQuery>>
                {
                    device => device.Field(f => f.Body.Dimensions.Cm[nameof(Dimension.height)]).GreaterThan(height.Min),
                    device => device.Field(f => f.Body.Dimensions.Cm[nameof(Dimension.height)]).LessThan(height.Max),
                    device => device.Field(f => f.Body.Dimensions.Cm[nameof(Dimension.weight)]).GreaterThan(weight.Min),
                    device => device.Field(f => f.Body.Dimensions.Cm[nameof(Dimension.weight)]).LessThan(weight.Max)
                });
            }
        }
    }
}
