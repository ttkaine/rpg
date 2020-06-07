using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    /// <summary>
    /// The IRepository interface is the lowest data interface and connects directly to the data source
    /// 
    /// Any code inside implementations of IRepository are very hard to unit test and for this reason implemenatations of IRepository should be entirely bolier-plate code
    /// No business logic or filtering of data should happen in the IRepository
    ///  </summary>
    public interface IRepository
    {
        List<int> VisibleCampagins { get; }
            IQueryable<ChangeLog> ChangeLogs();
        IQueryable<Person> People();
        IQueryable<Player> Players();
        int Save(Page page);
        IQueryable<Page> Pages();
        void Delete(Page page);
        IQueryable<Trophy> Trophies(bool hideGlobal = false);
        int Save(Trophy trophy);
        void Delete(Award award);
        IQueryable<Award> Awards();
        IQueryable<PageView> PageViews();
        void Delete(PageView pageView);
		int Save(Post post);
		IQueryable<Post> Posts();
		void Delete(Post post);
        IQueryable<Comment> Comments();
        void Delete(Comment comment);
        IQueryable<SiteFeature> SiteFeatures();
        int Save(SiteFeature feature);
        IQueryable<UserSetting> UserSettings();
        IQueryable<Setting> Settings();
        int Save(UserSetting setting);
        int Save(Comment comment);
        IQueryable<ScoreHistory> ScoreHistories();
        int Save(ScoreHistory scoreHistory);
        IQueryable<Person> PeopleForScoring();
        IQueryable<PageImage> PageImages();

        void BulkInsert<T>( IList<T> list);
        IQueryable<FateAspect> FateAspects();
        int Save(FateAspect fateAspect);
        void Delete(FateAspect fateAspect);
        IQueryable<FateStat> FateStats();
        int Save(FateStat fateStat);
        IQueryable<FateStunt> FateStunts();
        int Save(FateStunt fateStunt);
        void Delete(FateStunt fateStunt);
        IQueryable<AdminSetting> AdminSettings();
        int Save(AdminSetting setting);
        int Save(Player player);
        int Save(ExceptionLog exceptionLog);
        IQueryable<ExceptionLog> ExceptionLogs();
        void Delete(PersonStat personStat);
        int Save(PageView pageView);
        void Delete(ScoreHistory scoreHistory);
        int Save(PageImage pageImage);
        void Delete(PageImage pageImage);
        IQueryable<PriceListItem> PriceListItems();
        int Save(PriceListItem priceListItem);
        IQueryable<CampaignDetail> CampaignDetails();
        IQueryable<Rumour> Rumours();     
        int Save(Rumour rumour);
        void Delete(Rumour rumour);
        int Save(CampaignDetail campaignDetail);
        void Delete(Asset asset);
        int Save(Asset asset);
        IQueryable<Asset> Assets();
        IQueryable<PersonAttribute> PersonAttributes();
        void Delete(PersonAttribute personAttribute);
        int Save(PersonAttribute personAttribute);
        int Save(Award award);
        IQueryable<Person> AllPeople();
        IQueryable<ScoreHistory> AllScoreHistories();
        int Save(AwardNomination awardNomination);
        IQueryable<AwardNomination> AwardNominations();
        int CurrentCampaignId { get; }
        bool IsMasterDomain { get; }
        List<CampaignDetail> AllMyCampaigns();
        IQueryable<BookPage> BookPages();
        void Delete(BookPage bookPage);
        IQueryable<Arc> Arcs();
        IQueryable<ScoreBreakdown> ScoreBreakDowns();
        void Delete(ScoreBreakdown scoreBreakdown);
        int Save(SiteIcon icon);
        IQueryable<SiteIcon> SiteIcons();
        IQueryable<CampaignDetail> AllCampaigns();
        IQueryable<PlayerCampaign> PlayerCampaigns();
        void Delete(PlayerCampaign playerCampaign);
        int Save(PlayerCampaign playerCampaign);
        IQueryable<SiteFeature> AllSiteFeatures();
        IQueryable<PlayerSecret> PlayerSecrets();
        int Save(PlayerSecret playerSecret);
        IQueryable<DefaultPersonAttribute> DefaultPersonAttributes();
        void Delete(DefaultPersonAttribute defaultPersonAttribute);
        int Save(DefaultPersonAttribute defaultPersonAttribute);
        IQueryable<Term> Terms(); 
        void Delete(Term term);
        int Save(Term term);

        IQueryable<ExperiencePoint> ExperiencePoints();
        void Delete(ExperiencePoint experiencePoint);
        int Save(ExperiencePoint experiencePoint);
    }
}

