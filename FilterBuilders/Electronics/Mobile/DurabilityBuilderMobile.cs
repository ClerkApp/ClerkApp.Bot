using System;
using System.Collections.Generic;
using System.Linq;
using ClerkBot.Enums;
using ClerkBot.Models.Electronics.Mobile;
using Nest;

namespace ClerkBot.FilterBuilders.Electronics.Mobile
{
    public class DurabilityBuilderMobile<TP, TC>: IChildBuilder
        where TP : MobileProfile 
        where TC : MobileContract
    {
        private readonly Intensity Durability;
        private readonly FiltersCollections<TC> FiltersBuilder;

        private readonly List<Func<MatchQueryDescriptor<TC>, IMatchQuery>> MatchCriteria;
        private readonly List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>> MatchPhraseCriteria;

        public DurabilityBuilderMobile(TP profile, FiltersCollections<TC> filtersBuilder)
        {
            Durability = profile.Durability;
            FiltersBuilder = filtersBuilder;
            MatchCriteria = new List<Func<MatchQueryDescriptor<TC>, IMatchQuery>>();
            MatchPhraseCriteria = new List<Func<MatchPhraseQueryDescriptor<TC>, IMatchPhraseQuery>>();
        }

        public void Build()
        {
            GenerateCriteria();

            FiltersBuilder.Must.AddRange(MatchCriteria.Select(filter =>
                (Func<QueryContainerDescriptor<TC>, QueryContainer>)(b => b.Match(filter))
            ).ToList());

            FiltersBuilder.MustNot.AddRange(MatchPhraseCriteria.Select(filter =>
                (Func<QueryContainerDescriptor<TC>, QueryContainer>)(b => b.MatchPhrase(filter))
            ).ToList());
        }

        private void GenerateCriteria()
        {
            if (Durability.Value >= Intensity.Hard.Value)
            {
                MatchCriteria.Add(device => device.Field(f => f.Features.DustResistant).Query(bool.TrueString.ToLower()));
                MatchPhraseCriteria.Add(device => device.Field(f => f.Body.Build).Query("glass back"));

                if(Durability.Value >= Intensity.ExtraHard.Value)
                {
                    MatchCriteria.Add(device => device.Field(f => f.Body.Build).Query("steel").Operator(Operator.Or).Query("aluminum"));
                }
            }
        }
    }
}