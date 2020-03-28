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
        AwardHistory,
        FateStats,
        CurrentScorePie,
        SimpleAutoXp,
        SimpleHitPoints,
        CrowRules,
        Bestiary,
        PriceList,
        PublicPrices,
        PersonDetails,
        CrowTopStatsPanel,
        CrowStats,
        FuHammerStats,
        PersonDescriptors,
        PersonRoles,
        PersonAspects,
        RumourMill,
        CrowNpcSheet,
        ShowGameDate,
        Assets,
        ShadowMode,
        PersonAttributes,
        SoloXp,
        VariableSessionGm,
        PostBonusXp,
        XpCatchup,
        PlaygroundMode,
        ShowWearTrack,
        FavouritesGallery,
        WarhammerMoney,
        WarhammerDate,
        Reports,
        WordCountChart,
        PlayerWordCountChart,
        CharacterGenderChart,
        GenderScoresChart,
        PcNpcScoresChart,
        PlayerPageChart,
        TopAwardsChart,
        PlayerTextPostChart,
        AwardNominations,
        GmNotes,
        FixedHarmAndWear,
        RecentItemsMenu,
        XpGroups,
        CharacterWordCountChart,
        CharacterTextPostChart,
        RandomItemGenerator,
        ShowOtherSitesMenu,
        HideGlobalAwards,
        SessionArcs,
        CharacterTokens,
        EnforceShadowMode,
        ManageSessionPeople,
        MasterTextSessions,
        CrowMagic,
        FullCharacterLeague,
        UpdatedCharacterLeague,
        AutoTextSessionXp,
        ShowWishingWell,
        TrophyXp,
        Vampire,
        ResolveAndResilience
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
