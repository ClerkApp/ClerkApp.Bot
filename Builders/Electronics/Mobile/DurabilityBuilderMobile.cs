using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile;
using Nest;

namespace ClerkBot.Builders.Electronics.Mobile
{
    public class DurabilityBuilderMobile<TP, TC> where TP : MobileProfile where TC : MobileContract
    {
        private readonly BaseBuilderMobile<TP, TC> BaseBuilder;

        private readonly List<Func<MatchQueryDescriptor<TC>, IMatchQuery>> MatchCriteria;
        private readonly List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>> MatchPhraseCriteria;

        public DurabilityBuilderMobile(BaseBuilderMobile<TP, TC> baseBuilder)
        {
            BaseBuilder = baseBuilder;

            MatchCriteria = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>();
            MatchPhraseCriteria = new List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>>();
        }

        public void Build()
        {
            GenerateCriteria();

            BaseBuilder.MustCollection.AddRange(MatchCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(b => b.Match(filter));
            }).ToList());

            BaseBuilder.MustNotCollection.AddRange(MatchPhraseCriteria.Select(filter =>
            {
                return (Func<QueryContainerDescriptor<TC>, QueryContainer>)(b => b.MatchPhrase(filter));
            }).ToList());
        }

        private void GenerateCriteria()
        {
            var durability = BaseBuilder.Profile.Durability;

            if (durability.Value >= Intensity.Hard.Value)
            {
                MatchCriteria.Add(device => device.Field(f => f.Features.DustResistant).Query(bool.TrueString.ToLower()));
                MatchPhraseCriteria.Add(device => device.Field(f => f.Body.Build).Query("glass back"));

                if(durability.Value >= Intensity.ExtraHard.Value)
                {
                    MatchCriteria.Add(device => device.Field(f => f.Body.Build).Query("steel").Operator(Operator.Or).Query("aluminum"));
                }
            }
        }
    }
}