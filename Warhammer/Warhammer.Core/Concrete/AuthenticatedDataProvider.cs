﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
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

        private int UpliftId
        {
            get
            {
                if (ConfigurationManager.AppSettings["UpliftId"] != null)
                {
                    int id;
                    if (int.TryParse(ConfigurationManager.AppSettings["UpliftId"].ToString(), out id))
                    {
                        return id;
                    }

                }
                return 0;
            }
        }

        private int NailId
        {
            get
            {
                if (ConfigurationManager.AppSettings["NailId"] != null)
                {
                    int id;
                    if (int.TryParse(ConfigurationManager.AppSettings["NailId"].ToString(), out id))
                    {
                        return id;
                    }

                }
                return 0;
            }
        }

        public AuthenticatedDataProvider(IAuthenticatedUserProvider authenticatedUser, IRepository repository, IModelFactory factory, IEmailHandler email)
        {
            _authenticatedUser = authenticatedUser;
            _repository = repository;
            _factory = factory;
            _email = email;
        }

        public Player CurrentPlayer
        {
            get
            {

                if (!_authenticatedUser.UserIsAuthenticated)
                {
                    throw new Exception("User is not Authenticated");
                }
                return _repository.Players().FirstOrDefault(p => p.UserName == _authenticatedUser.UserName);
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
            return _repository.People().Where(p => p.Player.UserName == _authenticatedUser.UserName).ToList();
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

        public int AddPerson(string shortName, string longName, string description)
        {

            Person person = new Person
            {
                ShortName = shortName,
                FullName = longName,
                Description = description,
            };
            if (!CurrentPlayer.IsGm)
            {
                person.PlayerId = CurrentPlayer.Id;
            }
            int pageId = Save(person);
            return pageId;
        }


        public void ChangePicture(int id, byte[] data, string mimeType)
        {
            Page page = _repository.Pages().FirstOrDefault(p => p.Id == id);
            if (page != null)
            {
                page.ImageData = data;
                page.ImageMime = mimeType;
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
                if (pageText.Contains(string.Format(">{0} ",page.ShortName.ToLower())))
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

            return pageId;
        }

 

        private bool AnotherPageExistsWithThisName(string shortName, int id)
        {
            return _repository.Pages().Any(p => p.ShortName == shortName && p.Id != id);
        }

        public Page GetPage(int id)
        {
            return _repository.Pages().FirstOrDefault(p => p.Id == id);
        }

        public ICollection<Page> RecentPages()
        {
            return _repository.Pages().OrderByDescending(p => p.SignificantUpdate).Take(20).ToList();
        }

        public ICollection<Page> MyStuff()
        {
            return _repository.Pages().Where(p => p.CreatedById == CurrentPlayer.Id).OrderByDescending(p => p.Created).ToList();
        }

        public ICollection<Session> Sessions()
        {
            return _repository.Pages().OfType<Session>().ToList();
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
            
            int pageId =  Save(person);
            return pageId;
        }

        private void NotifyAddPage(int pageId)
        {
            List<Player> players = GetPlayersWithSetting(SettingNames.SendEmailOnNewPage);
            Page page = GetPage(pageId);
            if (page != null && players.Any())
            {
                _email.NotifyNewPage(page, players);
            }
        }

        private void NotifyAddComment(int pageId, string commenterName, string description)
        {
            List<Player> players = GetPlayersWithSetting(SettingNames.SendEmailOnNewComment);
            Page page = GetPage(pageId);
            if (page != null && players.Any())
            {
                _email.NotifyNewComment(commenterName, page, players, description);
            }
        }

        private void NotifyEditPage(int pageId)
        {
            List<Player> players = GetPlayersWithSetting(SettingNames.SendEmailOnUpdatePage);
            Page page = GetPage(pageId);
            if (page != null && players.Any())
            {
                _email.NotifyEditPage(page, players);
            }
        }

        private List<Player> GetPlayersWithSetting(SettingNames settingName, bool includeSelf = false)
        {
            Setting setting = _repository.Settings().FirstOrDefault(s => s.Name == settingName.ToString());
            if (setting != null)
            {
                var players =
                    _repository.Players().Where(p => p.UserSettings.Any(s => s.Enabled && s.SettingId == setting.Id));
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

        public ICollection<Page> PinnedPages()
        {
            return _repository.Pages().Where(p => p.Pinned).ToList();
        }

        public ICollection<Page> NewPages()
        {
            return _repository.Pages().Where(p => p.PageViews.All(v => v.PlayerId != CurrentPlayer.Id)).ToList();
        }

        public ICollection<Page> ModifiedPages()
        {
            return _repository.Pages().Where(p => p.PageViews.Any(v => v.PlayerId == CurrentPlayer.Id && v.Viewed < p.SignificantUpdate)).ToList();
        }

        public void PinPage(int id)
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
                    pageView = new PageView{ PageId = page.Id, PlayerId = CurrentPlayer.Id, Viewed = DateTime.Now };
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
                            AwardTrophy(person.Id, trophy.Id, string.Format(": {0}",causeOfDeath));
                        }
                    }
                }
            }
        }

        public Trophy GetTrophy(int id)
        {
            return _repository.Trophies().FirstOrDefault(t => t.Id == id);
        }

        public int AddTrophy(string name, string description, int pointsValue, byte[] imageData, string mimeType)
        {
            Trophy trophy = new Trophy();
            trophy.Name = name;
            trophy.Description = description;
            trophy.PointsValue = pointsValue;
            trophy.ImageData = imageData;
            trophy.MimeType = mimeType;
            return _repository.Save(trophy);       
        }

        public void UpdateTrophy(int id, string name, string description, int pointsValue, byte[] imageData, string mimeType)
        {
            Trophy trophy = GetTrophy(id);
            if (trophy != null)
            {
                trophy.Name = name;
                trophy.Description = description;
                trophy.PointsValue = pointsValue;
                trophy.ImageData = imageData;
                trophy.MimeType = mimeType;
                _repository.Save(trophy);
            }
        }

        public void UpdateTrophy(int id, string name, string description, int pointsValue)
        {
            Trophy trophy = GetTrophy(id);
            if (trophy != null)
            {
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

                    PersonStat stat = person.PersonStats.FirstOrDefault(p => p.StatId == (int) keyValuePair.Key);

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

        public void AddXp(int personId, int xpValue)
        {
            Person person = GetPerson(personId);
            person.CurrentXp = person.CurrentXp + xpValue;
            Save(person);
        }

        public bool CheckStatPermissions(int personId)
        {
            if (!SiteHasFeature(Feature.SimpleStats))
            {
                return false;
            }

            if (!CurrentPlayer.IsGm)
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
            return _repository.People().Where(p => p.IsDead).OrderBy(s => s.FullName).ToList();
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
            var allPossibleFeatures = Enum.GetNames(typeof (Feature)).ToList().Where(s => s != "Unknown");
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
            List<Page> pages = RecentPages().ToList();
            List<Award> awards = _repository.Awards().OrderByDescending(a => a.AwardedOn).Take(20).ToList();
            List<Comment> comments = RecentComments();

            Dictionary<DateTime, object> dateObject = new Dictionary<DateTime, object>();

            foreach (Page page in pages)
            {
                dateObject.Add(page.SignificantUpdate, page);
            }

            foreach (Award award in awards)
            {
                dateObject.Add(award.AwardedOn, award);
            }

            foreach (Comment comment in comments)
            {
                dateObject.Add(comment.Created, comment);
            }

            var list = dateObject.Keys.ToList();
            list = list.OrderByDescending(l => l).ToList();

            List<object> results = list.Take(20).Select(date => dateObject[date]).ToList();

            return results;
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
            return
                _repository.People()
                    .Where(p => !p.PlayerId.HasValue)
                    .Select(p => new PageListItemModel {Id = p.Id, Fullname = p.FullName})
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
            return _repository.ScoreHistories().Where(s => s.PersonId == id).Where(a => a.DateTime == DateTime.Today).OrderBy(s => s.ScoreTypeId).ToList();

        }

        public void SetDefaultHitPoints(int id)
        {
            Person person = GetPerson(id);
            if (person != null)
            {
                if (!person.SimpleHitPoints.Any(s => s.Purchased.HasValue))
                {
                    BuyHitPointSlot(id, SimpleHitPointLevel.Slight, SimpleHitPointType.Harm,free: true);
                    BuyHitPointSlot(id, SimpleHitPointLevel.Significant, SimpleHitPointType.Harm,free: true);
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
                        person.CurrentXp = person.CurrentXp - cost;
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

        public void AddXpForSession(int sessionId, int xpAwarded)
        {
            Session session = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                List<Person> people = session.People.ToList();

                //always award all players regardless - just to be fair
                List<Person> playerCharacters = _repository.People().Where(p => p.PlayerId.HasValue).ToList();
                foreach (Person person in playerCharacters)
                {
                    if (people.All(p => p.Id != person.Id))
                    {
                        people.Add(person);      
                    }
                }

                foreach (Person person in people)
                {
                    AddXp(person.Id, xpAwarded);
                }

                session.XpAwarded = session.XpAwarded + xpAwarded;
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
                person.CurrentXp = 0;

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

        public Person PersonWithMyAward(TrophyType awardType)
        {
            return _repository.People().FirstOrDefault(p => p.Awards.Any(a => a.NominatedById == CurrentPlayer.Id
                                                                  && a.Trophy.TypeId == (int) awardType));
        }

        public List<Page> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return  new List<Page>();
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

        public List<Person> TopNpcs()
        {
            List<Person> people =
                _repository.People()
                    .Where(p => !p.PlayerId.HasValue)
                    .OrderByDescending(p => p.CurrentScore)
                    .Take(5)
                    .ToList();
            return people;
        }

        public List<Person> AllNpcs()
        {
            return People().Where(p => p.PlayerId == null).ToList();
        }

        public void SetMyAward(int personId, TrophyType trophyType)
        {
            Person person = GetPerson(personId);
            Trophy trophy = Trophies().FirstOrDefault(t => t.TypeId == (int) trophyType);
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
            }
        }

        private void ApplyXpForSession(Session session)
        {
            List<Person> people = People().ToList();
            foreach (Person person in people)
            {
                if (!person.IsDead)
                {
                    person.CurrentXp++;
                    if (session.People.Contains(person))
                    {
                        person.CurrentXp++;
                    }
                    if (session.Posts.Any(p => p.CharacterId == person.Id))
                    {
                        person.CurrentXp++;
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
                Save(session);
            }
        }

        public List<Session> UpdatedTextSessions()
        {
           List<Session> pages =
                _repository.Pages()
					.OfType<Session>().Where(p => p.IsTextSession && !p.IsClosed).ToList();

	        if (!CurrentPlayer.IsGm)
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
            List<Session> pages =
                 _repository.Pages()
					 .OfType<Session>().Where(p => p.IsTextSession && !p.IsClosed)
						.ToList();

            return pages.Where(p => _factory.GetSession(p.Id).CurrentPlayerId == CurrentPlayer.Id || (p.IsGmTurn && CurrentPlayer.IsGm)).OrderBy(p => p.LastPostTime).ToList();
        }

	    public List<Session> TextSessionsContainingMyCharacters()
	    {
			List<Session> pages =
				 _repository.Pages()
					 .OfType<Session>().Where(p => p.IsTextSession).ToList();

			return pages.Where(p => p.PlayerCharacters.Any(c => c.PlayerId == CurrentPlayer.Id || CurrentPlayer.IsGm)).OrderBy(p => p.LastPostTime).ToList();
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
            return _repository.Comments().OrderByDescending(d => d.Created).Take(20).ToList();
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
                if (SiteHasFeature(Feature.PublicLeague) || (SiteHasFeature(Feature.AdminLeague) && CurrentUserIsAdmin))
                {
                    people = People().Where(p => p.CurrentScore > 0).OrderByDescending(s => s.PointsValue).ThenByDescending(s => s.Modified).ToList();
                }
            else
                {
                    people =
                        People().Where(p => p.CurrentScore > 0)
                            .Where(p => !p.PlayerId.HasValue || p.PlayerId == CurrentPlayer.Id)
                            .OrderByDescending(s => s.PointsValue)
                            .ThenByDescending(s => s.Modified)
                            .ToList();
                    people = ApplyUplift(people);
                    people = ApplyNail(people);
                }
            }
            return people;
        }

        public List<Person> OtherPCs()
        {
            return _repository.People().Where(p => p.PlayerId.HasValue && p.PlayerId != CurrentPlayer.Id).ToList();
        }

	    private List<Person> ApplyNail(List<Person> people)
        {
            if (NailId != 0 && people.FirstOrDefault(p => p.Id == NailId) != null)
            {
                people.First(p => p.Id == NailId).InclueUplift = true;
                people.First(p => p.Id == NailId).UpliftFactor = 0;
                people = people.OrderByDescending(s => s.PointsValue).ThenByDescending(s => s.Modified).ToList();
            }
            return people;
        }

        private List<Person> ApplyUplift(List<Person> people)
        {
            if (UpliftId != 0 && people.First().Id != UpliftId && people.FirstOrDefault(p => p.Id == UpliftId) != null)
            {
                if (people.First(p => p.Id == UpliftId).PlayerId == CurrentPlayer.Id)
                {
                    people = Uplift(people);
                }
            }
            return people;
        }

        private List<Person> Uplift(List<Person> people)
        {
            people.First(p => p.Id == UpliftId).InclueUplift = true;
            people.First(p => p.Id == UpliftId).UpliftFactor = 1.05;
            people = people.OrderByDescending(s => s.PointsValue).ThenByDescending(s => s.Modified).ToList();
            while (people.First().Id != UpliftId)
            {
                people.First(p => p.Id == UpliftId).UpliftFactor = people.First(p => p.Id == UpliftId).UpliftFactor + 0.05;
                people = people.OrderByDescending(s => s.PointsValue).ThenByDescending(s => s.Modified).ToList();
            }
            return people;
        }

        private List<int> GetExlusiveTrophyTypes(TrophyType trophyType)
        {
            List<int> favAwardId = new List<int>
            {
                (int)TrophyType.FirstFavouriteNpc,
                (int)TrophyType.SecondFavouriteNpc,
                (int)TrophyType.ThirdFavouriteNpc,
            };

            if (favAwardId.Contains((int) trophyType))
            {
                return favAwardId;
            }

            return new List<int>{(int)trophyType};
        }

        private int AddComment(int pageId, string description, bool isAdmin, int? personId)
        {
            Page page = GetPage(pageId);

            if(page != null)
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
				if (session.IsGmTurn)
				{
					return _repository.Players().FirstOrDefault(p => p.IsGm);
				}
				else
				{
					PostOrder postOrder = session.PostOrders.OrderBy(po => po.LastTurnEnded).FirstOrDefault();
                    
					return postOrder != null ? postOrder.Player : null;
				}
			}
			else
			{
				return null;
			}
		}

		public List<Session> OpenTextSessions()
		{
			return _repository.Pages().OfType<Session>().Where(s => s.IsTextSession && !s.IsClosed).ToList();
		}

		public List<Session> MyOpenTextSessions()
		{
			return
				OpenTextSessions()
					.Where(s => s.PlayerCharacters.Any(p => p.PlayerId == CurrentPlayer.Id) || CurrentPlayer.IsGm).ToList();
		}

		public List<Session> ModifiedTextSessions()
		{
			return _repository.Pages().OfType<Session>().ToList().Where(p => p.PageViews.Any(v => v.PlayerId == CurrentPlayer.Id && v.Viewed < p.LastPostTime)).ToList();
		}

        public bool SiteHasFeature(Feature feature)
        {
            string featureName = feature.ToString();
            return _repository.SiteFeatures().Any(f => f.Name == featureName && f.IsEnabled);
        }

        public void EnableFeature(string featureName)
        {
            SiteFeature feature = _repository.SiteFeatures().FirstOrDefault(f => f.Name == featureName);

            if (feature == null)
            {
                feature = new SiteFeature { Name = featureName, Description = featureName };
            }

            if (!feature.IsEnabled)
            {
                feature.IsEnabled = true;
                _repository.Save(feature);
            }
        }

        public void DisableFeature(string featureName)
        {
            SiteFeature feature = _repository.SiteFeatures().FirstOrDefault(f => f.Name == featureName);

            if (feature != null)
            {
                if (feature.IsEnabled)
                {
                    feature.IsEnabled = false;
                    _repository.Save(feature);
                }
            }
        }
    }
}
