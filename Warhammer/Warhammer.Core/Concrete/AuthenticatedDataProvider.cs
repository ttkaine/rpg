﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Transactions;
using LinqKit;
using SendGrid;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Extensions;
using Warhammer.Core.Models;
using Warhammer.Core.Models.Reports;

namespace Warhammer.Core.Concrete
{

    public class AuthenticatedDataProvider : IAuthenticatedDataProvider
    {
        private readonly IAuthenticatedUserProvider _authenticatedUser;
        private readonly IRepository _repository;
        private readonly IModelFactory _factory;
        private readonly IEmailHandler _email;
        private readonly ISiteFeatureProvider _feature;
        private readonly ICharacterAttributeManager _characterAttributeManager;
        private readonly IPublicDataProvider _publicData;
        private readonly IAzureProvider _azure;

        public const string _imageUrlBase = "https://sendingofeight.blob.core.windows.net/images/";

        public bool ShadowMode { get; set; }
        public int CurrentPlayerId => CurrentPlayer.Id;
        public int CurrentCampaignId => _repository.CurrentCampaignId;
        public bool IsMasterDomain => _repository.IsMasterDomain;
        public string ImageUrlBase => _imageUrlBase;
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

        public AuthenticatedDataProvider(IAuthenticatedUserProvider authenticatedUser, IRepository repository, IModelFactory factory, IEmailHandler email, ISiteFeatureProvider feature, ICharacterAttributeManager characterAttributeManager, IPublicDataProvider publicData, IAzureProvider azure)
        {
            _authenticatedUser = authenticatedUser;
            _repository = repository;
            _factory = factory;
            _email = email;
            _feature = feature;
            _characterAttributeManager = characterAttributeManager;
            _publicData = publicData;
            _azure = azure;

            if (_authenticatedUser.UserIsAuthenticated && !_authenticatedUser.IsAdmin)
            {
                ShadowMode = SiteHasFeature(Feature.EnforceShadowMode) || 
                             (SiteHasFeature(Feature.ShadowMode) && PlayerSettingEnabled(SettingNames.ShadowMode));
            }

        }

        private Player _currentPlayer;
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

        private Player _gm;
        private CampaignDetail _campaign;

        public CampaignDetail Campaign
        {
            get
            {
                if (_campaign == null)
                {
                    _campaign = _repository.CampaignDetails().FirstOrDefault();
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

        public ICollection<PageLinkModel> MyPeople()
        {
            return _repository.People().Where(p => p.PlayerId == CurrentPlayer.Id && p.IsDead == false)
                .Select(p => new PageLinkModel{ Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, Type = PageLinkType.Person, FileIdentifier = p.FileIdentifier}).ToList();
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

        public int AddSession(string title, string name, string description, DateTime date, bool sessionCreateWithPreviousCharacterList, List<PageToggleModel> modelLinkPages, GameDate gameDate, int? arcId)
        {
            Session session = new Session
            {
                ShortName = name,
                FullName = title,
                Description = description,
                DateTime = date,
                GameDate = new GameDate() { Year = gameDate.Year, Month = gameDate.Month, Day = gameDate.Day, Comment = gameDate.Comment },
                ArcId = arcId,
            };

            Session previousSession = null;

            if (sessionCreateWithPreviousCharacterList)
            {
                previousSession = GetMostRecentSession();
            }

            int id = Save(session);

            if (previousSession != null)
            {
                foreach (Person person in previousSession.People)
                {
                    AddLink(person.Id, id);
                }
            }

            foreach (PageToggleModel linkPage in modelLinkPages.Where(s => s.Selected))
            {
                AddLink(linkPage.PageId, id);
            }

            return id;
        }

        private Session GetMostRecentSession()
        {
            Session previousSession;
            previousSession =
                _repository.Pages()
                    .OfType<Session>()
                    .OrderByDescending(s => s.DateTime)
                    .FirstOrDefault(s => s.IsTextSession == false) ?? _repository.Pages()
                    .OfType<Session>()
                    .OrderByDescending(s => s.DateTime)
                    .FirstOrDefault();
            return previousSession;
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

        public int AddPerson(string shortName, string longName, string description, bool personCreateAsNpc, Gender personGender)
        {

            Person person = new Person
            {
                ShortName = shortName,
                FullName = longName,
                Description = description,
                Gender = personGender
            };
            if (!personCreateAsNpc)
            {
                person.PlayerId = CurrentPlayer.Id;
            }
            int pageId = Save(person);
            return pageId;
        }

        public int AddArc(string shortName, string longName, string description, GameDate startDate)
        {
            Arc arc = new Arc()
            {
                ShortName = shortName,
                FullName = longName,
                Description = description,
                StartGameDate = new GameDate() { Year = startDate.Year, Month = startDate.Month, Day = startDate.Day, Comment = startDate.Comment },
                CurrentGameDate = new GameDate() { Year = startDate.Year, Month = startDate.Month, Day = startDate.Day, Comment = startDate.Comment }
            };
            int arcId = Save(arc);
            return arcId;
        }


        public void ChangePicture(int id, byte[] data, string mimeType)
        {
            Page page = GetPage(id);
            if (page != null)
            {
                if (data != null)
                {
                    page.FileIdentifier = _azure.CreateImageBlob(data, mimeType);
                    page.ImageMime = mimeType;
                }
                else
                {
                    page.FileIdentifier = "default.png";
                    page.ImageMime = "image/png";
                }

                Save(page);
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
            }

            existingPage.WordCount = existingPage.CalculatedWordCount;
            Save(existingPage);
            AutoAddLinks(existingPage);
            return existingPage;
        }

        private void AutoAddLinks(Page existingPage)
        {
            string pageText = existingPage.RawText.ToLower();

            List<Page> pages =
                _repository.Pages().Where(c => c.CampaignId == existingPage.CampaignId)
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
                if (pageText.Contains(string.Format("{0}]]", page.ShortName.ToLower())))
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

                if (page is Person)
                {
                    page.FileIdentifier = "default_character.jpg";
                    page.ImageMime = "image/jpg";
                }
                else
                {
                    page.FileIdentifier = "default.png";
                    page.ImageMime = "image/png";
                }

                isNew = true;
            }

            while (AnotherPageExistsWithThisName(page.ShortName, page.Id))
            {
                page.ShortName = page.ShortName + " - New";
            }
            page.Modified = DateTime.Now;
            page.ModifedById = CurrentPlayer.Id;
            page.PlainText = page.RawText;
            page.WordCount = page.CalculatedWordCount;

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

            return query.OrderByDescending(p => p.SignificantUpdate).Select(p => new PageLinkWithUpdateDateModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, LastUpdate = p.SignificantUpdate, FileIdentifier = p.FileIdentifier }).Take(20).ToList();
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


        private IQueryable<AwardNomination> ApplyShadow(IQueryable<AwardNomination> query)
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

        public ICollection<Arc> Arcs()
        {
            return _repository.Pages().OfType<Arc>().ToList();
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
                foreach (Page relatedPage in page.Pages)
                {
                    RemoveLink(page.Id, relatedPage.Id);
                }
                foreach (BookPage bookPage in page.BookPages)
                {
                    RemoveBookPage(bookPage.Id);
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

                    List<ScoreBreakdown> scoreBreakdowns =
                        _repository.ScoreBreakDowns().Where(p => p.PersonId == person.Id).ToList();
                    foreach (ScoreBreakdown scoreBreakdown in scoreBreakdowns)
                    {
                        _repository.Delete(scoreBreakdown);
                    }
                }

                foreach (PageImage image in page.PageImages)
                {
                    _repository.Delete(image);
                }

                _repository.Delete(page);
            }
        }

        private void RemoveBookPage(int bookPageId)
        {
            BookPage bookPage = _repository.BookPages().FirstOrDefault(p => p.Id == bookPageId);
            _repository.Delete(bookPage);
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
            var query =  _repository.Pages().Where(p => p.Pinned).Where(p=> p.CampaignId == _repository.CurrentCampaignId);
            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }
            return query.Select(p => new PageLinkModel{ Id = p.Id, ShortName = p.ShortName, FullName = p.FullName }).ToList();
        }

        public ICollection<PageLinkModel> NewPages()
        {
            var query = _repository.Pages().Where(p => p.PageViews.All(v => v.PlayerId != CurrentPlayer.Id));
            if (ShadowMode)
            {
                query = ApplyShadow(query);
            }
            return query.OrderByDescending(p => p.SignificantUpdate).Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, FileIdentifier = p.FileIdentifier }).ToList();
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
            return query.OrderByDescending(p => p.SignificantUpdate).Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, FileIdentifier = p.FileIdentifier }).ToList();
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

        public int AddTrophy(string name, string description, int pointsValue, string fileIdentifier, string mimeType,
            bool currentCampaignOnly, TrophyType trophyTrophyType)
        {
            Trophy trophy = new Trophy();
            trophy.Name = name;
            trophy.Description = description;
            trophy.PointsValue = pointsValue;
            trophy.FileIdentifier = fileIdentifier;
            trophy.MimeType = mimeType;

            if (CurrentUserIsAdmin)
            {
                trophy.TrophyType = trophyTrophyType;
            }

            if (!_authenticatedUser.IsAdmin || currentCampaignOnly)
            {
                trophy.CampaignId = Campaign.CampaignId;
            }
            else
            {
                trophy.CampaignId = null;
            }

            return _repository.Save(trophy);
        }

        public void UpdateTrophy(int id, string name, string description, int pointsValue, bool currentCampaignOnly, TrophyType trophyTrophyType)
        {
            UpdateTrophy(id, name, description, pointsValue, currentCampaignOnly, trophyTrophyType, null, null);
        }

        public void UpdateTrophy(int id, string name, string description, int pointsValue, bool currentCampaignOnly,
            TrophyType trophyTrophyType, string fileIdentifier, string mimeType)
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
                    trophy.CampaignId = Campaign.CampaignId;
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

                if (!string.IsNullOrWhiteSpace(fileIdentifier))
                {
                    trophy.FileIdentifier = fileIdentifier;
                    trophy.MimeType = mimeType;
                }

                if (CurrentUserIsAdmin)
                {
                    trophy.TrophyType = trophyTrophyType;
                }


                _repository.Save(trophy);
            }
        }

        public ICollection<Trophy> Trophies()
        {
            return _repository.Trophies(SiteHasFeature(Feature.HideGlobalAwards))
                .OrderBy(t => t.TypeId == (int)TrophyType.DefaultAward)
                .ThenByDescending(t => t.TypeId == (int)TrophyType.Insignia)
                .ThenByDescending(t => t.TypeId)
                .ThenBy(t => t.TypeId).ThenByDescending(t => t.PointsValue)
                .ThenBy(t => t.Name).ToList();
        }

        public List<SelectItem> TrophiesForSelect()
        {
            return _repository.Trophies(SiteHasFeature(Feature.HideGlobalAwards))
                .Where(t => t.TypeId == (int)TrophyType.DefaultAward)
                .OrderBy(t => t.Name).Select(t => new SelectItem{ Id = t.Id, Name = t.Name }).ToList();
        }

        public int AwardTrophy(int personId, int trophyId, string reason, int? nominatedById = null)
        {
            Award award = new Award
            {
                PersonId = personId,
                TrophyId = trophyId,
                Reason = reason,
                AwardedOn = DateTime.Now,
                NominatedById = nominatedById ?? CurrentPlayerId
            };

            int awardId = _repository.Save(award);
            return awardId;
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

        public decimal CurrentMaxPlayerXp(int xpGroup = 0)
        {
            var query = _repository.People().Where(p => p.PlayerId.HasValue);

            if (SiteHasFeature(Feature.XpGroups))
            {
                if (xpGroup > 0)
                {
                    query = query.Where(p => p.XpGroup == xpGroup);
                }
                else
                {
                    query = query.Where(p => p.XpGroup == xpGroup || p.XpGroup == null);
                }               
            }
            decimal xpCap = query.Max(p => p.XPAwarded);
            return xpCap;
        }

        public void AddXp(int personId, decimal xpValue)
        {
            Person person = GetPerson(personId);
            person.XPAwarded = person.XPAwarded + xpValue;

            CharacterAttributeModel model = _characterAttributeManager.GetCharacterAttributes(personId);
            person.XpSpendAvailable = model.CanBuyAny;

            if (SiteHasFeature(Feature.XpCatchup))
            {
                int xpGroup = 0;
                if (SiteHasFeature(Feature.XpGroups))
                {
                    xpGroup = person.XpGroup.GetValueOrDefault(0);
                }

                decimal playerXpLevel = CurrentMaxPlayerXp(xpGroup);
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

        public bool SettingIsEnabled(SettingNames settingName)
        {
            Setting setting = _repository.Settings().FirstOrDefault(s => s.Name == settingName.ToString());
            return SettingIsEnabled(setting);
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
            List<AwardNomination> nominations = RecentNominations();

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

            foreach (AwardNomination nomination in nominations)
            {
                if (nomination.NominatedDate.HasValue && !dateObject.ContainsKey(nomination.NominatedDate.Value))
                {
                    dateObject.Add(nomination.NominatedDate.Value, nomination);
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

        private List<AwardNomination> RecentNominations()
        {
            var awardNominations = _repository.AwardNominations().Where(n => !n.IsPrivate).Include(a => a.Person).Include(a => a.Trophy);

            if (ShadowMode)
            {
                awardNominations = ApplyShadow(awardNominations);
            }

            List<AwardNomination> nominations = awardNominations.OrderByDescending(a => a.NominatedDate).Take(5).ToList();
            return nominations;
        }


        private List<Award> RecentAwards()
        {
            var awardQuery = _repository.Awards().Where(a => !a.AwardNominations.Any(n => n.IsPrivate)).Include(a => a.Person).Include(a => a.Trophy);

            if (ShadowMode)
            {
                awardQuery = ApplyShadow(awardQuery);
            }

            List<Award> awards = awardQuery.OrderByDescending(a => a.AwardedOn).Take(5).ToList();
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
                    int xpGrouping = people.Where(p => !p.IsNpc).Select(p => p.XpGroup).FirstOrDefault() ?? 0;
                    //always award all players regardless - just to be fair
                    List<Person> playerCharacters = _repository.People().Where(p => p.PlayerId.HasValue && !p.IsDead).ToList();

                    foreach (Person person in playerCharacters)
                    {
                        if (people.All(p => p.Id != person.Id))
                        {
                            if (SiteHasFeature(Feature.XpGroups))
                            {
                                if (person.XpGroup.GetValueOrDefault(0) == xpGrouping)
                                {
                                    people.Add(person);
                                }
                            }
                            else
                            {
                                people.Add(person);
                            }
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
                    xp = 1;
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
                FileIdentifier = _azure.CreateImageBlob(image),
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
            List<PageLinkModel> links = new List<PageLinkModel>();

            links.AddRange(GetRelated<Session>(id));

            links.AddRange(
            _repository.Pages()
                .OfType<SessionLog>()
                .Where(s => s.PersonId != id)
                .Where(p => p.Pages.Any(r => r.Id == id)).Select(p => new PageLinkModel
                {
                    Created = p.Created,
                    FullName = p.FullName,
                    Id = p.Id,
                    ShortName = p.ShortName,
                    FileIdentifier = p.FileIdentifier,
                    Type = PageLinkType.SessionLog,                            
                }).ToList());

            links.AddRange(GetRelated<Person>(id));
            links.AddRange(GetRelated<Place>(id));
            links.AddRange(GetRelated<Page>(id, links.Select(l => l.Id).ToList()));

            return links;

        }

        private IEnumerable<PageLinkModel> GetRelated<T>(int id, List<int> exclude = null) where T:Page
        {
            var query = _repository.Pages().OfType<T>();
            if (ShadowMode)
            {
                query = query.Where(p => MyPageIds.Contains(p.Id));
            }

            query = query.Where(p => p.Pages.Any(r => r.Id == id));

            if (exclude != null)
            {
                query = query.Where(p => !exclude.Contains(p.Id));
            }

            IEnumerable<PageLinkModel>  links = query.Select(p => new PageLinkModel
            {
                Created = p.Created,
                FullName = p.FullName,
                Id = p.Id,
                ShortName = p.ShortName,
                FileIdentifier = p.FileIdentifier,

            }).ToList();

            foreach (PageLinkModel linkModel in links)
            {
                string typeName = typeof(T).Name;
                if (Enum.TryParse(typeName, true, out PageLinkType linkType))
                {
                    linkModel.Type = linkType;
                }
                linkModel.BaseType = typeof(T);
            }

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
            return _repository.People().Where(p => !p.PlayerId.HasValue && p.XpSpendAvailable).Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, FileIdentifier = p.FileIdentifier }).ToList();
        }

        public void AwardShiftForSession(int id)
        {
            Session session = _repository.Pages().OfType<Session>().Include(s => s.Pages).FirstOrDefault(s => s.Id == id);
            AwardShiftForSession(session);
        }

        private void AwardShiftForSession(Session session)
        {
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
                .Where(p => !p.PlayerId.HasValue)
                .Where(p => p.PersonAttributes.Any())
                .Where(p => !p.IsDead)
                .OrderByDescending(p => p.CurrentScore).ToList();
        }

        public List<Person> GetCharacterSheetPeopleWithTrophy(int trophyId)
        {
            return _repository.People()
                .Include(p => p.PersonAttributes)
                .Include(p => p.Awards)
                .Where(p => p.Awards.Any(a => a.TrophyId == trophyId))
                .Where(p => !p.IsDead)
                .OrderByDescending(p => p.CurrentScore).ToList();
        }

        public List<PageLinkModel> GetFavourites()
        {
            var query = _repository.Pages().OfType<Person>()
                .Where(p => p.PlayerId == null)
                .Where(p => p.Awards.Any(a => a.Trophy.TypeId == (int)TrophyType.FirstFavouriteNpc ||
                                              a.Trophy.TypeId == (int)TrophyType.SecondFavouriteNpc ||
                                              a.Trophy.TypeId == (int)TrophyType.ThirdFavouriteNpc ||
                                              a.Trophy.TypeId == (int)TrophyType.NemesisAward));

            if (ShadowMode)
            {
                query = ApplyPeopleShadow(query);
            }

            return query.Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, FileIdentifier = p.FileIdentifier }).ToList();
        }

        public void SetGender(int personId, Gender gender)
        {
            Person person = GetPerson(personId);
            if (person != null)
            {
                person.Gender = gender;
                Save(person);
            }
        }

        public List<ChartDataItem> GetWordcountReportData()
        {
            List<ChartDataItem> counts = new List<ChartDataItem>();

            if (_repository.Pages().OfType<Person>().Any())
            {
                counts.Add(new ChartDataItem
                {
                    Name = "Characters",
                    Value = _repository.Pages().OfType<Person>().Sum(p => p.WordCount)
                });
            }
            if (_repository.Pages().OfType<Session>().Any())
            {
                counts.Add(new ChartDataItem
                {
                    Name = "Sessions",
                    Value = _repository.Pages().OfType<Session>().Sum(p => p.WordCount)
                });
            }
            if (_repository.Pages().OfType<SessionLog>().Any())
            {
                counts.Add(new ChartDataItem
                {
                    Name = "Session Logs",
                    Value = _repository.Pages().OfType<SessionLog>().Sum(p => p.WordCount)
                });
            }
            if (_repository.Pages().OfType<Place>().Any())
            {
                counts.Add(new ChartDataItem
                {
                    Name = "Locations",
                    Value = _repository.Pages().OfType<Place>().Sum(p => p.WordCount)
                });
            }
            int othersTotal = counts.Sum(c => c.Value);
            if (_repository.Pages().OfType<Page>().Any())
            {
                counts.Add(new ChartDataItem
                {
                    Name = "Other Pages",
                    Value = _repository.Pages().OfType<Page>().Sum(p => p.WordCount) - othersTotal
                });
            }
            return counts;

        }

        public List<ChartDataItem> GetPlayerWordcountReportData()
        {
            List<ChartDataItem> counts = new List<ChartDataItem>();

            List<Player> players = _repository.Players().ToList();

            var theData = _repository.Pages().GroupBy(p => p.CreatedById).Select(g => new {playerId = g.Key, Words = g.Sum(x => x.WordCount)});

            foreach (Player player in players)
            {
                int wordCount = theData.Where(d => d.playerId == player.Id).Select(d => d.Words).FirstOrDefault();
                if (wordCount > 0)
                {
                    string count = Convert.ToDouble(string.Format("{0:G2}", wordCount)).ToString("R0");
                    int roundedCount;
                    int.TryParse(count, out roundedCount);

                    if (roundedCount > 0)
                    {

                        counts.Add(new ChartDataItem
                        {
                            Name = player.DisplayName,
                            Value = roundedCount,
                            Color = GetChartColorFor(player)
                        });
                    }
                }
            }

            return counts;
        }

        public List<ChartDataItem> GetCharacterWordcountReportData()
        {
            List<ChartDataItem> counts = new List<ChartDataItem>();

            List<Person> people = _repository.Pages().OfType<Person>().Where(p => p.SessionLogs.Any()).ToList();

            var theData = _repository.Pages().OfType<SessionLog>().GroupBy(p => p.PersonId).Select(g => new { personId = g.Key, Words = g.Sum(x => x.WordCount) });
            

            foreach (Person person in people)
            {
                int wordCount = theData.Where(d => d.personId == person.Id).Select(d => d.Words).FirstOrDefault();
                if (wordCount > 0)
                {
                    string count = Convert.ToDouble(string.Format("{0:G2}", wordCount)).ToString("R0");
                    int roundedCount;
                    int.TryParse(count, out roundedCount);

                    if (roundedCount > 0)
                    {

                        counts.Add(new ChartDataItem
                        {
                            Name = person.FullName,
                            Value = roundedCount,
                        });
                    }
                }
            }

            return counts;
        }

        private Color GetChartColorFor(Player player)
        {
            return GetDefaultColors(player.Id).Last();
        }

        public List<ChartDataItem> GetCharacterGenderReportData()
        {
            List<ChartDataItem> counts = new List<ChartDataItem>();
            var playerData = _repository.People().Where(p => p.GenderEnum != null && p.PlayerId != null).GroupBy(p => p.GenderEnum).Select(g => new { genderId = g.Key, Count = g.Count() });
            var npcData = _repository.People().OfType<Person>().Where(p => p.GenderEnum != null && p.PlayerId == null).GroupBy(p => p.GenderEnum).Select(g => new { genderId = g.Key, Count = g.Count() });

            foreach (Gender gender in Gender.Female.ToList<Gender>())
            {
                var datum = playerData.FirstOrDefault(d => d.genderId == (int) gender && d.Count > 0);
                if (datum != null)
                {
                    counts.Add(new ChartDataItem
                    {
                        Name = ((Gender)datum.genderId).ToString() + " (Player Characters)",
                        Value = datum.Count,
                        Color = GetChartColorFor(gender, true)
                    });
                }
                datum = npcData.FirstOrDefault(d => d.genderId == (int)gender && d.Count > 0);
                if (datum != null)
                {
                    counts.Add(new ChartDataItem
                    {
                        Name = ((Gender)datum.genderId).ToString() + " (NPCs)",
                        Value = datum.Count,
                        Color = GetChartColorFor(gender, false)
                    });
                }
            }

            return counts;
        }

        private Color GetChartColorFor(Gender gender, bool isPlayer)
        {
            Color color = Color.Black;
            
            switch (gender)
            {
                case Gender.Unknown:
                    break;
                case Gender.Female:
                    color = Color.Red;
                    break;
                case Gender.Male:
                    color = Color.Blue;
                    break;
                case Gender.Hermaphrodite:
                    color =  Color.Purple ;
                    break;
                case Gender.Neuter:
                    color = Color.Green;
                    break;
                case Gender.Other:
                    color = Color.Orange;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
            return isPlayer ? color : color.Lerp(Color.White,0.30f);
        }

        public List<ChartDataItem> GetGenderScoresReportData()
        {
            List<ChartDataItem> counts = new List<ChartDataItem>();
            var playerData = _repository.Pages().OfType<Person>().Where(p => p.GenderEnum != null && p.PlayerId != null).GroupBy(p => p.GenderEnum).Select(g => new { genderId = g.Key, Count = g.Sum(c => c.CurrentScore) });
            var npcData = _repository.Pages().OfType<Person>().Where(p => p.GenderEnum != null && p.PlayerId == null).GroupBy(p => p.GenderEnum).Select(g => new { genderId = g.Key, Count = g.Sum(c => c.CurrentScore) });

            foreach (Gender gender in Gender.Female.ToList<Gender>())
            {
                var datum = playerData.FirstOrDefault(d => d.genderId == (int)gender && d.Count > 0);
                if (datum != null)
                {
                    counts.Add(new ChartDataItem
                    {
                        Name = ((Gender)datum.genderId).ToString() + " (Player Characters)",
                        Value = (int)datum.Count,
                        Color = GetChartColorFor(gender, true)
                    });
                }
                datum = npcData.FirstOrDefault(d => d.genderId == (int)gender && d.Count > 0);
                if (datum != null)
                {
                    counts.Add(new ChartDataItem
                    {
                        Name = ((Gender)datum.genderId).ToString() + " (NPCs)",
                        Value = (int)datum.Count,
                        Color = GetChartColorFor(gender, false)
                    });
                }
            }

            return counts;
        }

        public List<ChartDataItem> GetPagesByPlayerReportData()
        {
            List<ChartDataItem> counts = new List<ChartDataItem>();

            List<Player> players = _repository.Players().ToList();

            var theData = _repository.Pages().GroupBy(p => p.CreatedById).Select(g => new { playerId = g.Key, Words = g.Count() });

            foreach (Player player in players)
            {
                int pageCount = theData.Where(d => d.playerId == player.Id).Select(d => d.Words).FirstOrDefault();
                if (pageCount > 0)
                {
                        counts.Add(new ChartDataItem
                        {
                            Name = player.DisplayName,
                            Value = pageCount,
                            Color = GetChartColorFor(player)
                        });
                }
            }

            return counts;
        }

        public List<ChartDataItem> GetTopAwardsReportData()
        {
            List<ChartDataItem> counts = new List<ChartDataItem>();
            int campaginId = _repository.CurrentCampaignId;
            var trophyData = _repository.Trophies()
                .Where(t => t.Awards.Any())
               // .Where(t => t.TypeId == (int) TrophyType.DefaultAward)
                .OrderByDescending(t => t.Awards.Count(a => a.CampaignId == campaginId))
                .Take(10)
                .OrderByDescending(t => t.PointsValue)
                .Select(a => new { trophy = a.Name, count = a.Awards.Count(b => b.CampaignId == campaginId) });

            foreach (var datum in trophyData)
            {
                counts.Add(new ChartDataItem
                {
                    Name = datum.trophy.Replace("'","`"),
                    Value = datum.count
                });
            }

            return counts;
        }

        public List<ChartDataItem> GetPersonTextPostReportData()
        {
            List<ChartDataItem> counts = new List<ChartDataItem>();

            var theData =
                _repository.Posts()
                .Where(p => p.CharacterId.HasValue)
                .Where(p => p.PostType == (int)PostType.InCharacter)
                    .GroupBy(p => p.CharacterId)
                    .OrderByDescending(p => p.Count())
                    .Select(g => new {CharacterId = g.Key.Value, Posts = g.Count()})
                    .Take(10);
            List<int> personIds = theData.Select(d => d.CharacterId).ToList();

            List<Person> people = _repository.Pages().OfType<Person>().Where(p => personIds.Contains(p.Id)).ToList();

            if (people.Count > 0)
            {
                foreach (Person person in people)
                {
                    int postCount = theData.Where(d => d.CharacterId == person.Id).Select(d => d.Posts).FirstOrDefault();
                    if (postCount > 0)
                    {
                        counts.Add(new ChartDataItem
                        {
                            Name = person.FullName,
                            Value = postCount,
                        });
                    }
                }
            }
            return counts;
        }

        public int TotalAwardCount()
        {
            return _repository.Awards().Count();
        }

        public List<ChartDataItem> GetPlayerTextPostReportData()
        {
            List<ChartDataItem> counts = new List<ChartDataItem>();

            List<Player> players = _repository.Players().ToList();

            if (players.Count > 0)
            {
                int topPlayerId = players.OrderByDescending(p => p.Id).First().Id;
                Color[] colors = GetDefaultColors(topPlayerId);


                var theGmData =
                    _repository.Posts()
                        .Where(p => p.PostType == (int)PostType.GmInCharacter)
                        .GroupBy(p => p.PlayerId)
                        .Select(g => new { playerId = g.Key, Count = g.Count() });
                var theIcData =
                    _repository.Posts()
                        .Where(p => p.PostType == (int) PostType.InCharacter)
                        .GroupBy(p => p.PlayerId)
                        .Select(g => new {playerId = g.Key, Count = g.Count()});
                var theOocData =
                    _repository.Posts()
                        .Where(p => p.PostType == (int) PostType.OutOfCharacter)
                        .GroupBy(p => p.PlayerId)
                        .Select(g => new {playerId = g.Key, Count = g.Count()});
                var diceData =
                    _repository.Posts()
                        .Where(p => p.PostType == (int)PostType.DiceRoll)
                        .GroupBy(p => p.PlayerId)
                        .Select(g => new { playerId = g.Key, Count = g.Count() });

                foreach (Player player in players)
                {

                    int postCount = theGmData.Where(d => d.playerId == player.Id).Select(d => d.Count).FirstOrDefault();
                    if (postCount > 0)
                    {
                        counts.Add(new ChartDataItem
                        {
                            Name = $"{player.DisplayName} (GM)",
                            Value = postCount,
                            Color = colors[player.Id - 1].Lerp(Color.Black, 0.3f)
                        });
                    }

                    postCount = theIcData.Where(d => d.playerId == player.Id).Select(d => d.Count).FirstOrDefault();
                    if (postCount > 0)
                    {
                        counts.Add(new ChartDataItem
                        {
                            Name = $"{player.DisplayName} (In Character)",
                            Value = postCount,
                            Color = colors[player.Id - 1]
                        });
                    }
                    postCount = theOocData.Where(d => d.playerId == player.Id).Select(d => d.Count).FirstOrDefault();
                    if (postCount > 0)
                    {
                        counts.Add(new ChartDataItem
                        {
                            Name = $"{player.DisplayName} (OOC)",
                            Value = postCount,
                            Color = colors[player.Id -1].Lerp(Color.White, 0.3f)
                        });
                    }
                    postCount = diceData.Where(d => d.playerId == player.Id).Select(d => d.Count).FirstOrDefault();
                    if (postCount > 0)
                    {
                        counts.Add(new ChartDataItem
                        {
                            Name = $"{player.DisplayName} (Dice Rolls)",
                            Value = postCount,
                            Color = colors[player.Id - 1].Lerp(Color.White, 0.6f)
                        });
                    }
                }
            }
            return counts;
        }

        public Color[] GetDefaultColors(int count)
        {
            List<Color> colors = new List<Color>();
            for (int i = 0; i < count; i++)
            {
                switch (i % 10)
                {
                    case 0:
                        colors.Add(Color.Black);
                        break;
                    case 1:
                        colors.Add(Color.BlueViolet);
                        break;
                    case 2:
                        colors.Add(Color.Blue);
                        break;
                    case 3:
                        colors.Add(Color.DarkCyan);
                        break;
                    case 4:
                        colors.Add(Color.Green);
                        break;
                    case 5:
                        colors.Add(Color.YellowGreen);
                        break;
                    case 6:
                        colors.Add(Color.Gold);
                        break;
                    case 7:
                        colors.Add(Color.Orange);
                        break;
                    case 8:
                        colors.Add(Color.OrangeRed);
                        break;
                    case 9:
                        colors.Add(Color.Red);
                        break;
                    default:
                        break;


                }
            }
            return colors.ToArray();
        }

        public void SetAsNemisis(int id)
        {
            SetMyAward(id, TrophyType.NemesisAward);
        }

        public void SetAsTopFavourite(int personId)
        {
            int playerId = CurrentPlayerId;
            Person person = GetPerson(personId);
            if (person != null)
            {
                Award thridAward = _repository.Awards().FirstOrDefault(a => a.Trophy.TypeId == (int) TrophyType.ThirdFavouriteNpc && a.NominatedById == playerId);
                if(thridAward !=null)
                {
                    RemoveAward(thridAward.PersonId, thridAward.Id);
                }

                Award secondAward = _repository.Awards().FirstOrDefault(a => a.Trophy.TypeId == (int)TrophyType.SecondFavouriteNpc && a.NominatedById == playerId);
                if (secondAward != null)
                {
                    if (secondAward.PersonId == personId)
                    {
                        RemoveAward(secondAward.PersonId, secondAward.Id);
                    }
                    else
                    {
                        Trophy trophy = Trophies().FirstOrDefault(t => t.TypeId == (int) TrophyType.ThirdFavouriteNpc);
                        if (trophy != null)
                        {
                            secondAward.TrophyId = trophy.Id;
                            _repository.Save(secondAward);
                        }
                    }
                }

                Award firstAward = _repository.Awards().FirstOrDefault(a => a.Trophy.TypeId == (int)TrophyType.FirstFavouriteNpc && a.NominatedById == playerId);
                if (firstAward != null)
                {
                    Trophy trophy = Trophies().FirstOrDefault(t => t.TypeId == (int)TrophyType.SecondFavouriteNpc);
                    if (trophy != null)
                    {
                        firstAward.TrophyId = trophy.Id;
                        _repository.Save(firstAward);
                    }
                }

                Trophy firstTrophy = Trophies().FirstOrDefault(t => t.TypeId == (int)TrophyType.FirstFavouriteNpc);
                if (firstTrophy != null)
                {
                    AwardTrophy(personId, firstTrophy.Id, ": Awarded by " + CurrentPlayer.DisplayName);
                }

            }
        }

        public void NominateForAward(int personId, int selectedAward, string reason, bool isPrivate)
        {
            AwardNomination award = new AwardNomination
            {
                CampaignId = CurrentCampaignId,
                NominationReason = reason,
                NominatedById = CurrentPlayerId,
                NominatedDate = DateTime.Now,
                TrophyId = selectedAward,
                PersonId = personId,
                IsPrivate = isPrivate
            };
            _repository.Save(award);
        }

        public List<AwardNomination> OutstandingPublicNominationsForPerson(int personId)
        {
            return _repository.AwardNominations().Where(n => !n.IsPrivate || n.NominatedById == CurrentPlayerId).Where(a => !a.AwardedOn.HasValue && !a.RejectedOn.HasValue && a.PersonId == personId).ToList();
        }

        public List<AwardNomination> OutstandingNominations()
        {
            return _repository.AwardNominations().Where(a => !a.AwardedOn.HasValue && !a.RejectedOn.HasValue).ToList();
        }

        public void AcceptNomination(int nominationId, string acceptComment, string awardText)
        {
            AwardNomination nomination = _repository.AwardNominations().Single(a => a.Id == nominationId);
            nomination.AwardedOn = DateTime.Now;
            nomination.AcceptedReason = acceptComment;
            int awardId = AwardTrophy(nomination.PersonId, nomination.TrophyId, awardText, nomination.NominatedById);
            nomination.AwardId = awardId;
            _repository.Save(nomination);
        }

        public void RejectNomination(int nominationId, string rejectedReason)
        {
            AwardNomination nomination = _repository.AwardNominations().Single(a => a.Id == nominationId);
            nomination.RejectedOn = DateTime.Now;
            nomination.RejectedReason = rejectedReason;
            _repository.Save(nomination);
        }

        public void SaveGmNotes(int pageId, string gmNotes)
        {
            Page page = _repository.Pages().FirstOrDefault(p => p.Id == pageId);
            if (page != null)
            {
                page.GmNotes = gmNotes;
                Save(page);
            }
        }

        public void ApplyShift(int id)
        {
            Page page = _repository.Pages().FirstOrDefault(p => p.Id == id);
            if (page != null)
            {
                Session session = page as Session;
                if (session != null)
                {
                    AwardShiftForSession(session.Id);
                }
                Person person = page as Person;
                if (person != null)
                {
                    person.HasAttributeMoveAvailable = true;
                    _repository.Save(person);
                }
            }
        }

        public List<PageLinkModel> GetRecentViewedPages()
        {
            return
                _repository.Pages()
                    .OrderByDescending(p => p.PageViews
                        .Where(u => u.CampaignId == CurrentCampaignId)
                        .Where(v => v.PlayerId == CurrentPlayerId).Select(s => s.Viewed).FirstOrDefault())
                    .Take(15)
                    .Select(p => new PageLinkModel {Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, FileIdentifier = p.FileIdentifier })
                    .ToList();
        }

        public string GetPageName(int id)
        {
            return _repository.Pages().Where(i => i.Id == id).Select(p => p.FullName).First();
        }

        public void SetCustomCss(string customCss)
        {
            CampaignDetail campaign = _repository.CampaignDetails().FirstOrDefault();
            if (campaign != null)
            {
                campaign.CustomCss = customCss;
                _repository.Save(campaign);
            }
        }

        public void CreateCampaign(string domain, string customCss, DateTime? gameDate, int modelGmId)
        {
            CampaignDetail campaign = new CampaignDetail();
            campaign.CustomCss = customCss;
            campaign.CurrentGameDate = gameDate;
            campaign.Url = domain;

            if (modelGmId > 0)
            {
                campaign.GmId = modelGmId;
            }
            else
            {
                campaign.GmId = CurrentPlayerId;
            }
         
            campaign.AverageStat = 3;
            _repository.Save(campaign);
        }

        public Player GetPlayer(string email)
        {
            return _repository.Players().FirstOrDefault(p => p.UserName == email);
        }

        public void SetPersonPlayer(int personId, int? playerId)
        {
            Person person = GetPerson(personId);
            if (person != null)
            {
                person.PlayerId = playerId;
                Save(person);
            }
        }

        public List<Comment> GetCommentsForPage(int pageId)
        {
            return _repository.Comments().Where(c => c.PageId == pageId).ToList();
        }

        public List<CampaignDetail> AllCampaigns()
        {
            return _repository.CampaignDetails().ToList();
        }

        public void SetCampaignName(string name)
        {
            CampaignDetail campaign = _repository.CampaignDetails().FirstOrDefault();
            if (campaign != null)
            {
                campaign.Name = name;
                _repository.Save(campaign);
            }
        }

        public CampaignDetail GetCampaginDetailsForPage(int id)
        {
            int campaignId = _repository.Pages().Where(p => p.Id == id).Select(p => p.CampaignId).FirstOrDefault();
            return _repository.CampaignDetails().FirstOrDefault(c => c.CampaignId == campaignId);
        }

        public List<CampaignDetail> GetPermittedCampaigns()
        {
            List<CampaignDetail> allMyCampagins = _repository.AllMyCampaigns();
            return allMyCampagins;
        }

        public List<PageToggleModel> GetSuggestedPageLinksForNewSession()
        {
            List<PageToggleModel> suggestedPageLinks = new List<PageToggleModel>();

            suggestedPageLinks.AddRange(_repository.People().Where(p => p.PlayerId.HasValue && !p.IsDead).Select(p => new PageToggleModel
            {
                FullName = p.FullName,
                PageId = p.Id,
                Selected = false,
                InitialState = false,
                ShortName = p.ShortName
            }).ToList());

            suggestedPageLinks.AddRange(_repository.People()
                .OrderByDescending(p=> p.CurrentScore)
                .Where(p => !p.PlayerId.HasValue && !p.IsDead)
                .Select(p => new PageToggleModel
            {
                FullName = p.FullName,
                PageId = p.Id,
                Selected = false,
                InitialState = false,
                ShortName = p.ShortName
            }).Take(10).ToList());

            suggestedPageLinks.AddRange(_repository.People()
                .OrderByDescending(p => p.Created)
                .Where(p => !p.IsDead)
                .Select(p => new PageToggleModel
                {
                    FullName = p.FullName,
                    PageId = p.Id,
                    Selected = false,
                    InitialState = false,
                    ShortName = p.ShortName
                }).Take(5).ToList());

            List<Session> lastThreeSessions = _repository.Pages().OfType<Session>().OrderByDescending(s => s.DateTime).Take(3).ToList();

            foreach (Session session in lastThreeSessions)
            {
                suggestedPageLinks.AddRange(session.People
                    .Where(p => !p.IsDead)
                    .Select(p => new PageToggleModel
                    {
                        FullName = p.FullName,
                        PageId = p.Id,
                        Selected = false,
                        InitialState = false,
                        ShortName = p.ShortName
                    }).Take(10).ToList());
            }

            List<int> distinctIds = suggestedPageLinks.Select(s => s.PageId).Distinct().ToList();

            var uniquePageLinks = new List<PageToggleModel>();

            foreach (int id in distinctIds)
            {
                uniquePageLinks.Add(suggestedPageLinks.First(p => p.PageId == id));   
            }

            Session lastSession = GetMostRecentSession();
            if (lastSession != null)
            {
                foreach (Person person in lastSession.People)
                {
                    foreach (PageToggleModel pageLink in uniquePageLinks)
                    {
                        if (pageLink.PageId == person.Id)
                        {
                            pageLink.Selected = true;
                            pageLink.InitialState = true;
                        }
                    }
                }
            }
            return uniquePageLinks.OrderBy(s => s.FullName).ToList();
        }

        public Arc GetArc(int id)
        {
            return _repository.Arcs().FirstOrDefault(a => a.Id == id);
        }

        public void SaveDate(int pageId, GameDate date, bool isStartDate)
        {
            Page page = _repository.Pages().FirstOrDefault(p => p.Id == pageId);
            if (page != null && (page is Session || page is Arc) && date != null)
            {
                if (page is Arc)
                {
                    Arc arc = (Arc)page;
                    if (isStartDate)
                    {
                        if (arc.StartGameDate != null)
                        {
                            arc.StartGameDate.Year = date.Year;
                            arc.StartGameDate.Month = date.Month;
                            arc.StartGameDate.Day = date.Day;
                        }
                        else
                        {
                            arc.StartGameDate = date;
                        }
                    }
                    else
                    {
                        if (arc.CurrentGameDate != null)
                        {
                            arc.CurrentGameDate.Year = date.Year;
                            arc.CurrentGameDate.Month = date.Month;
                            arc.CurrentGameDate.Day = date.Day;
                        }
                        else
                        {
                            arc.CurrentGameDate = date;
                        }
                    }
                }

                if (page is Session)
                {
                    Session session = (Session)page;
                    if (session.GameDate != null)
                    {
                        session.GameDate.Year = date.Year;
                        session.GameDate.Month = date.Month;
                        session.GameDate.Day = date.Day;
                    }
                    else
                    {
                        session.GameDate = date;
                    }
                }

                _repository.Save(page);
            }
        }

        public void AddDayToDate(int pageId, bool isStartDate)
        {
            AddToDate(pageId, DateAddType.Day, isStartDate);
        }

        public void AddWeekToDate(int pageId, bool isStartDate)
        {
            AddToDate(pageId, DateAddType.Week, isStartDate);
        }

        public void AddMonthToDate(int pageId, bool isStartDate)
        {
            AddToDate(pageId, DateAddType.Month, isStartDate);
        }

        private void AddToDate(int pageId, DateAddType dateAddType, bool isStartDate)
        {
            GameDate date = null;
            Page page = _repository.Pages().FirstOrDefault(p => p.Id == pageId);
            if (page != null && (page is Session || page is Arc))
            {
                if (page is Arc)
                {
                    Arc arc = (Arc)page;
                    if (isStartDate && arc.StartGameDate != null)
                    {
                        date = arc.StartGameDate.Clone();
                    }
                    else if (!isStartDate && arc.CurrentGameDate != null)
                    {
                        date = arc.CurrentGameDate.Clone();
                    }
                }

                if (page is Session)
                {
                    Session session = (Session)page;
                    if (session.GameDate != null)
                    {
                        date = session.GameDate.Clone();
                    }
                    else if (session.Arc?.CurrentGameDate != null)
                    {
                        date = session.Arc.CurrentGameDate.Clone();
                    }
                }
            }

            if (date == null)
            {
                CampaignDetail campaignDetail = GetCampaginDetails();
                if (campaignDetail?.CurrentGameDate != null)
                {
                    date = campaignDetail.CurrentGameDate.Value.ToGameDate();
                }
                else
                {
                    date = DateTime.Now.ToGameDate();
                }
            }

            switch (dateAddType)
            {
                case DateAddType.Day:
                    date.AddDay();
                    break;
                case DateAddType.Month:
                    date.AddMonth();
                    break;
                case DateAddType.Week:
                    date.AddWeek();
                    break;
            }

            SaveDate(pageId, date, isStartDate);
        }
 

        public List<Session> AllSessions()
        {
            return _repository.Pages().OfType<Session>().OrderBy(s => s.DateTime).ToList();
        }

        public void SetSessionArc(int? arcId, int sessionId)
        {
            Arc arc = _repository.Arcs().FirstOrDefault(a => a.Id == arcId);
            Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);

            if (session != null)
            {
                session.ArcId = arc?.Id;
                _repository.Save(session);
            }
        }

        public Session GetSession(int id)
        {
            return _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == id);
        }

        public SessionArcSummaryModel GetSessionArcSummary(int id)
        {
            int? arcId = _repository.Pages().OfType<Session>().Where(s => s.Id == id).Select(s => s.ArcId).FirstOrDefault();
            if (arcId.HasValue)
            {
                SessionArcSummaryModel model = _repository.Pages().OfType<Arc>().Where(a => a.Id == arcId).Select(m =>
                    new SessionArcSummaryModel
                    {
                        Name = m.FullName,
                        SessionId = id,
                        ArcId = arcId,
                    }).Single();

                model.Sessions = _repository.Pages().OfType<Session>().Where(s => s.ArcId == arcId)
                    .OrderBy(s => s.GameDate.Year)
                    .ThenBy(s => s.GameDate.Month)
                    .ThenBy(s => s.GameDate.Day)
                    .ThenBy(s => s.DateTime)
                    .ThenBy(s => s.Id).Select(s =>
                    new PageListItemModel {Fullname = s.FullName, Id = s.Id, ShortName = s.ShortName}).ToList();

                return model;
            }
            else
            {
                return new SessionArcSummaryModel{ SessionId = id, Sessions = new List<PageListItemModel>() };
            }
        }

        public List<PageLinkModel> AllNpcLinks()
        {
            return _repository.Pages().OfType<Person>().Where(s => !s.PlayerId.HasValue && !s.IsDead)
                .Select(p => new PageLinkModel
                {
                    Id = p.Id, FullName = p.FullName, ShortName = p.ShortName, Type = PageLinkType.Person,
                    FileIdentifier = p.FileIdentifier
                }).ToList();
        }



        public List<PageLinkModel> SessionLogs(int id)
        {
            return _repository.Pages().OfType<SessionLog>().Where(s => s.PersonId == id)
                .Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, FileIdentifier = p.FileIdentifier }).ToList();
        }

        public List<AwardSummaryModel> GetAwardsForPerson(int id, bool pointsOrder = false)
        {
            var query = _repository.Awards().Where(a => a.PersonId == id);

            if (pointsOrder)
            {
                query = query.OrderBy(t => t.Trophy.TypeId == (int) TrophyType.DefaultAward)
                    .ThenByDescending(t => t.Trophy.TypeId == (int)TrophyType.Insignia)
                    .ThenBy(m => m.Trophy.TypeId).ThenByDescending(m => m.Trophy.PointsValue).ThenBy(a => a.Trophy.Name)
                    .ThenBy(a => a.Id);
            }
            else
            {
                query = query.OrderByDescending(a => a.AwardedOn);
            }

            return query.Select(a => new AwardSummaryModel
            {
                AwardedOn = a.AwardedOn,
                Id = a.Id,
                TrophyId = a.TrophyId,
                Reason = a.Reason,
                TrophyName = a.Trophy.Name,
                FileIdentifier = a.Trophy.FileIdentifier
            }).ToList();
        }

        public List<ScoreBreakdown> GetScoreBreakdown(int personId)
        {
            return _repository.ScoreBreakDowns().Where(s => s.PersonId == personId).ToList();
        }

        public void TogglePublicImage(int id)
        {
            PageImage image = _repository.PageImages().FirstOrDefault(i => i.Id == id);
            if (image != null)
            {
                image.Public = !image.Public;
                _repository.Save(image);
            }
        }

        public List<PageLinkModel> RecentSessionsToLink(int id)
        {
            DateTime twentyFourHoursAgo = DateTime.Now.AddDays(-1);
            return _repository.Pages().OfType<Session>()
                .Where(s => s.Created > twentyFourHoursAgo)
                .Where(p => p.CampaignId == CurrentCampaignId)
                .Where(s => s.Id != id)
                .Where(s => s.Pages.All(a => a.Id != id))
                .Select(s => new PageLinkModel
                {
                    FullName = s.FullName,
                    ShortName = s.ShortName,
                    Id = s.Id,
                    Created = s.Created,
                    Type = PageLinkType.Session,
                    FileIdentifier = s.FileIdentifier
                }).ToList();
        }

        public List<PageToggleModel> GetAllPeopleForSession(int id)
        {
            List<PageToggleModel> people = new List<PageToggleModel>();

            people.AddRange(_repository.People().Where(p => p.Pages.Any(g => g.Id == p.Id)).Select(p => new PageToggleModel
            {
                FullName = p.FullName,
                PageId = p.Id,
                Selected = true,
                InitialState = true,
                ShortName = p.ShortName
            }).ToList());

            people.AddRange(_repository.People().Where(p => p.Pages.All(g => g.Id != p.Id)).Select(p => new PageToggleModel
            {
                FullName = p.FullName,
                PageId = p.Id,
                Selected = false,
                InitialState = false,
                ShortName = p.ShortName
            }).ToList());

            return people.OrderBy(p => p.FullName).ToList();
        }

        public void SaveIcon(int size, byte[] imageData)
        {
            SiteIcon existing = _repository.SiteIcons().FirstOrDefault(s => s.Size == size);
            if (existing != null)
            {
                existing.Data = imageData;
                _repository.Save(existing);
            }
            else
            {
                SiteIcon icon = new SiteIcon { CampaignId = CurrentCampaignId, Size = size, Data = imageData };
                _repository.Save(icon);
            }
        }

        public CharacterLeagueModel GetFullLeague()
        {
            CharacterLeagueModel model = new CharacterLeagueModel();
            var peopleData = _publicData.AllPeople().OrderByDescending(c => c.CurrentScore);
            List<int> excludedCampaigns = _publicData.CampaignsWithFeature(Feature.PlaygroundMode);
            model.Items = GetLeagueItems(peopleData, excludedCampaigns);

            return model;
        }

        public List<PlayerCampaign> GetAllPlayerCampaigns()
        {
            return _repository.PlayerCampaigns().ToList();
        }

        public List<PlayerCampaignLinkModel> GetAllPlayerCampaignLinkModels()
        {
            return _repository.Players().Select(p => new PlayerCampaignLinkModel
            {
                PlayerId = p.Id,
                PlayerName = p.DisplayName,
                PlayerEmail = p.UserName,
                ShowInGlobal = p.PlayerCampaigns.Where(c => c.CampaginId == CurrentCampaignId).Select(c => c.ShowInGlobal).FirstOrDefault(),
                PlayerCampaignId = p.PlayerCampaigns.Where(c => c.CampaginId == CurrentCampaignId).Select(c => c.Id).FirstOrDefault(),
                IncludeInCampaign = p.PlayerCampaigns.Where(c => c.CampaginId == CurrentCampaignId).Select(c => c.Id).FirstOrDefault() > 0
            }).ToList();
        }

        public void UpdatePlayerSiteLinks(List<PlayerCampaignLinkModel> playerLinks)
        {
            List<Player> players = GetAllPlayers();
            foreach (PlayerCampaignLinkModel link in playerLinks)
            {
                Player player = players.FirstOrDefault(p => p.Id == link.PlayerId);
                if (player != null)
                {
                    PlayerCampaign campaign = player.PlayerCampaigns.FirstOrDefault(c => c.CampaginId == CurrentCampaignId);
                    if (link.IncludeInCampaign)
                    {
                        if (campaign == null)
                        {
                            campaign = new PlayerCampaign { PlayerId = link.PlayerId, PlayerModeEnum = 1, CampaginId = CurrentCampaignId };
                        }

                        campaign.ShowInGlobal = link.ShowInGlobal;
                        _repository.Save(campaign);

                    }
                    else
                    {
                        
                        if(campaign != null)
                        {
                            _repository.Delete(campaign);
                        }
                    }
                }

            }
        }

        public List<PageImage> GetPageImages()
        {
            return _repository.PageImages().ToList();
        }

        public void SaveImage(PageImage pageImage)
        {
            _repository.Save(pageImage);
        }

        public List<Trophy> AdminGetTrophy()
        {
            return _repository.AdminTrophies().ToList();
        }

        public void RemoveAward(int personId, int awardId)
        {
            Person person = GetPerson(personId);
            if (person != null)
            {
                Award award = person.Awards.FirstOrDefault(a => a.Id == awardId);
                if (award != null)
                {
                    if (award.AwardNominations.Any())
                    {
                        award.AwardNominations.Clear();
                    }
                    _repository.Delete(award);
                }
            }
        }

        public PageLinkModel PersonWithMyAward(TrophyType awardType)
        {
           return _repository.People()
               .Where(p => p.Awards.Any(a => a.NominatedById == CurrentPlayer.Id&& a.Trophy.TypeId == (int)awardType))
               .Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, FileIdentifier = p.FileIdentifier }).FirstOrDefault();

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


            return query.Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, FileIdentifier = p.FileIdentifier }).ToList();
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

            AwardShiftForSession(session);
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

        public CharacterLeagueModel GetLeague()
        {
            CharacterLeagueModel model = new CharacterLeagueModel();
            
            //List<Person> people = new List<Person>();
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

                query = query.OrderByDescending(s => s.CurrentScore)
                    .ThenByDescending(s => s.Modified);

                model.Items = GetLeagueItems(query, new List<int>());
            }
            return model;
        }

        private List<CharacterLeagueItemModel> GetLeagueItems(IQueryable<Person> query, List<int> excludedCampaigns)
        {

            Dictionary<int, string> campaignUrls = _repository.AllCampaigns().Where(c => !excludedCampaigns.Contains(c.CampaignId)).ToDictionary(t => t.CampaignId, t => t.Url);
            Dictionary<int, string> campaignNames = _repository.AllCampaigns().Where(c => !excludedCampaigns.Contains(c.CampaignId)).ToDictionary(t => t.CampaignId, t => t.DisplayName);

            int[] campaignIds = campaignUrls.Keys.ToArray();
            List <CharacterLeagueItemModel> data = query
                .Where(p => campaignIds.Contains(p.CampaignId)).Select(p => new CharacterLeagueItemModel
                {
                    FullName = p.FullName,
                    Id = p.Id,
                    PlainText = p.PlainText,
                    XpAwarded = p.XPAwarded,
                    CurrentScore = p.CurrentScore,
                    ImageFile = p.PageImages.Where(s => s.IsPrimary).Select(s => s.FileIdentifier).FirstOrDefault(),
                    Awards = p.Awards
                        .OrderByDescending(t => t.Trophy.PointsValue)
                        .Select(a => new AwardSummaryModel
                        {
                            AwardedOn = a.AwardedOn,
                            Id = a.Id,
                            Reason = a.Reason,
                            TrophyId = a.TrophyId,
                            TrophyName = a.Trophy.Name,
                            FileIdentifier = a.Trophy.FileIdentifier
                        }).ToList(),
                    CampaignId = p.CampaignId,
                    Player = p.Player.DisplayName
                    
            }).ToList()
                .ToList();

            int i = 1;
            foreach (CharacterLeagueItemModel item in data)
            {
                item.Rank = i++;
                item.Campaign = campaignNames[item.CampaignId];
                item.Url = $"https://{campaignUrls[item.CampaignId]}/page/index/{item.Id}";
                if (item.ImageFile != null)
                {
                    item.ImageUrl = $"{ImageUrlBase}{item.ImageFile}";
                }
                else
                {
                    item.ImageUrl = $"{ImageUrlBase}default.png";
                }
            }
            return data;
        }

        public List<PageLinkModel> OtherPCs()
        {
            var query = _repository.People().Where(p => p.PlayerId.HasValue && p.PlayerId != CurrentPlayer.Id && !p.IsDead);

            if (ShadowMode)
            {
                query = ApplyPeopleShadow(query);
            }

            return query.Select(p => new PageLinkModel { Id = p.Id, ShortName = p.ShortName, FullName = p.FullName, FileIdentifier = p.FileIdentifier }).ToList();
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

        public List<OpenTextSessionSummaryModel> MyOpenTextSessions()
        {
            List<CampaignDetail> campaigns = _repository.CampaignDetails().ToList();

            List<OpenTextSessionSummaryModel> sessions = _repository.Pages().OfType<Session>()
                .Where(s => s.IsTextSession)
                .Where(s => !s.IsClosed)
                .Where(s => s.Related.OfType<Person>().Where(p => p.PlayerId != null).Any(p => p.PlayerId == CurrentPlayer.Id) || CurrentPlayerIsGm || s.GmId == CurrentPlayer.Id)
                .Select(s => new OpenTextSessionSummaryModel
                {
                    LastPostTime = s.Posts.OrderByDescending(p => p.DatePosted).Select(d => d.DatePosted).FirstOrDefault(),
                    IsPrivate =  s.IsPrivate,
                    SessionId = s.Id,
                    SessionName = s.FullName,
                    CampaginId = s.CampaignId,
                    IsUpdated = s.PageViews.Any(v => v.PlayerId == CurrentPlayerId && v.Viewed < s.Posts.OrderByDescending(p => p.DatePosted).Select(d => d.DatePosted).FirstOrDefault()),

                })
                .ToList();
        
            foreach (OpenTextSessionSummaryModel session in sessions)
            {
                session.MyTurn = PlayerToPostInSession(session.SessionId)?.Id == CurrentPlayerId;
                session.Domain = campaigns.Where(c => c.CampaignId == session.CampaginId).Select(c => c.Url)
                    .FirstOrDefault();
            }

            return sessions.OrderByDescending(s => s.MyTurn).ThenBy(s => s.LastPostTime).ToList();
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
