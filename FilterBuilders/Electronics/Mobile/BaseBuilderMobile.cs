using System;
using System.Collections.Generic;
using ClerkBot.Models.Electronics.Mobile;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class BaseBuilderMobile<TP, TC> : IFiltersBuilder<TP, TC> 
        where TP : MobileProfile
        where TC : MobileContract
    {
        public TP Profile { get; set; }

        internal SortDescriptor<TC> SortCollection { get; }
        internal List<Func<QueryContainerDescriptor<TC>, QueryContainer>> MustCollection { get; }
        internal List<Func<QueryContainerDescriptor<TC>, QueryContainer>> MustNotCollection { get; }

        private readonly CameraBuilderMobile<TP, TC> CameraBuilderMobile;
        private readonly GamingBuilderMobile<TP, TC> GamingBuilderMobile;
        private readonly CallBuilderMobile<TP, TC> CallBuilderMobile;
        private readonly DurabilityBuilderMobile<TP, TC> DurabilityBuilderMobile;
        private readonly ReliableBuilderMobile<TP, TC> ReliableBuilderMobile;
        private readonly BudgetBuilderMobile<TP, TC> BudgetBuilderMobile;
        private readonly AspectBuilderMobile<TP, TC> AspectBuilderMobile;

        public BaseBuilderMobile(TP profile)
        {
            Profile = profile;

            MustCollection = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
            MustNotCollection = new List<Func<QueryContainerDescriptor<TC>, QueryContainer>>();
            SortCollection = new SortDescriptor<TC>();

            CameraBuilderMobile = new CameraBuilderMobile<TP, TC>(this);
            GamingBuilderMobile = new GamingBuilderMobile<TP, TC>(this);
            CallBuilderMobile = new CallBuilderMobile<TP, TC>(this);
            DurabilityBuilderMobile = new DurabilityBuilderMobile<TP, TC>(this);
            ReliableBuilderMobile = new ReliableBuilderMobile<TP, TC>(this);
            BudgetBuilderMobile = new BudgetBuilderMobile<TP, TC>(this);
            AspectBuilderMobile = new AspectBuilderMobile<TP, TC>(this);

            ExecuteBuilders();
        }

        public Func<QueryContainerDescriptor<TC>, QueryContainer> GetQuery()
        {
            return q => q
                .Bool(bq => bq
                    .MustNot(MustNotCollection)
                    .Must(MustCollection));
        }

        public Func<SortDescriptor<TC>, IPromise<IList<ISort>>> GetSort()
        {
            SortCollection.Descending(de => de.Display.Protection.Value);
            SortCollection.Descending(des => des.Features.DustResistant);
            SortCollection.Descending(desc => desc.Battery.Capacity);

            SortCollection.Descending(SortSpecialField.Score);
            return s => SortCollection;
        }

        private void ExecuteBuilders()
        {
            BudgetBuilderMobile.Build();
            ReliableBuilderMobile.Build();
            DurabilityBuilderMobile.Build();
            CameraBuilderMobile.Build();
            GamingBuilderMobile.Build();
            CallBuilderMobile.Build();
            AspectBuilderMobile.Build();
        }
    }
}
