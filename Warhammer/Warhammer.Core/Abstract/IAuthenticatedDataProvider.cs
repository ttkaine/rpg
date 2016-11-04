using System;
using System.Collections.Generic;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Core.Abstract
{
    public interface IAuthenticatedDataProvider
    {
        string VersionInfo();
        ICollection<Person> MyPeople();
        int AddSessionLog(int sessionId, int personId, string name, string title, string description);
        int AddSession(string title, string name, string description, DateTime dateTime);
        int AddPerson(string shortName, string longName, string description);
        void ChangePicture(int id, byte[] data, string mimeType);
        Page UpdatePageDetails(int id, string shortName, string fullName, string description);    
        Page GetPage(int id);
        ICollection<Page> RecentPages();
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
        ICollection<Page> PinnedPages();
        ICollection<Page> NewPages();
        ICollection<Page> ModifiedPages();
        void PinPage(int id);
        void MarkAsSeen(int id);
        void ResurrectPerson(int id);
        void KillPerson(int id, string obiturary, string causeOfDeath);
        Trophy GetTrophy(int id);
        int AddTrophy(string name, string description, int pointsValue, byte[] imageData, string mimeType);
        void UpdateTrophy(int id, string name, string description, int pointsValue, byte[] imageData, string mimeType);
        void UpdateTrophy(int id, string name, string description, int pointsValue);
        ICollection<Trophy> Trophies();
        void AwardTrophy(int personId, int trophyId, string reason);
        void RemoveAward(int personId, int awardId);
        Person PersonWithMyAward(TrophyType awardType);
        List<Page> Search(string searchTerm);
        int AddComment(int pageId, string description);
        int AddComment(int pageId, string description, int personId);
        int AddComment(int pageId, string description, bool isAdmin);
        List<Person> TopNpcs();
        List<Person> AllNpcs();
        void SetMyAward(int personId, TrophyType trophyType);
	    bool IsLoggedIn();
        void OpenOrCloseTextSession(int id);
        void ToggleSetAsTextSession(int id);
        List<Session> UpdatedTextSessions();
        List<Session> TextSessionsWhereItisMyTurn();
		List<Session> TextSessionsContainingMyCharacters();
        void EnsurePostOrders(int sessionId);
        List<Comment> RecentComments();
        Player MyPlayer();
        void DeleteComment(int commentId);
        List<Person> GetLeague();
        List<Person> OtherPCs();
	    Player PlayerToPostInSession(int sessionId);
		List<Session> OpenTextSessions();
		List<Session> MyOpenTextSessions();
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
    }
}
