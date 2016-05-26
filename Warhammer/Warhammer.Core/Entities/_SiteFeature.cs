using System;

namespace Warhammer.Core.Entities
{

    public enum Feature
    {
        Unknown,
        SimpleStats,
        UserSettings,
        CharacterLeague,
        Graveyard,
        CharacterSheet,
        WarhammerMap,
        TrophyCabinet,
        SessionPage,
        PublicLeague,
        AdminLeague,
        ImmediateEmailer,
        NightlyEmailer,
        AwardHistory,
        AutoPopulatePeopleInNewSessions,
        ScoreHistory,
        FateStats,
        CurrentScorePie
    }

    public partial class SiteFeature
    {
        public Feature Feature
        {
            get
            {
                Feature feature;
                if(Enum.TryParse(Name, true, out feature))
                {
                    return feature;
                }
                return Feature.Unknown;
            }
            set { Name = value.ToString(); }
        }
    }
}
