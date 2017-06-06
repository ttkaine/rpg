using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Transactions;
using LinqKit;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Core.Concrete
{

    public class AuthenticatedDataProvider : IAuthenticatedDataProvider
    {
        private readonly IAuthenticatedUserProvider _authenticatedUser;
        private readonly IRepository _repository;
        private readonly IModelFactory _factory;
        private readonly IEmailHandler _email;
        private readonly IScoreCalculator _score;
        private readonly ISiteFeatureProvider _feature;
        public bool ShadowMode { get; set; }
        public int CurrentPlayerId => CurrentPlayer.Id;
        private List<int> _myPageIds;
        public List<int> MyPageIds
        {
            get
            {
                if (_myPageIds == null)
                {
                    _myPageIds = _repository.Pages()
                    .Where(p => p.CreatedBy.UserName == _authenticatedUser.UserName)
                    .Select(p => p.Id)
                    .ToList();
                    _myPageIds.AddRange(MySessionIds);
                    _myPageIds = _myPageIds.Distinct().ToList();
                }
                return _myPageIds;
            }
        }

        private List<int> _mySessionIds;
        public List<int> MySessionIds
        {
            get
            {
                if (_mySessionIds == null)
                {
                    _mySessionIds = _repository.Pages().OfType<Session>()
                    .Where(p => p.Related.OfType<Person>().Any(r => r.Player.UserName == _authenticatedUser.UserName))
                    .Select(p => p.Id)
                    .ToList();
                }
                return _mySessionIds;
            }
        }

        public AuthenticatedDataProvider(IAuthenticatedUserProvider authenticatedUser, IRepository repository, IModelFactory factory, IEmailHandler email, IScoreCalculator score, ISiteFeatureProvider feature)
        {
            _authenticatedUser = authenticatedUser;
            _repository = repository;
            _factory = factory;
            _email = email;
            _score = score;
            _feature = feature;

            if (_authenticatedUser.UserIsAuthenticated)
            {
                ShadowMode = SiteHasFeature(Feature.ShadowMode) && PlayerSettingEnabled(SettingNames.ShadowMode);
            }
        }

        private Player _currentPlayer = null;
        public Player CurrentPlayer
        {
            get
            {
                if (_currentPlayer == null)
                {
                    if (!_authenticatedUser.UserIsAuthenticated)
                    {
                        throw new Exception("User is not Authenticated");
                    }
                    _currentPlayer = _repository.Players().Single(p => p.UserName == _authenticatedUser.UserName);
                }
                return _currentPlayer;
            }
        }

        private Player _gm = null;
        private CampaignDetail _campaign = null;

        public CampaignDetail Campaign
        {
            get
            {
                if (_campaign == null)
                {
                    _campaign = _repository.CampaignDetails().Single();
                }
                return _campaign;
            }
        }

        public Player Gm
        {
            get
            {
                if (_gm == null)
                {

                    if (Campaign != null)
                    {
                        _gm = _repository.Players().Single(p => p.Id == Campaign.GmId);
                    }
                }
                return _gm;
            }
        }

        public bool CurrentPlayerIsGm
        {
            get
            {
                if (Campaign?.GmId == CurrentPlayer.Id)
                {
                    return true;
                }
                return false;
            }
        }

        public string VersionInfo()
        {
            string softwareVersion = $"Software Version: {Assembly.GetExecutingAssembly().GetName().Version}";
            string databaseVersion = "Default Database";

            ChangeLog dbVersion = _repository.ChangeLogs().OrderByDescending(c => c.DateTime).FirstOrDefault();
            if (dbVersion != null)
            {
                databaseVersion =
                    $"Database: {dbVersion.Id}:{dbVersion.DateTime.Year.ToString("00")}{dbVersion.DateTime.Month.ToString("00")}{dbVersion.DateTime.Day.ToString("00")}";
            }

            return $"{softwareVersion} :: {databaseVersion}";
        }

        public ICollection<Person> MyPeople()
        {
            return _repository.People().Where(p => p.PlayerId == CurrentPlayer.Id && p.IsDead == false).ToList();
        }

        public int AddSessionLog(int sessionId, int personId, string name, string title, string description)
        {
            SessionLog session = new SessionLog
            {
                ShortName = name,
                FullName = title,
                Description = description,
                SessionId = sessionId,
                PersonId = personId
            };
            return Save(session);
        }

        public int AddSession(string title, string name, string description, DateTime date)
        {
            Session session = new Session
            {
                ShortName = name,
                FullName = title,
                Description = description,
                DateTime = date
            };

            Session previousSession = null;

            if (SiteHasFeature(Feature.AutoPopulatePeopleInNewSessions))
            {

                previousSession =
                    _repository.Pages()
                        .OfType<Session>()
                        .OrderByDescending(s => s.DateTime)
                        .FirstOrDefault(s => s.IsTextSession == false) ?? _repository.Pages()
                            .OfType<Session>()
                            .OrderByDescending(s => s.DateTime)
                            .FirstOrDefault();
            }

            int id = Save(session);

            if (SiteHasFeature(Feature.AutoPopulatePeopleInNewSessions) && previousSession != null)
            {
                foreach (Person person in previousSession.People)
                {
                    AddLink(person.Id, id);
                }
            }
            return id;
        }

        public int GetGmId(int sessionId)
        {
            int? sessionGm = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId)?.GmId;
            if (sessionGm.HasValue)
            {
                return sessionGm.Value;
            }
            return GetGmId();
        }

        public int AddPerson(string shortName, string longName, string description, bool personCreateAsNpc)
        {

            Person person = new Person
            {
                ShortName = shortName,
                FullName = longName,
                Description = description,
            };
            if (!CurrentPlayerIsGm && !personCreateAsNpc)
            {
                person.PlayerId = CurrentPlayer.Id;
            }
            int pageId = Save(person);
            return pageId;
        }


        public void ChangePicture(int id, byte[] data, string mimeType)
        {
            PageImage image = _repository.PageImages().FirstOrDefault(p => p.PageId == id && p.IsPrimary) ?? new PageImage();

            if (data == null && image.Id > 0)
            {
                _repository.Delete(image);
            }
            else
            {
                image.Data = data;
                image.PageId = id;
                image.IsPrimary = true;

                _repository.Save(image);
            }

        }

        public Page UpdatePageDetails(int id, string shortName, string fullName, string description)
        {
            Page existingPage = _repository.Pages().FirstOrDefault(p => p.Id == id);
            if (existingPage != null)
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = string.Empty;
                }

                int changedLength = description.Length;
                int originalLength = 0;

                if (!string.IsNullOrWhiteSpace(existingPage.Description))
                {
                    originalLength = existingPage.Description.Length;
                }

                if (changedLength > (originalLength + 200))
                {
                    existingPage.SignificantUpdate = DateTime.Now;
                    existingPage.SignificantUpdateById = CurrentPlayer.Id;
                }

                existingPage.ShortName = shortName;
                existingPage.FullName = fullName;
                existingPage.Description = description;

                if (existingPage.HasInlineImage)
                {
                    if (!existingPage.HasExternalImage)
                    {
                        PageImage image = new PageImage
                        {
                            PageId = existingPage.Id,
                            Data = existingPage.ImageData,
                            IsPrimary = true
                        };

                        _repository.Save(image);
                    }
                    existingPage.ImageData = null;
                }
            }

            Save(existingPage);
            AutoAddLinks(existingPage);
            return existingPage;
        }

        private void AutoAddLinks(Page existingPage)
        {
            string pageText = existingPage.RawText.ToLower();

            List<Page> pages =
                _repository.Pages()
                    .Where(p => !(p is Session) && !(p is SessionLog) && (p.Id != existingPage.Id))
                    .ToList();


            foreach (Page page in pages)
            {
                bool isFound = pageText.Contains(string.Format(" {0} ", page.ShortName.ToLower()));

                if (pageText.Contains(string.Format(" {0}.", page.ShortName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format(" {0},", page.ShortName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format(" {0}'", page.ShortName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format("[[{0}]]", page.ShortName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format(">{0} ", page.ShortName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format(">{0},", page.ShortName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format(">{0}'", page.ShortName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format(" {0} ", page.FullName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format(" {0},", page.FullName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format(" {0}.", page.FullName.ToLower())))
                {
                    isFound = true;
                }
                if (pageText.Contains(string.Format(" {0}'", page.FullName.ToLower())))
                {
                    isFound = true;
                }
                if (isFound)
                {
                    AddLink(page.Id, existingPage.Id);
                }
            }
        }

        private int Save(Page page)
        {
            bool isNew = false;
            Page existingPage = _repository.Pages().FirstOrDefault(p => p.Id == page.Id);

            if (existingPage == null)
            {
                page.Created = DateTime.Now;
                page.CreatedById = CurrentPlayer.Id;
                page.SignificantUpdate = DateTime.Now;
                page.SignificantUpdateById = CurrentPlayer.Id;
                isNew = true;
            }

            while (AnotherPageExistsWithThisName(page.ShortName, page.Id))
            {
                page.ShortName = page.ShortName + " - New";
            }
            page.Modified = DateTime.Now;
            page.ModifedById = CurrentPlayer.Id;
            page.PlainText = page.RawText;


            int pageId = _repository.Save(page);

            if (isNew)
            {
                NotifyAddPage(pageId);
            }
            else
            {
                if (page.SignificantUpdate >= DateTime.Now.AddSeconds(-5) && page.Created <= DateTime.Now.AddHours(-1))
                {
                    NotifyEditPage(pageId);
                }
            }

            Person person  = page as Person;
            if (person != null)
            {
                _score.UpdatePersonScore(pageId);
            }

            return pageId;
        }



        private bool AnotherPageExistsWithThisName(string shortName, int id)
        {
            return _repository.Pages().Any(p => p.ShortName == shortName && p.Id != id);
        }

        public Page GetPage(int id, bool asNoTracking = false)
        {
            var query = _repository.Pages();
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
                
            return query.FirstOrDefault(p => p.Id == id);
        }

        public ICollection<PageLinkWithUpdateDateModel> RecentPages()
        {
            var query = _repository.Pages();

            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }

            return query.OrderByDescending(p => p.SignificantUpdate).Select(p => new PageLinkWithUpdateDateModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, LastUpdate = p.SignificantUpdate }).Take(20).ToList();
        }

        private IQueryable<Page> ApplyShadow(IQueryable<Page> query)
        {
            return query.Where(p => MyPageIds.Contains(p.Id) || p.Pages.Any(r => MyPageIds.Contains(r.Id)));
        }

        private IQueryable<Person> ApplyPeopleShadow(IQueryable<Person> query)
        {
            return query.Where(p => MyPageIds.Contains(p.Id) || p.Pages.Any(r => MyPageIds.Contains(r.Id)));
        }

        private IQueryable<Comment> ApplyShadow(IQueryable<Comment> query)
        {
            return query.Where(c => MyPageIds.Contains(c.PageId) || c.Page.Pages.Any(r => MyPageIds.Contains(r.Id)));
        }

        private IQueryable<Award> ApplyShadow(IQueryable<Award> query)
        {
            return query.Where(c => MyPageIds.Contains(c.PersonId) || c.Person.Pages.Any(r => MyPageIds.Contains(r.Id)));
        }

        public ICollection<Page> MyStuff()
        {
            return _repository.Pages().Where(p => p.CreatedById == CurrentPlayer.Id).OrderByDescending(p => p.Created).ToList();
        }

        public ICollection<Session> Sessions()
        {
            var query = _repository.Pages();
            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }
            return query.OfType<Session>().ToList();
        }

        public ICollection<Person> People()
        {
            return _repository.Pages().OfType<Person>().ToList();
        }

        public ICollection<SessionLog> Logs()
        {
            return _repository.Pages().OfType<SessionLog>().ToList();
        }

        public void RemoveProfileImage(int id)
        {
            ChangePicture(id, null, null);
        }

        public int AddPage(string shortName, string fullName, string description)
        {
            Page person = new Page
            {
                ShortName = shortName,
                FullName = fullName,
                Description = description,
            };

            int pageId = Save(person);
            return pageId;
        }

        private void NotifyAddPage(int pageId)
        {
            if (SiteHasFeature(Feature.ImmediateEmailer))
            {
                List<Player> players = GetPlayersWithSetting(SettingNames.SendEmailOnNewPage);
                Page page = GetPage(pageId);
                if (page != null && players.Any())
                {
                    _email.NotifyNewPage(page, players);
                }
            }
        }

        private void NotifyAddComment(int pageId, string commenterName, string description)
        {
            if (SiteHasFeature(Feature.ImmediateEmailer))
            {
                List<Player> players = GetPlayersWithSetting(SettingNames.SendEmailOnNewComment);
                Page page = GetPage(pageId);
                if (page != null && players.Any())
                {
                    _email.NotifyNewComment(commenterName, page, players, description);
                }
            }
        }

        private void NotifyEditPage(int pageId)
        {
            if (SiteHasFeature(Feature.ImmediateEmailer))
            {
                List<Player> players = GetPlayersWithSetting(SettingNames.SendEmailOnUpdatePage);
                Page page = GetPage(pageId);
                if (page != null && players.Any())
                {
                    _email.NotifyEditPage(page, players);
                }
            }
        }

        private List<Player> GetPlayersWithSetting(SettingNames settingName, bool includeSelf = false)
        {
            Setting setting = _repository.Settings().FirstOrDefault(s => s.Name == settingName.ToString());
            if (setting != null)
            {
                var players =
                    _repository.UserSettings().Where(u => u.Enabled && u.SettingId == setting.Id).Select(s => s.Player);

                if (!includeSelf)
                {
                    players = players.Where(p => p.Id != CurrentPlayer.Id);
                }
                return players.ToList();
            }
            return new List<Player>();
        }

        public ICollection<Place> Places()
        {
            return _repository.Pages().OfType<Place>().ToList();
        }

        public int AddPlace(string fullName, string shortName, string description, int? parentId)
        {
            Place place = new Place
            {
                ShortName = shortName,
                FullName = fullName,
                Description = description,
                IsWithin = parentId
            };
            return Save(place);
        }

        public ICollection<Page> PossibleLinks(int id)
        {
            Page page = GetPage(id);
            List<int> relatedIds = page.Related.Select(p => p.Id).ToList();
            List<Page> linkPages = _repository.Pages().Where(p => !relatedIds.Contains(p.Id)).OrderBy(s => s.ShortName).ToList();
            return linkPages;
        }

        public void AddLink(int id, int addLinkTo)
        {
            Page page = GetPage(id);
            Page linkTo = GetPage(addLinkTo);
            if (!page.Related.Contains(linkTo))
            {
                page.Related.Add(linkTo);
                linkTo.Related.Add(page);
                Save(page);
                Save(linkTo);
            }
        }

        public void RemoveLink(int id, int linkToDeleteId)
        {
            Page page = GetPage(id);
            Page linkTo = GetPage(linkToDeleteId);
            if (page.Related.Contains(linkTo))
            {
                page.Related.Remove(linkTo);
                Save(page);
            }
            if (linkTo.Related.Contains(page))
            {
                linkTo.Related.Remove(page);
                Save(linkTo);
            }
        }

        public void DeletePage(int id)
        {
            Page page = GetPage(id);

            if (page != null)
            {
                foreach (Page relatedPage in page.Related)
                {
                    RemoveLink(page.Id, relatedPage.Id);
                }
                List<PageView> views = page.PageViews.ToList();
                foreach (PageView pageView in views)
                {
                    DeletePageView(pageView.Id);
                }

                Person person = page as Person;

                if (person != null)
                {
                    List<ScoreHistory> scores =
                        _repository.ScoreHistories().Where(p => p.PersonId == person.Id).ToList();
                    foreach (ScoreHistory scoreHistory in scores)
                    {
                        _repository.Delete(scoreHistory);
                    }
                    foreach (Award personAward in person.Awards)
                    {
                        RemoveAward(person.Id, personAward.Id);
                    }
                }

                foreach (PageImage image in page.PageImages)
                {
                    _repository.Delete(image);
                }

                _repository.Delete(page);
            }
        }

        private void DeletePageView(int id)
        {
            PageView pageView = _repository.PageViews().FirstOrDefault(p => p.Id == id);
            _repository.Delete(pageView);
        }

        public bool PageExists(string shortName)
        {
            return _repository.Pages().Any(p => p.ShortName == shortName);
        }

        public Page GetPage(string shortName)
        {
            return _repository.Pages().FirstOrDefault(p => p.ShortName == shortName);
        }

        public ICollection<PageLinkModel> PinnedPages()
        {
            var query =  _repository.Pages().Where(p => p.Pinned);
            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }
            return query.Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName}).ToList();
        }

        public ICollection<PageLinkModel> NewPages()
        {
            var query = _repository.Pages().Where(p => p.PageViews.All(v => v.PlayerId != CurrentPlayer.Id));
            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }
            return query.OrderByDescending(p => p.SignificantUpdate).Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName }).ToList();
        }

        public ICollection<PageLinkModel> ModifiedPages()
        {
            var query =
                _repository.Pages()
                    .Where(p => p.PageViews.Any(v => v.PlayerId == CurrentPlayer.Id && v.Viewed < p.SignificantUpdate));
            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }
            return query.OrderByDescending(p => p.SignificantUpdate).Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName }).ToList();
        }

        public void TogglePagePin(int id)
        {
            Page page = _repository.Pages().FirstOrDefault(p => p.Id == id);
            if (page != null)
            {
                page.Pinned = !page.Pinned;
                Save(page);
            }
        }

        public void MarkAsSeen(int id)
        {
            Page page = _repository.Pages().FirstOrDefault(p => p.Id == id);
            if (page != null)
            {
                PageView pageView = page.PageViews.FirstOrDefault(p => p.PlayerId == CurrentPlayer.Id);
                if (pageView != null)
                {
                    pageView.Viewed = DateTime.Now;
                }
                else
                {
                    pageView = new PageView { PageId = page.Id, PlayerId = CurrentPlayer.Id, Viewed = DateTime.Now };
                }
                _repository.Save(pageView);
            }
        }

        public void ResurrectPerson(int id)
        {
            Person person = _repository.People().FirstOrDefault(p => p.Id == id);
            if (person != null)
            {
                person.IsDead = false;
                Save(person);

                List<int> awardIds = person.Awards.Where(a => a.Trophy.TypeId == (int)TrophyType.DeadAward).Select(t => t.Id).ToList();
                foreach (int awardid in awardIds)
                {
                    RemoveAward(person.Id, awardid);
                }
            }
        }

        public void KillPerson(int id, string obiturary, string causeOfDeath)
        {

            Person person = _repository.People().FirstOrDefault(p => p.Id == id);
            if (person != null)
            {
                person.IsDead = true;
                person.Obiturary = obiturary;
                person.CauseOfDeath = causeOfDeath;
                person.SignificantUpdate = DateTime.Now;
                person.SignificantUpdateById = CurrentPlayer.Id;

                Save(person);

                if (person.Awards.All(a => a.Trophy.TypeId != (int)TrophyType.DeadAward))
                {
                    Trophy trophy = Trophies().FirstOrDefault(t => t.TypeId == (int)TrophyType.DeadAward);
                    {
                        if (trophy != null)
                        {
                            AwardTrophy(person.Id, trophy.Id, string.Format(": {0}", causeOfDeath));
                        }
                    }
                }
            }
        }

        public Trophy GetTrophy(int id)
        {
            return _repository.Trophies().FirstOrDefault(t => t.Id == id);
        }

        public int AddTrophy(string name, string description, int pointsValue, byte[] imageData, string mimeType, bool currentCampaignOnly)
        {
            Trophy trophy = new Trophy();
            trophy.Name = name;
            trophy.Description = description;
            trophy.PointsValue = pointsValue;
            trophy.ImageData = imageData;
            trophy.MimeType = mimeType;

            if (!_authenticatedUser.IsAdmin || currentCampaignOnly)
            {
                trophy.CampaignId = Campaign.Id;
            }
            else
            {
                trophy.CampaignId = null;
            }

            return _repository.Save(trophy);
        }

        public void UpdateTrophy(int id, string name, string description, int pointsValue, byte[] imageData, string mimeType, bool currentCampaignOnly)
        {
            Trophy trophy = GetTrophy(id);
            if (trophy != null)
            {
                if (!_authenticatedUser.IsAdmin)
                {
                    if (!trophy.CurrentCampaignOnly)
                    {
                        return;
                    }
                }

                if (currentCampaignOnly)
                {
                    trophy.CampaignId = Campaign.Id;
                }
                else
                {
                    if (_authenticatedUser.IsAdmin)
                    {
                        trophy.CampaignId = null;
                    }
                }

                trophy.Name = name;
                trophy.Description = description;
                trophy.PointsValue = pointsValue;
                trophy.ImageData = imageData;
                trophy.MimeType = mimeType;

                if (currentCampaignOnly)
                {
                    trophy.CampaignId = Campaign.Id;
                }
                else
                {
                    trophy.CampaignId = null;
                }

                _repository.Save(trophy);
            }
        }

        public void UpdateTrophy(int id, string name, string description, int pointsValue, bool currentCampaignOnly)
        {
            Trophy trophy = GetTrophy(id);
            if (trophy != null)
            {
                if (!_authenticatedUser.IsAdmin)
                {
                    if (!trophy.CurrentCampaignOnly)
                    {
                        return;
                    }
                }

                if (currentCampaignOnly)
                {
                    trophy.CampaignId = Campaign.Id;
                }
                else
                {
                    if (_authenticatedUser.IsAdmin)
                    {
                        trophy.CampaignId = null;
                    }
                }

                trophy.Name = name;
                trophy.Description = description;
                trophy.PointsValue = pointsValue;



                _repository.Save(trophy);
            }
        }

        public ICollection<Trophy> Trophies()
        {
            return _repository.Trophies().OrderBy(t => t.TypeId == (int)TrophyType.DefaultAward).ThenBy(t => t.TypeId).ThenByDescending(t => t.PointsValue).ThenBy(t => t.Name).ToList();
        }

        public void AwardTrophy(int personId, int trophyId, string reason)
        {
            Person person = GetPerson(personId);
            Trophy trophy = GetTrophy(trophyId);
            if (person != null && trophy != null)
            {
                Award award = new Award
                {
                    Trophy = trophy,
                    Reason = reason,
                    AwardedOn = DateTime.Now,
                    NominatedBy = CurrentPlayer
                };
                person.Awards.Add(award);
                Save(person);
            }
        }

        public void DisableFeature(string featureName)
        {
            _feature.DisableFeature(featureName);
        }

        public Person GetPerson(int personId)
        {
            Person person = _repository.People().FirstOrDefault(p => p.Id == personId);
            return person;
        }

        public void SetStats(int personId, Dictionary<StatName, int> stats, string addedRole, List<string> descriptors)
        {
            Person person = GetPerson(personId);
            if (person.PersonStats.Sum(s => s.CurrentValue) == 0)
            {
                person.Roles = addedRole;

                foreach (string descriptor in descriptors)
                {
                    person.AddDescriptor(descriptor, true);
                }

                foreach (KeyValuePair<StatName, int> keyValuePair in stats)
                {

                    PersonStat stat = person.PersonStats.FirstOrDefault(p => p.StatId == (int)keyValuePair.Key);

                    if (stat == null)
                    {
                        stat = new PersonStat { StatId = (int)keyValuePair.Key };
                        person.PersonStats.Add(stat);
                    }

                    stat.CurrentValue = keyValuePair.Value;
                    stat.InitialValue = keyValuePair.Value;
                    stat.PersonId = personId;
                    stat.XpSpent = 0;
                }
                Save(person);
            }
        }

        public void AddRoleToPerson(int personId, string role)
        {
            Person person = GetPerson(personId);
            person.AddRole(role);
            Save(person);
        }

        public void AddDescriptorToPerson(int personId, string descriptor)
        {
            Person person = GetPerson(personId);
            person.AddDescriptor(descriptor);
            Save(person);
        }

        public void BuyStatIncrease(int personId, StatName statName)
        {
            Person person = GetPerson(personId);
            person.BuyStatIncrease(statName);
            Save(person);
        }

        public decimal CurrentMaxPlayerXp()
        {
            decimal xpCap = _repository.People().Where(p => p.PlayerId.HasValue).Max(p => p.XPAwarded);
            return xpCap;
        }

        public void AddXp(int personId, decimal xpValue)
        {
            Person person = GetPerson(personId);
            person.XPAwarded = person.XPAwarded + xpValue;

            if (SiteHasFeature(Feature.XpCatchup))
            {
                decimal playerXpLevel = CurrentMaxPlayerXp();
                if (person.PlayerId.HasValue && person.XPAwarded < playerXpLevel)
                {
                    person.XPAwarded = person.XPAwarded + xpValue;
                    if (person.XPAwarded > playerXpLevel)
                    {
                        person.XPAwarded = playerXpLevel;
                    }
                }
            }

            _repository.Save(person);
        }

        public bool CheckStatPermissions(int personId)
        {
            if (!SiteHasFeature(Feature.SimpleStats))
            {
                return false;
            }

            if (!CurrentPlayerIsGm)
            {
                Person person = GetPerson(personId);
                if (person.PlayerId != CurrentPlayer.Id)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckStatSummaryPermissions()
        {
            if (!SiteHasFeature(Feature.SimpleStats))
            {
                return false;
            }
            return true;
        }

        public List<Person> NpcWithXp()
        {
            return new List<Person>();
            return _repository.People().Where(p => !p.PlayerId.HasValue)
                .Where(p => p.Stats.Sum(s => s.Value) > 10 && p.CurrentXp > 10)
                .OrderByDescending(p => p.CurrentXp).ToList();
        }

        public List<Person> PeopleInGraveyard()
        {
            var query = _repository.People().Where(p => p.IsDead);

            if (ShadowMode)
            {
                query = ApplyPeopleShadow(query);
            }

            return query.OrderBy(s => s.FullName).ToList();
        }

        public bool CurrentUserIsAdmin
        {
            get { return _authenticatedUser.IsAdmin; }
        }

        public bool ShowGraveyard
        {
            get
            {
                return SiteHasFeature(Feature.Graveyard) && _repository.People().Any(p => p.IsDead);
            }
        }

        public bool ShowLeague
        {
            get
            {
                return SiteHasFeature(Feature.CharacterLeague) && _repository.People().Any();
            }
        }

        public bool ShowCharacterSheet
        {
            get { return SiteHasFeature(Feature.CharacterSheet); }
        }

        public bool CurrentUserIsGuest
        {
            get { return _authenticatedUser.IsGuest; }
        }

        public List<UserSetting> UserSettings()
        {
            List<UserSetting> settings = _repository.UserSettings().Where(u => u.PlayerId == CurrentPlayer.Id).ToList();

            foreach (int value in Enum.GetValues(typeof(Setting)))
            {
                if (settings.All(s => s.SettingId != value))
                {
                    settings.Add(new UserSetting
                    {
                        PlayerId = CurrentPlayer.Id,
                        Enabled = false,
                        SettingId = value
                    });
                }
            }
            return settings;
        }

        public bool SettingIsEnabled(Setting setting)
        {
            return _repository.UserSettings().Any(s => s.Enabled && s.PlayerId == CurrentPlayer.Id && s.SettingId == setting.Id);
        }

        public List<Setting> SettingSection(int sectionId)
        {
            return _repository.Settings().Where(s => s.SectionId == sectionId).ToList();
        }

        public int SwitchSetting(int settingId)
        {
            using (TransactionScope transaction = new TransactionScope())
            {

                UserSetting setting =
                    _repository.UserSettings()
                        .FirstOrDefault(s => s.SettingId == settingId && s.PlayerId == CurrentPlayer.Id);
                if (setting == null)
                {
                    setting = new UserSetting
                    {
                        PlayerId = CurrentPlayer.Id,
                        SettingId = settingId
                    };
                }

                setting.Enabled = !setting.Enabled;


                _repository.Save(setting);


                transaction.Complete();
            }

            Setting settingDefinition = _repository.Settings().FirstOrDefault(s => s.Id == settingId);
            if (settingDefinition != null)
            {
                return settingDefinition.SectionId;
            }
            else
            {
                return 0;
            }

        }

        public void EditComment(int commentId, string content)
        {
            Comment comment = _repository.Comments().FirstOrDefault(c => c.Id == commentId);
            if (comment != null)
            {
                if (CurrentUserIsAdmin || comment.CreatedById == CurrentPlayer.Id)
                {
                    comment.Description = content;
                    _repository.Save(comment);
                }
            }
        }

        public void NotifyTurn(int sessionId)
        {
            if (SiteHasFeature(Feature.ImmediateEmailer))
            {
                Player player = PlayerToPostInSession(sessionId);
                Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
                if (player != null && session != null)
                {
                    Setting setting =
                        _repository.Settings().FirstOrDefault(s => s.Name == SettingNames.SendEmailOnMyTurn.ToString());
                    if (player.SettingIsEnabled(setting))
                    {
                        _email.NotifyPlayerTurn(session, player);
                    }
                }
            }
        }

        public List<SiteFeature> AllFeatures()
        {
            return _repository.SiteFeatures().ToList();
        }

        public void EnsureFeatures()
        {
            var allPossibleFeatures = Enum.GetNames(typeof(Feature)).ToList().Where(s => s != "Unknown");
            foreach (string possibleFeature in allPossibleFeatures)
            {

                SiteFeature existing = _repository.SiteFeatures().FirstOrDefault(f => f.Name == possibleFeature);
                if (existing == null)
                {
                    SiteFeature toAdd = new SiteFeature
                    {
                        Description = possibleFeature,
                        Feature = (Feature)Enum.Parse(typeof(Feature), possibleFeature),
                        Name = possibleFeature,
                        IsEnabled = false
                    };
                    _repository.Save(toAdd);
                }

            }
        }

        public List<object> RecentActivity()
        {
            List<PageLinkWithUpdateDateModel> pages = RecentPages().ToList();

            List<Award> awards = RecentAwards();
            List<Comment> comments = RecentComments();

            Dictionary<DateTime, object> dateObject = new Dictionary<DateTime, object>();

            foreach (PageLinkWithUpdateDateModel page in pages)
            {
                if (!dateObject.ContainsKey(page.LastUpdate))
                {
                    dateObject.Add(page.LastUpdate, page);
                }
            }

            foreach (Award award in awards)
            {
                if (!dateObject.ContainsKey(award.AwardedOn))
                {
                    dateObject.Add(award.AwardedOn, award);
                }
            }

            foreach (Comment comment in comments)
            {
                if (!dateObject.ContainsKey(comment.Created))
                {
                    dateObject.Add(comment.Created, comment);
                }
            }

            if (SiteHasFeature(Feature.RumourMill))
            {
                List<Rumour> rumours = _repository.Rumours().OrderByDescending(a => a.Created).Take(20).ToList();

                foreach (Rumour rumour in rumours)
                {
                    if (!dateObject.ContainsKey(rumour.Created))
                    {
                        dateObject.Add(rumour.Created, rumour);
                    }
                }
            }

            var list = dateObject.Keys.ToList();
            list = list.OrderByDescending(l => l).ToList();

            List<object> results = list.Take(20).Select(date => dateObject[date]).ToList();

            if (SiteHasFeature(Feature.RumourMill))
            {
                int numberOfRumours = _repository.Rumours().Count();
                if (numberOfRumours > 10)
                {
                    int randomIndex = new Random().Next(1, numberOfRumours - 1);
                    Rumour rumour = _repository.Rumours().OrderBy(r => r.Id).Skip(randomIndex).Take(1).FirstOrDefault();
                    if (rumour != null)
                    {
                        int position = new Random().Next(1, 19);
                        results.Insert(position, rumour);
                    }
                }
            }

            return results;
        }

        private List<Award> RecentAwards()
        {
            var awardQuery = _repository.Awards().Include(a => a.Person).Include(a => a.Trophy);

            if (ShadowMode)
            {
                awardQuery = ApplyShadow(awardQuery);
            }

            List<Award> awards = awardQuery.OrderByDescending(a => a.AwardedOn).Take(20).ToList();
            return awards;
        }


        public void ToggleSessionPrivacy(int id)
        {
            Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == id);
            if (session != null)
            {
                session.IsPrivate = !session.IsPrivate;
                Save(session);
            }
        }

        public List<ScoreHistory> PersonScoreHistory(int id)
        {
            return _repository.ScoreHistories().Where(s => s.PersonId == id).OrderBy(a => a.DateTime).ToList();
        }

        public List<PageListItemModel> NpcList()
        {
            var query = _repository.People();

            if (ShadowMode)
            {
                query = ApplyPeopleShadow(query);
            }
                return query.Where(p => !p.PlayerId.HasValue)
                    .Select(p => new PageListItemModel { Id = p.Id, Fullname = p.FullName })
                    .OrderBy(p => p.Fullname)
                    .ToList();
        }

        public List<FateAspect> GetAspects(int id)
        {
            return _repository.FateAspects().Where(a => a.PersonId == id).OrderBy(a => a.AspectType).ToList();
        }

        public void SaveAspects(List<FateAspect> fateAspects)
        {
            foreach (FateAspect fateAspect in fateAspects)
            {
                if (!string.IsNullOrWhiteSpace(fateAspect.AspectName))
                {
                    _repository.Save(fateAspect);
                }
                else
                {
                    if (fateAspect.Id > 0)
                    {
                        _repository.Delete(fateAspect);
                    }
                }
            }
        }

        public List<FateStat> GetFateStats(int id)
        {
            return _repository.FateStats().Where(f => f.PersonId == id).OrderBy(f => f.StatType).ToList();
        }

        public void SaveFateStats(IEnumerable<FateStat> fateStats)
        {
            foreach (FateStat fateStat in fateStats)
            {
                _repository.Save(fateStat);
            }
        }

        public List<FateStunt> GetStunts(int id)
        {
            return _repository.FateStunts().Where(s => s.PersonId == id).ToList();
        }

        public void SaveStunt(FateStunt stunt)
        {
            _repository.Save(stunt);
        }

        public void DeleteStunt(int stuntId)
        {
            FateStunt stunt = _repository.FateStunts().FirstOrDefault(s => s.Id == stuntId);
            if (stunt != null)
            {
                _repository.Delete(stunt);
            }
        }

        public void ToggleStuntVisibility(int stuntId)
        {
            FateStunt stunt = _repository.FateStunts().FirstOrDefault(s => s.Id == stuntId);
            if (stunt != null)
            {
                stunt.IsVisible = !stunt.IsVisible;
                _repository.Save(stunt);
            }
        }

        public List<Award> GetLatestAwards(int count)
        {
            List<Award> awards = _repository.Awards().OrderByDescending(d => d.AwardedOn).Take(count).ToList();
            return awards;
        }

        public List<ScoreHistory> GetCurrentScoresForPerson(int id)
        {
            return _repository.ScoreHistories().Where(s => s.PersonId == id).OrderBy(s => s.ScoreTypeId).ToList();

        }

        public void SetDefaultHitPoints(int id)
        {
            Person person = GetPerson(id);
            if (person != null)
            {
                if (!person.SimpleHitPoints.Any(s => s.Purchased.HasValue))
                {
                    BuyHitPointSlot(id, SimpleHitPointLevel.Slight, SimpleHitPointType.Harm, free: true);
                    BuyHitPointSlot(id, SimpleHitPointLevel.Significant, SimpleHitPointType.Harm, free: true);
                    BuyHitPointSlot(id, SimpleHitPointLevel.Slight, SimpleHitPointType.Wear, free: true);
                    BuyHitPointSlot(id, SimpleHitPointLevel.Significant, SimpleHitPointType.Wear, free: true);
                }
            }
        }

        public void BuyHitPointSlot(int id, SimpleHitPointLevel level, SimpleHitPointType type, bool free = false)
        {
            Person person = GetPerson(id);
            if (person != null)
            {
                if (free || person.CanBuyHitSlot(level, type))
                {
                    if (!free)
                    {
                        int cost = person.HitSlotCost(level, type);
                        person.XpSpent = person.XpSpent + cost;
                    }

                    SimpleHitPoint hitPoint =
                        person.SimpleHitPoints.FirstOrDefault(
                            p => p.SimpleHitPointLevel == level && p.SimpleHitPointType == type);
                    if (hitPoint == null)
                    {
                        hitPoint = new SimpleHitPoint { SimpleHitPointLevel = level, SimpleHitPointType = type };
                        person.SimpleHitPoints.Add(hitPoint);
                    }

                    hitPoint.Purchased = DateTime.Now;

                    Save(person);
                }
            }
        }

        public void AddXpForSession(int sessionId, decimal xpAwarded)
        {
            Session session = _repository.Pages().OfType<Session>().Include(p => p.Pages).FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                List<Person> people = session.People.Where(p => !p.IsDead).ToList();

                if (!SiteHasFeature(Feature.SoloXp))
                {
                    //always award all players regardless - just to be fair
                    List<Person> playerCharacters = _repository.People().Where(p => p.PlayerId.HasValue && !p.IsDead).ToList();

                    foreach (Person person in playerCharacters)
                    {
                        if (people.All(p => p.Id != person.Id))
                        {
                            people.Add(person);
                        }
                    }
                }

                foreach (Person person in people)
                {
                    if (SiteHasFeature(Feature.PostBonusXp))
                    {
                        decimal postBonus =
                            _repository.Posts().Count(p => p.CharacterId == person.Id && p.SessionId == sessionId)*0.01m;
                        AddXp(person.Id, xpAwarded + postBonus);
                    }
                    else
                    {
                        AddXp(person.Id, xpAwarded);
                    }

                    
                }

                Save(session);
            }
        }

        public void AddPlayer(string name, string email)
        {
            Player player = new Player
            {
                DisplayName = name,
                UserName = email
            };
            _repository.Save(player);

            DateTime createDate = DateTime.Now;

            List<int> ids = _repository.Pages().Select(p => p.Id).ToList();

            foreach (int pageId in ids)
            {
                var view = new PageView
                {
                    PageId = pageId,
                    PlayerId = player.Id,
                    Viewed = createDate
                };
                _repository.Save(view);
            }
        }

        public List<ExceptionLog> GetExceptionLogs(int count)
        {
            return _repository.ExceptionLogs().OrderByDescending(l => l.DateTime).Take(count).ToList();
        }

        public void ResetNpcStats(int id)
        {
            Person person = _repository.People().FirstOrDefault(p => p.Id == id);
            if (person != null && !person.PlayerId.HasValue)
            {
                person.Descriptors = "";
                person.Roles = "";

                person.XpSpent = 0;

                foreach (SimpleHitPoint simpleHitPoint in person.SimpleHitPoints)
                {
                    simpleHitPoint.Purchased = null;
                    simpleHitPoint.XpCost = 0;
                }

                Save(person);

                List<PersonStat> stats = person.PersonStats.ToList();
                foreach (PersonStat personStat in stats)
                {
                    _repository.Delete(personStat);
                }
            }
        }

        public List<Page> AllPages()
        {
            return _repository.Pages().ToList();
        }

        public decimal GetDefaultXpForPage(Page page, out int sessionId)
        {
            decimal xp = 0;
            sessionId = 0;

            Session session = page as Session;
            if (session != null)
            {
                if (!session.XpAwarded.HasValue)
                {
                    session.XpAwarded = DateTime.Now;
                    xp = 0.5m;
                    sessionId = session.Id;
                }
            }

            SessionLog log = page as SessionLog;
            if (log != null)
            {
                if (!log.XpAwarded.HasValue)
                {
                    xp = 0.25m;
                    sessionId = log.SessionId ?? 0;

                    decimal wordBonus = (decimal)log.Session.WordCount / 2000 * (log.Session.IsTextSession ? 1 : 2);
                    if (wordBonus > 0.75m)
                    {
                        wordBonus = 0.75m;
                    }

                    xp = xp + wordBonus;

                    log.XpAwarded = DateTime.Now;
                    //    Save(log);
                }
            }

            return xp;
        }

        public void AddDefaultXp(int pageId)
        {
            Page page = GetPage(pageId);

            int sessionId;
            decimal xp = GetDefaultXpForPage(page, out sessionId);


            if (xp > 0 && sessionId > 0)
            {
                AddXpForSession(sessionId, xp);
            }
        }

        public List<PageListItemModel> FullPageList()
        {
            var query = _repository.Pages();

            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }

            return query.OrderBy(p => p.FullName)
                .ThenBy(p => p.ShortName)
                .Select(p => new PageListItemModel
                {
                    Id = p.Id,
                    Fullname = p.FullName,
                    ShortName = p.ShortName
                }).ToList();
        }

        public List<Page> PagesWithOutstandingXp()
        {
            List<Session> sessions = _repository.Pages().OfType<Session>().Where(p => !p.XpAwarded.HasValue && ((p.IsClosed && p.IsTextSession) || !p.IsTextSession)).ToList();
            List<SessionLog> logs = _repository.Pages().OfType<SessionLog>().Where(p => !p.XpAwarded.HasValue).ToList();
            List<Page> pages = new List<Page>();
            pages.AddRange(sessions);
            pages.AddRange(logs);
            return pages.OrderBy(p => p.AgeInDays).ToList();
        }

        public List<Person> NpcsWithStats()
        {
            return
                _repository.People()
                .Where(p => p.PlayerId == null)
                .Where(p => p.PersonStats.Any())
                .Where(p => !p.IsDead)
                .OrderByDescending(p => p.CurrentScore).ToList();
        }

        public PageImage SaveImage(int pageId, byte[] image)
        {
            PageImage pageImage = new PageImage
            {
                Data = image,
                PageId = pageId
            };
            int id = _repository.Save(pageImage);
            return _repository.PageImages().FirstOrDefault(p => p.Id == id);
        }

        public PageImage GetPageImage(int id)
        {
            return _repository.PageImages().FirstOrDefault(p => p.Id == id);
        }

        public void SetPageXpAwarded(int pageId)
        {
            Page page = _repository.Pages().FirstOrDefault(p => p.Id == pageId);
            if (page != null)
            {
                Session session = page as Session;
                if (session != null)
                {
                    if (!session.XpAwarded.HasValue)
                    {
                        session.XpAwarded = DateTime.Now;
                    }
                }

                SessionLog log = page as SessionLog;
                if (log != null)
                {
                    if (!log.XpAwarded.HasValue)
                    {
                        log.XpAwarded = DateTime.Now;
                    }
                }

                Save(page);
            }
        }

        public List<Creature> Creatures()
        {
            return _repository.Pages().OfType<Creature>().ToList();
        }

        public int AddCreature(CreateCreatureViewModel creatureModel)
        {
            Creature creature = new Creature
            {
                ParentType = creatureModel.ParentId,
                ShortName = creatureModel.Name,
                FullName = creatureModel.Name,
                Description = creatureModel.Description,
                ThreatLevel = creatureModel.ThreatLevel
            };
            return Save(creature);
        }

        public int AddOrganisation(string name, string description)
        {
            Organisation org = new Organisation
            {
                ShortName = name,
                FullName = name,
                Description = description
            };
            return Save(org);
        }

        public List<PriceListItem> PriceList()
        {
            List<PriceListItem> items = _repository.PriceListItems().ToList();

            foreach (PriceListItem priceListItem in items)
            {
                priceListItem.AllItems = items;
            }
            return items.OrderBy(p => p.Breadcrumb).ToList();
        }

        public void SavePriceList(List<PriceListItem> priceListItems)
        {
            foreach (PriceListItem item in priceListItems)
            {
                PriceListItem existing = _repository.PriceListItems().FirstOrDefault(p => p.Id == item.Id);
                if (existing != null)
                {
                    if (item.ParentId.HasValue)
                    {
                        if (existing.AllChildren().Select(c => c.Id).ToList().Contains(item.ParentId.Value))
                        {
                            item.ParentId = null;
                        }
                    }
                    existing.Name = item.Name;
                    existing.ParentId = item.ParentId;
                    existing.PriceInPence = item.PriceInPence;
                    existing.Description = item.Description;
                    _repository.Save(existing);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.Name))
                    {
                        _repository.Save(item);
                    }
                }

            }
        }

        public CampaignDetail GetCampaginDetails()
        {
            return _repository.CampaignDetails().FirstOrDefault();
        }

        public void SetDetails(int personId, int crowns, int shillings, int pennies, DateTime dob, string height, int upkeep)
        {
            Person person = GetPerson(personId);
            if (person != null)
            {
                person.Crowns = crowns;
                person.Shillings = shillings;
                person.Pennies = pennies;
                person.DateOfBirth = dob;
                person.Height = height;
                person.Upkeep = upkeep;
                Save(person);
            }
        }

        public List<Rumour> GetAllRumours()
        {
            return _repository.Rumours().OrderByDescending(r => r.Created).ToList();
        }

        public void SaveRumours(List<Rumour> rumours)
        {
            foreach (Rumour item in rumours)
            {
                Rumour existing = _repository.Rumours().FirstOrDefault(p => p.Id == item.Id);
                if (existing != null)
                {

                    existing.Title = item.Title;
                    existing.PlaceId = item.PlaceId;
                    existing.Description = item.Description;
                    _repository.Save(existing);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.Title) && !string.IsNullOrWhiteSpace(item.Description))
                    {
                        item.Created = DateTime.Now;
                        _repository.Save(item);
                    }
                }
            }
        }

        public void DeleteRumour(int id)
        {
            Rumour rumour = _repository.Rumours().FirstOrDefault(r => r.Id == id);
            if (rumour != null)
            {
                _repository.Delete(rumour);
            }
        }

        public List<Rumour> GetRumoursForPlace(int placeId)
        {
            List<Rumour> rumours = _repository.Rumours().Where(r => r.PlaceId == placeId || r.Place.Parent.Id == placeId || r.Place.Child.Any(p => p.Id == placeId)).ToList();

            return rumours;
        }

        public void SetAge(int personId, int age)
        {
            CampaignDetail campaign = GetCampaginDetails();
            if (campaign.CurrentGameDate.HasValue)
            {
                DateTime dateOfBirth = campaign.CurrentGameDate.Value.AddYears(-age);

                int months = new Random().Next(0, 11);
                int days = new Random().Next(0, 28);

                dateOfBirth = dateOfBirth.AddMonths(-months);
                dateOfBirth = dateOfBirth.AddDays(-days);

                SetDob(personId, dateOfBirth);
            }
        }

        public void SetDob(int personId, DateTime dateOfBirth)
        {
            Person person = GetPerson(personId);
            if (person != null)
            {
                person.DateOfBirth = dateOfBirth;
                Save(person);
            }
        }

        public void SetHeight(int personId, string height)
        {
            Person person = GetPerson(personId);
            if (person != null)
            {
                person.Height = height;
                Save(person);
            }
        }

        public void SetMoney(int personId, int crowns, int shillings, int pennies, int upkeep)
        {
            Person person = GetPerson(personId);
            if (person != null)
            {
                person.Crowns = crowns;
                person.Shillings = shillings;
                person.Pennies = pennies;
                person.Upkeep = upkeep;
                Save(person);
            }
        }

        public void SetGameDate(DateTime? currentGameDate)
        {
            CampaignDetail detail = GetCampaginDetails();
            detail.CurrentGameDate = currentGameDate;
            _repository.Save(detail);

        }

        public void AddDayToGameDate()
        {
            CampaignDetail detail = GetCampaginDetails();
            if (detail.CurrentGameDate.HasValue)
            {
                DateTime original = detail.CurrentGameDate.Value.Date;
                detail.CurrentGameDate = detail.CurrentGameDate.Value.Date.AddDays(1);
                _repository.Save(detail);
                ProcessUpkeep(original, detail.CurrentGameDate.Value);
            }

        }

        private void ProcessUpkeep(DateTime originalDate, DateTime currentDate)
        {
            int daysElapsed = (currentDate - originalDate).Days;
            if (daysElapsed > 0)
            {
                List<Person> peopleWithUpkeep =
                    _repository.People().Where(p => (p.Upkeep.HasValue && p.Upkeep.Value != 0) || p.Assets.Sum(a => a.Upkeep) != 0).ToList();

                foreach (Person person in peopleWithUpkeep)
                {
                    int amount = person.Assets.Sum(a => a.Upkeep);
                    if (person.Upkeep.HasValue)
                    {
                        amount = amount - person.Upkeep.Value;
                    }

                    if (amount != 0)
                    {
                        if (amount > 0)
                        {
                            int amountToAdd = amount * daysElapsed;
                            person.AddMoney(amountToAdd);
                        }
                        else
                        {
                            int amountToDeduct = 0 - (amount * daysElapsed);
                            person.DeductMoney(amountToDeduct);
                        }

                        Save(person);
                    }
                }
            }
        }

        public void AddWeekToGameDate()
        {
            CampaignDetail detail = GetCampaginDetails();
            if (detail.CurrentGameDate.HasValue)
            {
                DateTime original = detail.CurrentGameDate.Value.Date;
                detail.CurrentGameDate = detail.CurrentGameDate.Value.Date.AddDays(7);
                _repository.Save(detail);
                ProcessUpkeep(original, detail.CurrentGameDate.Value);
            }
        }

        public void AddMonthToGameDate()
        {
            CampaignDetail detail = GetCampaginDetails();
            if (detail.CurrentGameDate.HasValue)
            {
                DateTime original = detail.CurrentGameDate.Value.Date;
                detail.CurrentGameDate = detail.CurrentGameDate.Value.Date.AddMonths(1);
                _repository.Save(detail);
                ProcessUpkeep(original, detail.CurrentGameDate.Value);
            }
        }

        public void SetAssets(int personId, List<Asset> assets)
        {
            foreach (Asset asset in assets.ToList())
            {
                Asset originalAsset = _repository.Assets().FirstOrDefault(a => a.Id == asset.Id);
                if (originalAsset != null)
                {
                    if (asset.Delete)
                    {
                        _repository.Delete(originalAsset);
                    }
                    else
                    {
                        originalAsset.Upkeep = asset.Upkeep;
                        originalAsset.Description = asset.Description;
                        originalAsset.Title = asset.Title;
                        _repository.Save(originalAsset);
                    }
                }
            }
        }

        public void AddAsset(int personId, string title, string description, int upkeep)
        {
            Asset asset = new Asset
            {
                Title = title,
                Description = description,
                Upkeep = upkeep,
                PersonId = personId
            };
            _repository.Save(asset);
        }

        public void SpendMoney(int personId, int spendCrowns, int spendShillings, int spendPence)
        {
            Person person = _repository.People().FirstOrDefault(p => p.Id == personId);

            if (person != null)
            {
                if (person.Crowns > spendCrowns && person.Shillings > spendShillings && person.Pennies > spendPence)
                {
                    person.Crowns = person.Crowns - spendCrowns;
                    person.Shillings = person.Shillings - spendShillings;
                    person.Pennies = person.Pennies - spendPence;
                }
                else
                {
                    int amountToDeduct = (spendCrowns * 240) + (spendShillings * 12) + spendPence;
                    person.DeductMoney(amountToDeduct);
                }

                Save(person);
            }

        }

        public void AddMoney(int personId, int addCrowns, int addShillings, int addPence)
        {
            Person person = _repository.People().FirstOrDefault(p => p.Id == personId);

            if (person != null)
            {
                if (addPence > 240 && addCrowns == 0)
                {
                    int amountToAdd = (addCrowns * 240) + (addShillings * 12) + addPence;
                    person.DeductMoney(amountToAdd);
                }
                else
                {
                    person.Crowns = person.Crowns + addCrowns;
                    person.Shillings = person.Shillings + addShillings;
                    person.Pennies = person.Pennies + addPence;
                }

                Save(person);
            }

        }

        public int GetGmId()
        {
            return Gm.Id;
        }

        public void SetGmSuspended(int sessionId, bool suspended)
        {
            Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                session.GmIsSuspended = suspended;
                Save(session);
            }
        }

        public void SetPlayerSuspended(int sessionId, int playerId, bool suspended)
        {
            Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                PostOrder postOrder = session.PostOrders.FirstOrDefault(p => p.PlayerId == playerId);
                if (postOrder != null)
                {
                    postOrder.IsSuspended = suspended;
                    Save(session);
                }                
            }
        }

        public List<PageLinkModel> GetRelatedPages(int id)
        {

            if (SiteHasFeature(Feature.ShadowMode) && PlayerSettingEnabled(SettingNames.ShadowMode))
            {
                List<PageLinkModel> shadowLinks = _repository.Pages()
                    .Include(p => p.Pages)
                    .Single(p => p.Id == id)
                    .Pages.Where(p => MyPageIds.Contains(p.Id) || p.Pages.Any(r => MyPageIds.Contains(r.Id)))
                    .Select(p => new PageLinkModel {Id = p.Id, ShortName = p.ShortName, FullName = p.FullName})
                    .ToList();

                return shadowLinks;
            }

            List<PageLinkModel> links =
                _repository.Pages()
                    .Include(p => p.Pages)
                    .Single(p => p.Id == id)
                    .Pages.Select(p => new PageLinkModel {Id = p.Id, ShortName = p.ShortName, FullName = p.FullName})
                    .ToList();
            return links;
        }

        public bool PlayerSettingEnabled(SettingNames setting)
        {
            Player player = CurrentPlayer;
             Setting theSetting =  _repository.Settings().FirstOrDefault(s => s.Name == setting.ToString());

            return player.SettingIsEnabled(theSetting);
        }

        public List<Award> AwardsForTrophy(int id)
        {
            List<Award> awards = _repository.Awards().Where(t => t.TrophyId == id).ToList();
            return awards;
        }

        public List<Player> GetAllPlayers()
        {
            return _repository.Players().OrderBy(p => p.DisplayName).ToList();
        }

        public void SetSessionGm(int sessionId, int? selectedGm)
        {
            Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                session.GmId = selectedGm;
                Save(session);
            }
        }

        public List<PageLinkModel> PeopleWithXpToSpend()
        {
            return _repository.People().Where(p => !p.PlayerId.HasValue && p.XpSpendAvailable).Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName } ).ToList();
        }

        public void AwardShiftForSession(int id)
        {
            Session session = _repository.Pages().OfType<Session>().Include(s => s.Pages).FirstOrDefault(s => s.Id == id);
            if (session != null)
            {
                foreach (Person person in session.People.ToList())
                {
                    person.HasAttributeMoveAvailable = true;
                    Save(person);
                }
            }
        }

        public void UpdateAward(int id, string awardReason)
        {
            Award award = _repository.Awards().FirstOrDefault(a => a.Id == id);
            if (award != null)
            {
                award.Reason = awardReason;
                _repository.Save(award);
            }
        }

        public List<Person> GetNpcSheetPeople()
        {
            return _repository.People()
                .Include(p => p.PersonAttributes)
                .Include(p => p.Awards)
                .Where(p => p.PersonAttributes.Any()).ToList();
        }

        public void RemoveAward(int personId, int awardId)
        {
            Person person = GetPerson(personId);
            if (person != null)
            {
                Award award = person.Awards.FirstOrDefault(a => a.Id == awardId);
                if (award != null)
                {
                    _repository.Delete(award);
                }
            }
        }

        public PageLinkModel PersonWithMyAward(TrophyType awardType)
        {
           return _repository.People().Where(p => p.Awards.Any(a => a.NominatedById == CurrentPlayer.Id
                                                                  && a.Trophy.TypeId == (int)awardType)).Select(p => new PageLinkModel
            {
                FullName = p.FullName,
                Id = p.Id,
                ShortName = p.ShortName
            }).FirstOrDefault();

        }

        public List<Page> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Page>();
            }

            var filterPredicate = PredicateBuilder.True<Page>();
            var postPredicate = PredicateBuilder.True<Session>();



            string[] terms = searchTerm.Split(' ');

            foreach (string term in terms)
            {
                var termPredicate = PredicateBuilder.False<Page>();
                string temp = term;
                termPredicate = termPredicate.Or(p => p.ShortName.Contains(temp));
                termPredicate = termPredicate.Or(p => p.FullName.Contains(temp));
                termPredicate = termPredicate.Or(p => p.PlainText.Contains(temp));
                filterPredicate = filterPredicate.And(termPredicate.Expand());

                var postTermPredicate = PredicateBuilder.False<Session>();
                postTermPredicate = postTermPredicate.Or(s => s.Posts.Any(p => !p.IsRevised && p.OriginalContent.Contains(temp)));
                postTermPredicate = postTermPredicate.Or(s => s.Posts.Any(p => p.IsRevised && p.RevisedContent.Contains(temp)));
                postPredicate = postPredicate.And(postTermPredicate.Expand());
            }

            var sessionQuery = _repository.Pages().OfType<Session>();
            sessionQuery = sessionQuery.AsExpandable().Where(postPredicate.Expand());
            List<int> sessionIds = sessionQuery.Select(s => s.Id).ToList();

            filterPredicate = filterPredicate.Or(p => sessionIds.Any(s => s == p.Id));

            var query = _repository.Pages();

            query = query.AsExpandable().Where(filterPredicate.Expand());

            string spacedTerm = " " + searchTerm + " ";
            string startedTerm = " " + searchTerm;
            string endedTerm = searchTerm + " ";

            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }

            return
               query.OrderByDescending(p => p.ShortName == searchTerm)
                    .ThenByDescending(p => p.FullName == searchTerm)
                    .ThenByDescending(p => p.ShortName.StartsWith(searchTerm))
                    .ThenByDescending(p => p.FullName.StartsWith(searchTerm))
                    .ThenByDescending(p => p.ShortName.Contains(searchTerm))
                    .ThenByDescending(p => p.FullName.Contains(searchTerm))
                    .ThenByDescending(p => p.PlainText.Contains(spacedTerm))
                    .ThenByDescending(p => p.PlainText.Contains(startedTerm))
                    .ThenByDescending(p => p.PlainText.Contains(endedTerm))
                    .ThenByDescending(p => p.FullName)
                    .Take(15).ToList();
        }

        public bool PageExists(string shortName, string fullName)
        {
            return _repository.Pages().Any(p => p.ShortName == shortName && p.FullName == fullName);
        }

        public int AddComment(int pageId, string description)
        {

            return AddComment(pageId, description, false, null);
        }

        public int AddComment(int pageId, string description, int personId)
        {
            return AddComment(pageId, description, false, personId);
        }

        public int AddComment(int pageId, string description, bool isAdmin)
        {
            return AddComment(pageId, description, isAdmin, null);
        }

        public List<PageLinkModel> TopNpcs()
        {
            var query = _repository.People()
                .Where(p => !p.PlayerId.HasValue);

            if (ShadowMode)
            {
                query = ApplyPeopleShadow(query);
            }

            query = query.OrderByDescending(p => p.CurrentScore).Take(5);


            return query.Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName }).ToList();
        }

        public List<Person> AllNpcs()
        {
            var query = _repository.People().Where(p => !p.PlayerId.HasValue);

            if (ShadowMode)
            {
                query = ApplyPeopleShadow(query);
            }

            return query.OrderBy(n => n.ShortName).ToList();
        }

        public void SetMyAward(int personId, TrophyType trophyType)
        {
            Person person = GetPerson(personId);
            Trophy trophy = Trophies().FirstOrDefault(t => t.TypeId == (int)trophyType);
            if (person != null && trophy != null)
            {
                List<int> typesToRemove = GetExlusiveTrophyTypes(trophyType);
                List<Award> currentAwards = _repository.Awards().Where(a => ((typesToRemove.Contains(a.Trophy.TypeId) && a.PersonId == personId) || a.Trophy.TypeId == (int)trophyType) && a.NominatedById == CurrentPlayer.Id).ToList();
                foreach (Award award in currentAwards)
                {
                    RemoveAward(award.PersonId, award.Id);
                }

                AwardTrophy(personId, trophy.Id, ": Awarded by " + CurrentPlayer.DisplayName);
            }
        }

        public bool IsLoggedIn()
        {
            return CurrentPlayer != null;
        }

        public void OpenOrCloseTextSession(int id)
        {
            Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == id);
            if (session != null)
            {
                session.IsClosed = !session.IsClosed;
                Save(session);

                if (SiteHasFeature(Feature.SimpleStats) && session.IsClosed && SiteHasFeature(Feature.SimpleAutoXp))
                {
                    ApplyXpForSession(session);
                }
                if (SiteHasFeature(Feature.PersonAttributes) && session.IsClosed)
                {
                    RefreshAttributeMoves(session);
                }                      
            }
        }

        private void RefreshAttributeMoves(Session session)
        {
            foreach (Person person in session.People)
            {
                person.HasAttributeMoveAvailable = true;
                Save(person);
            }
        }

        private void ApplyXpForSession(Session session)
        {
            List<Person> people = People().ToList();
            foreach (Person person in people)
            {
                if (!person.IsDead)
                {
                    person.XPAwarded++;
                    if (session.People.Contains(person))
                    {
                        person.XPAwarded++;
                    }
                    if (session.Posts.Any(p => p.CharacterId == person.Id))
                    {
                        person.XPAwarded++;
                    }
                    Save(person);
                }
            }
        }

        public void ToggleSetAsTextSession(int id)
        {
            Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == id);
            if (session != null)
            {
                session.IsTextSession = !session.IsTextSession;
                session.IsGmTurn = true;
                Save(session);
            }
        }

        public List<Session> UpdatedTextSessions()
        {
            List<Session> pages =
                 _repository.Pages()
                     .OfType<Session>().Where(p => p.IsTextSession && !p.IsClosed).ToList();

            if (!CurrentPlayerIsGm)
            {
                //pages = pages.Where(p => p.PlayerCharacters.Any(c => c.PlayerId == CurrentPlayer.Id)).ToList();
                pages = pages.Where(p => p.PlayerCharacters.Any(c => c.PlayerId == CurrentPlayer.Id)).ToList();
            }

            pages =
                pages.Where(
                    p =>
                        p.Posts.Any() &&
                        p.Posts.OrderByDescending(ps => ps.DatePosted).FirstOrDefault().PlayerId != CurrentPlayer.Id).ToList();

            return pages.OrderBy(p => p.LastPostTime).ToList();
        }

        public List<Session> TextSessionsWhereItisMyTurn()
        {
            bool isGm = CurrentPlayerIsGm;
            List<Session> pages =
                 _repository.Pages()
                     .OfType<Session>().Where(p => p.IsTextSession && !p.IsClosed && p.PostOrders.Any(po => po.PlayerId == CurrentPlayer.Id && !po.IsSuspended || isGm))
                        .ToList();

            if (isGm)
            {
                return pages.Where(p => p.IsGmTurn && !p.GmIsSuspended).OrderBy(p => p.LastPostTime).ToList();
            }
            else
            {
                return pages.Where(p => _factory.GetSession(p.Id).CurrentPlayerId == CurrentPlayer.Id).OrderBy(p => p.LastPostTime).ToList();
            }

            //return pages.Where(p => _factory.GetSession(p.Id).CurrentPlayerId == CurrentPlayer.Id || (p.IsGmTurn && isGm && p.GmIsSuspended == 0)).OrderBy(p => p.LastPostTime).ToList();
        }

        [Obsolete("Seriously... just no...", true)]
        public List<Session> TextSessionsContainingMyCharacters()
        {
            List<Session> pages =
                 _repository.Pages()
                     .OfType<Session>().Where(p => p.IsTextSession).ToList();

            return pages.Where(p => p.PlayerCharacters.Any(c => c.PlayerId == CurrentPlayer.Id || CurrentPlayerIsGm)).OrderBy(p => p.LastPostTime).ToList();
        }

        public void EnsurePostOrders(int sessionId)
        {
            Session session = GetPage(sessionId) as Session;
            if (session != null)
            {
                List<int> playerIds = session.PlayerCharacters.Where(p => p.PlayerId.HasValue).Select(p => p.PlayerId.Value).ToList();
                foreach (int playerId in playerIds)
                {
                    if (session.PostOrders.All(s => s.PlayerId != playerId))
                    {
                        session.PostOrders.Add(new PostOrder { SessionId = sessionId, PlayerId = playerId, LastTurnEnded = DateTime.Now });
                        _repository.Save(session);
                    }
                }
            }
        }

        public List<Comment> RecentComments()
        {
            var query = _repository.Comments();

            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }
            return query.OrderByDescending(d => d.Created).Take(20).ToList();
        }



        public Player MyPlayer()
        {
            return _repository.Players().FirstOrDefault(p => p.UserName == _authenticatedUser.UserName);
        }

        public void DeleteComment(int commentId)
        {
            Comment comment = _repository.Comments().FirstOrDefault(c => c.Id == commentId);
            if (comment != null)
            {
                if (CurrentUserIsAdmin || comment.CreatedById == CurrentPlayer.Id)
                {
                    _repository.Delete(comment);
                }
            }
        }

        public List<Person> GetLeague()
        {
            List<Person> people = new List<Person>();
            if (SiteHasFeature(Feature.CharacterLeague))
            {
                var query =_repository.People().Where(p => p.CurrentScore > 0);

                if (ShadowMode)
                {
                    query = ApplyPeopleShadow(query);
                }

                if (!SiteHasFeature(Feature.PublicLeague) && !(SiteHasFeature(Feature.AdminLeague) && CurrentUserIsAdmin))
                {
                    query = query.Where(p => !p.PlayerId.HasValue || p.PlayerId == CurrentPlayer.Id);
                }

                people = query.ToList()
                    .OrderByDescending(s => s.PointsValue)
                    .ThenByDescending(s => s.Modified).ToList();
            }
            return people;
        }

        public List<PageLinkModel> OtherPCs()
        {
            var query = _repository.People().Where(p => p.PlayerId.HasValue && p.PlayerId != CurrentPlayer.Id && !p.IsDead);

            if (ShadowMode)
            {
                query = ApplyPeopleShadow(query);
            }

            return query.Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName }).ToList();
        }



        private List<int> GetExlusiveTrophyTypes(TrophyType trophyType)
        {
            List<int> favAwardId = new List<int>
            {
                (int)TrophyType.FirstFavouriteNpc,
                (int)TrophyType.SecondFavouriteNpc,
                (int)TrophyType.ThirdFavouriteNpc,
            };

            if (favAwardId.Contains((int)trophyType))
            {
                return favAwardId;
            }

            return new List<int> { (int)trophyType };
        }

        private int AddComment(int pageId, string description, bool isAdmin, int? personId)
        {
            Page page = GetPage(pageId);

            if (page != null)
            {
                Comment comment = new Comment();
                comment.Created = DateTime.Now;
                comment.Description = description;
                comment.IsAdmin = isAdmin;
                comment.PersonId = personId;
                comment.CreatedById = CurrentPlayer.Id;
                page.Comments.Add(comment);
                int updatedPageId = Save(page);

                string commenterName = CurrentPlayer.DisplayName;
                if (personId.HasValue)
                {
                    Person person = GetPerson(personId.Value);
                    commenterName = person.FullName;
                }


                NotifyAddComment(pageId, commenterName, description);
                return updatedPageId;

            }
            return 0;
        }

        public Player PlayerToPostInSession(int sessionId)
        {
            Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                if (session.IsGmTurn && !session.GmIsSuspended)
                {
                    return SessionGm(session);
                }
                else
                {
                    PostOrder postOrder = session.PostOrders.Where(p => !p.IsSuspended).OrderBy(po => po.LastTurnEnded).FirstOrDefault();
                    if (postOrder == null && !session.GmIsSuspended)
                    {
                        return SessionGm(session);
                    }

                    return postOrder != null ? postOrder.Player : null;
                }
            }
            else
            {
                return null;
            }
        }

        private Player SessionGm(Session session)
        {
            if (session.GmId.HasValue)
            {
                return GetPlayer(session.GmId.Value);
            }

            return Gm;
        }

        private Player GetPlayer(int playerId)
        {
            return _repository.Players().FirstOrDefault(p => p.Id == playerId);
        }

        public List<Session> OpenTextSessions()
        {
            return _repository.Pages()
                .OfType<Session>()
                .Include(s => s.Posts)
                .Include(s => s.PageViews)
                .Where(s => s.IsTextSession && !s.IsClosed).ToList();
        }

        public List<Session> MyOpenTextSessions()
        {
            return
                OpenTextSessions()
                    .Where(s => s.PlayerCharacters.Any(p => p.PlayerId == CurrentPlayer.Id) || CurrentPlayerIsGm || s.GmId == CurrentPlayer.Id).ToList();
        }

        [Obsolete("Seriously... just no...", true)]
        public List<Session> ModifiedTextSessions()
        {           
            return _repository.Pages().OfType<Session>().ToList().Where(p => p.PageViews.Any(v => v.PlayerId == CurrentPlayer.Id && v.Viewed < p.LastPostTime)).ToList();
        }

        public bool SiteHasFeature(Feature featureName)
        {
            return _feature.SiteHasFeature(featureName);

        }

        public void EnableFeature(string featureName)
        {
            _feature.EnableFeature(featureName);
        }
    }
}
