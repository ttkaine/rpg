using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Core.Models.Reports;

namespace Warhammer.Core.Abstract
{
    public interface IAuthenticatedDataProvider
    {
        bool CurrentPlayerIsGm { get; }
        string VersionInfo();
        ICollection<Person> MyPeople();
        int AddSessionLog(int sessionId, int personId, string name, string title, string description);
        int AddSession(string title, string name, string description, DateTime dateTime);
        int GetGmId(int sessionId);
        int AddPerson(string shortName, string longName, string description, bool personCreateAsNpc, Gender personGender);
        void ChangePicture(int id, byte[] data, string mimeType);
        Page UpdatePageDetails(int id, string shortName, string fullName, string description);    
        Page GetPage(int id, bool asNoTracking = false);
        ICollection<PageLinkWithUpdateDateModel> RecentPages();
        ICollection<Page> MyStuff();
        ICollection<Session> Sessions();
        ICollection<Person> People();
        ICollection<SessionLog> Logs();
        void RemoveProfileImage(int id);
        int AddPage(string shortName, string fullName, string description);
        ICollection<Place> Places();
        int AddPlace(string fullName, string shortName, string description, int? parentId);
        ICollection<Page> PossibleLinks(int id);
        void AddLink(int id, int addLinkTo);
        void RemoveLink(int id, int linkToDeleteId);
        void DeletePage(int id);
        bool PageExists(string shortName, string fullName);
        bool PageExists(string shortName);
        Page GetPage(string shortName);
        ICollection<PageLinkModel> PinnedPages();
        ICollection<PageLinkModel> NewPages();
        ICollection<PageLinkModel> ModifiedPages();
        void TogglePagePin(int id);
        void MarkAsSeen(int id);
        void ResurrectPerson(int id);
        void KillPerson(int id, string obiturary, string causeOfDeath);
        Trophy GetTrophy(int id);
        int AddTrophy(string name, string description, int pointsValue, byte[] imageData, string mimeType, bool currentCampaignOnly);
        void UpdateTrophy(int id, string name, string description, int pointsValue, byte[] imageData, string mimeType, bool currentCampaignOnly);
        void UpdateTrophy(int id, string name, string description, int pointsValue, bool currentCampaignOnly);
        ICollection<Trophy> Trophies();
        void AwardTrophy(int personId, int trophyId, string reason);
        void RemoveAward(int personId, int awardId);
        PageLinkModel PersonWithMyAward(TrophyType awardType);
        List<Page> Search(string searchTerm);
        int AddComment(int pageId, string description);
        int AddComment(int pageId, string description, int personId);
        int AddComment(int pageId, string description, bool isAdmin);
        List<PageLinkModel> TopNpcs();
        List<Person> AllNpcs();
        void SetMyAward(int personId, TrophyType trophyType);
	    bool IsLoggedIn();
        void OpenOrCloseTextSession(int id);
        void ToggleSetAsTextSession(int id);
        List<Session> UpdatedTextSessions();
        List<Session> TextSessionsWhereItisMyTurn();

        [Obsolete("Seriously... just no...", true)]
        List<Session> TextSessionsContainingMyCharacters();
        void EnsurePostOrders(int sessionId);
        List<Comment> RecentComments();
        Player MyPlayer();
        void DeleteComment(int commentId);
        List<Person> GetLeague();
        List<PageLinkModel> OtherPCs();
	    Player PlayerToPostInSession(int sessionId);
		List<Session> OpenTextSessions();
		List<Session> MyOpenTextSessions();
        [Obsolete("Seriously... just no...", true)]
        List<Session> ModifiedTextSessions();
        bool SiteHasFeature(Feature featureName);
        void EnableFeature(string featureName);
        void DisableFeature(string featureName);
        Person GetPerson(int personId);
        void SetStats(int personId, Dictionary<StatName, int> stats, string addedRole, List<string> descriptors);
        void AddRoleToPerson(int personId, string role);
        void AddDescriptorToPerson(int personId, string descriptor);
        void BuyStatIncrease(int personId, StatName statName);
        void AddXp(int personId, decimal xpValue);
        bool CheckStatPermissions(int personId);
        bool CheckStatSummaryPermissions();
        List<Person> NpcWithXp();
        List<Person> PeopleInGraveyard();
        bool CurrentUserIsAdmin { get; }
        bool ShowGraveyard { get;  }
        bool ShowLeague { get; }
        bool ShowCharacterSheet { get; }
        bool CurrentUserIsGuest { get; }
        bool ShadowMode { get; set; }
        int CurrentPlayerId { get; }
        List<UserSetting> UserSettings();
        bool SettingIsEnabled(Setting setting);
        List<Setting> SettingSection(int sectionId);
        int SwitchSetting(int settingId);
        void EditComment(int commentId, string comment);
        void NotifyTurn(int sessionId);
        List<SiteFeature> AllFeatures();
        void EnsureFeatures();
        List<Object> RecentActivity();
        void ToggleSessionPrivacy(int id);
        List<ScoreHistory> PersonScoreHistory(int id);
        List<PageListItemModel> NpcList();
        List<FateAspect> GetAspects(int id);
        void SaveAspects(List<FateAspect> fateAspects);
        List<FateStat> GetFateStats(int id);
        void SaveFateStats(IEnumerable<FateStat> fateStats);
        List<FateStunt> GetStunts(int id);
        void SaveStunt(FateStunt stunt);
        void DeleteStunt(int stuntId);
        void ToggleStuntVisibility(int stuntId);
        List<Award> GetLatestAwards(int count);
        List<ScoreHistory> GetCurrentScoresForPerson(int id);
        void SetDefaultHitPoints(int id);
        void BuyHitPointSlot(int id, SimpleHitPointLevel simpleHitPointLevel, SimpleHitPointType simpleHitPointType, bool free = false);
        void AddXpForSession(int sessionId, decimal xpAwarded);

        void AddPlayer(string name, string email);
        List<ExceptionLog> GetExceptionLogs(int count);
        void ResetNpcStats(int id);
        List<Page> AllPages();
        void AddDefaultXp(int pageId);
        List<PageListItemModel> FullPageList();
        List<Page> PagesWithOutstandingXp();
        List<Person> NpcsWithStats();
        PageImage SaveImage(int pageId, byte[] image);
        PageImage GetPageImage(int id);
        void SetPageXpAwarded(int pageId);
        List<Creature> Creatures();
        int AddCreature(CreateCreatureViewModel creatureModel);
        int AddOrganisation(string name, string description);
        List<PriceListItem> PriceList();
        void SavePriceList(List<PriceListItem> priceListItems);
        CampaignDetail GetCampaginDetails();
        void SetDetails(int personId, int crowns, int shillings, int pennies, DateTime dob, string height, int modelUpkeep);
        List<Rumour> GetAllRumours();
        void SaveRumours(List<Rumour> rumours);
        void DeleteRumour(int id);
        List<Rumour> GetRumoursForPlace(int placeId);
        void SetAge(int personId, int age);
        void SetDob(int personId, DateTime dateOfBirth);
        void SetHeight(int personId, string height);
        void SetMoney(int personId, int crowns, int shillings, int pennies, int upkeep);
        void SetGameDate(DateTime? currentGameDate);
        void AddDayToGameDate();
        void AddWeekToGameDate();
        void AddMonthToGameDate();
        void SetAssets(int personId, List<Asset> assets);
        void AddAsset(int personId, string title, string description, int upkeep);
        void SpendMoney(int personId, int spendCrowns, int spendShillings, int spendPence);
        void AddMoney(int personId, int addCrowns, int addShillings, int addPence);
        int GetGmId();
        void SetGmSuspended(int sessionId, bool suspended);
        void SetPlayerSuspended(int sessionId, int playerId, bool suspended);
        List<PageLinkModel> GetRelatedPages(int id);
        bool PlayerSettingEnabled(SettingNames setting);
        List<Award> AwardsForTrophy(int id);
        List<Player> GetAllPlayers();
        void SetSessionGm(int sessionId, int? selectedGm);
        List<PageLinkModel> PeopleWithXpToSpend();
        void AwardShiftForSession(int id);
        void UpdateAward(int id, string awardReason);
        List<Person> GetNpcSheetPeople();
        List<PageLinkModel> GetFavourites();
        void SetGender(int personId, Gender gender);
        List<ChartDataItem> GetWordcountReportData();
        List<ChartDataItem> GetPlayerWordcountReportData();
        List<ChartDataItem> GetCharacterGenderReportData();
        List<ChartDataItem> GetGenderScoresReportData();
        List<ChartDataItem> GetPagesByPlayerReportData();
    }
}
