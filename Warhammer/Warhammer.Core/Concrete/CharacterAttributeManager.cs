using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using Newtonsoft.Json.Serialization;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Core.Concrete
{
    public class CharacterAttributeManager : ICharacterAttributeManager
    {
        private readonly IRepository _repo;
        private readonly IAuthenticatedUserProvider _user;
        private readonly ISiteFeatureProvider _featureProvider;
        private static object _editLock = new object();

        public CharacterAttributeManager(IRepository repo, IAuthenticatedUserProvider user, ISiteFeatureProvider featureProvider)
        {
            _repo = repo;
            _user = user;
            _featureProvider = featureProvider;
        }

        public CharacterAttributeModel GetCharacterAttributes(int personId)
        {
            bool isV3 = _featureProvider.SiteHasFeature(Feature.Version3);
            bool isV4 = _featureProvider.SiteHasFeature(Feature.Version4);

            Person person = _repo.People().Where(p => p.Id == personId)
                .Include(p => p.Player)
                .Include(p => p.PersonAttributes).FirstOrDefault();

            CampaignDetail campaignDetail = _repo.CampaignDetails().FirstOrDefault();

            Player player = _repo.Players().FirstOrDefault(p => p.UserName == _user.UserName);

            if (person != null && player != null && campaignDetail != null)
            {
                int totalRoles = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Role).Sum(p => p.CurrentValue);
                int totalDisciplines = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Discipline).Sum(p => p.CurrentValue);
                int totalSkills = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Skill).Sum(p => p.CurrentValue);
                int totalStats = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Stat || p.AttributeType == AttributeType.Magic || p.AttributeType == AttributeType.MagicItem).Sum(p => p.CurrentValue);
                int totalDescriptors = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Descriptor).Sum(p => p.CurrentValue);
                int totalEdge = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Edge).Sum(p => p.CurrentValue);
                int totalWear = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Wear).Sum(p => p.CurrentValue);
                int totalHarm = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Harm).Sum(p => p.CurrentValue);
                int numberOfWear = person.PersonAttributes.Count(p => p.AttributeType == AttributeType.Wear);
                int numbeOfHarm = person.PersonAttributes.Count(p => p.AttributeType == AttributeType.Harm);
                int numberOfEdge = person.PersonAttributes.Count(p => p.AttributeType == AttributeType.Edge);
                var averageStat = GetAverageStatValue();

                CharacterLevelInfo info = new CharacterLevelInfo
                {
                    TotalAdvancesTaken = person.TotalAdvancesTaken,
                    IsV3 = isV3,
                    IsV4 = isV4,
                    CurrentXp = person.CurrentXp,
                    XpSpent = person.XpSpent,
                    TotalRoles = totalRoles,
                    TotalDisciplines = totalDisciplines,
                    TotalStats = totalStats,
                    TotalSkills = totalSkills,
                    TotalEdge = totalEdge,
                    TotalWear = totalWear,
                    TotalHarm = totalHarm,
                    NumberOfWear = numberOfWear,
                    NumberOfHarm = numbeOfHarm,
                    NumberOfEdge = numberOfEdge,
                    TotalDescriptors = totalDescriptors,
                    CanEdit = person.Player?.UserName == player.UserName || campaignDetail.GmId == player.Id,
                    AverageStatValue = averageStat,
                    NumberOfStats =  person.PersonAttributes.Count(c => c.AttributeType == AttributeType.Stat),
                    HasAttributeMoveAvailable = person.HasAttributeMoveAvailable,
                    FixedWearAndHarm = _featureProvider.SiteHasFeature(Feature.FixedHarmAndWear),
                };

                CharacterAttributeModel model = new CharacterAttributeModel
                {
                    PersonId = person.Id,
                    PersonName = person.ShortName,
                    CharacterInfo = info,
                    PersonAttributes = person.PersonAttributes.Select(a => new PersonAttributeAdvanceModel
                    {
                        PersonAttribute = a,
                        CharacterInfo = info
                    }).ToList(),
                    WishingWell =  person.WishingWell,
                    CurrentResolve = person.CurrentResolve,
                    CanEditResolve = _user.IsAdmin || campaignDetail.GmId == player.Id,
                    CanEditWishingWell = _user.IsAdmin || campaignDetail.GmId == player.Id,
                    PlayerId = player.Id,
                    FixedWearAndHarm = _featureProvider.SiteHasFeature(Feature.FixedHarmAndWear),
                    IncludeMagic = _featureProvider.SiteHasFeature(Feature.CrowMagic),
                    IncludeDisciplines = _featureProvider.SiteHasFeature(Feature.Vampire),
                    CampaignDetail = campaignDetail,
                    CanAddXp = campaignDetail.GmId == player.Id || _featureProvider.SiteHasFeature(Feature.PlaygroundMode),
                    ShowWearTrack = _featureProvider.SiteHasFeature(Feature.ShowWearTrack),
                    ShowWishingWell = _featureProvider.SiteHasFeature(Feature.ShowWishingWell) && !person.IsNpc,
                    ShowResolveAndResilience = _featureProvider.SiteHasFeature(Feature.ResolveAndResilience),
                    IsV3 = isV3,
                    IsV4 = isV4,
                };

                return model;
            }

            return null;
        }

        public bool BuyAttributeAdvance(int personId, int attributeId)
        {
            lock (_editLock)
            {
                CharacterAttributeModel model = GetCharacterAttributes(personId);
                PersonAttributeAdvanceModel attribute =
                    model.PersonAttributes.FirstOrDefault(p => p.PersonAttribute.Id == attributeId);

                if (attribute != null && attribute.CanBuy)
                {
                    Person person = _repo.People().Include(p => p.PersonAttributes)
                        .FirstOrDefault(p => p.Id == personId);
                    PersonAttribute personAttribute = person?.PersonAttributes.FirstOrDefault(a => a.Id == attributeId);
                    if (personAttribute != null)
                    {
                        personAttribute.XpSpent += attribute.Cost;
                        person.XpSpent += attribute.Cost;
                        personAttribute.CurrentValue++;
                        person.TotalAdvancesTaken++;

                        _repo.Save(person);

                        model = GetCharacterAttributes(personId);
                        person.XpSpendAvailable = model.NpcAdvanceAvailable;
                        person.Modified = DateTime.UtcNow;
                        
                        _repo.Save(person);

                        return true;
                    }
                }

                return false;
            }
        }

        public bool BuyNewAttribute(int personId, AttributeType attributeType, string name, string description, int initialValue = 1)
        {
            lock (_editLock)
            {
                CharacterAttributeModel model = GetCharacterAttributes(personId);

                if (!model.CanAddNew(attributeType, initialValue))
                {
                    return false;
                }

                Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
                if (person != null)
                {
                    PersonAttribute personAttribute = new PersonAttribute
                    {
                        AttributeType = attributeType,
                        Name = name,
                        Description = description,
                        CurrentValue = initialValue,
                        InitialValue = initialValue,
                    };

                    personAttribute.XpSpent += model.NewCost(attributeType, initialValue);
                    person.XpSpent += model.NewCost(attributeType, initialValue);
                    person.TotalAdvancesTaken++;
                    person.PersonAttributes.Add(personAttribute);
                    
                    _repo.Save(person);
                    model = GetCharacterAttributes(personId);
                    person.XpSpendAvailable = model.NpcAdvanceAvailable;
                    _repo.Save(person);

                    return true;
                }
                return false;
            }
        }

        public bool MoveAttributePoint(int personId, int sourceAttributeId, int targetAttributeId)
        {
            lock (_editLock)
            {
                Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
                if (person != null)
                {
                    if (person.HasAttributeMoveAvailable)
                    {
                        PersonAttribute sourceAttribute =
                            person?.PersonAttributes.FirstOrDefault(
                                a => a.Id == sourceAttributeId);
                        PersonAttribute targetAttribute =
                            person?.PersonAttributes.FirstOrDefault(
                                a => a.Id == targetAttributeId);

                        if (sourceAttribute != null && targetAttribute != null)
                        {
                            sourceAttribute.CurrentValue--;
                            targetAttribute.CurrentValue++;
                            person.HasAttributeMoveAvailable = false;
                            _repo.Save(person);
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool RenameAttribute(int personId, int attributeId, string name, string description)
        {
            lock (_editLock)
            {
                Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
                if (person != null)
                {
                    if (person.HasAttributeMoveAvailable)
                    {
                        PersonAttribute attribute = person?.PersonAttributes.FirstOrDefault(a => a.Id == attributeId);

                        if (attribute != null)
                        {
                            attribute.Name = name;
                            attribute.Description = description;
                            person.HasAttributeMoveAvailable = false;
                            _repo.Save(person);
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public CharacterInitialStatsModel GetDefaultStats(int personId)
        {
            CharacterInitialStatsModel model = new CharacterInitialStatsModel();
            model.PersonId = personId;

            model.IncludeMagic = _featureProvider.SiteHasFeature(Feature.CrowMagic);

            int? averageStatDefault = _repo.CampaignDetails().Select(c => c.AverageStat).FirstOrDefault();


            if (averageStatDefault.HasValue)
            {
                model.AverageStat = averageStatDefault.Value;
            }
            else
            {
                model.AverageStat = 3;
            }

            foreach (int statId in Enum.GetValues(typeof(StatName)).AsQueryable())
            {
                if (statId < 100)
                {
                    StatName stat = (StatName)statId;
                    model.Stats.Add(new StatInitModel
                    {
                            CurrentValue = model.AverageStat,
                            InitialValue = model.AverageStat,
                            MinValue = 1,
                            StatName = stat,           
                    });
                }
            }

            model.TotalStats = model.AverageStat*model.Stats.Count;

            return model;
        }

        public bool InitializeStats(CharacterInitialStatsModel model)
        {
            lock (_editLock)
            {
                Player player = _repo.Players().Single(p => p.UserName == _user.UserName);
                CampaignDetail campaignDetail = _repo.CampaignDetails().FirstOrDefault();

                bool playerIsGm = player.Id == campaignDetail?.GmId;

                Person person = _repo.People().Include(p => p.PersonAttributes)
                    .FirstOrDefault(p => p.Id == model.PersonId);
                if (person != null)
                {
                    var averageStat = GetAverageStatValue();

                    int expectedTotal = averageStat * model.Stats.Count;

                    List<MagicInitModel> MagicStats = model.Magic.Where(i => i.InitialValue > 0 && !string.IsNullOrWhiteSpace(i.Name)).ToList();
                    List<MagicInitModel> MagicItems = model.MagicItems.Where(i => i.InitialValue > 0 && !string.IsNullOrWhiteSpace(i.Name)).ToList();

                    int actualTotal = model.Stats.Sum(s => s.CurrentValue) + MagicStats.Sum(s => s.InitialValue) + MagicItems.Sum(s => s.InitialValue);


                    if (!playerIsGm && expectedTotal != actualTotal)
                    {
                        return false;
                    }

                    int skillLevel = averageStat;

                    if (skillLevel < 1)
                    {
                        skillLevel = 1;
                    }

                    ResetAttributes(person.Id);

                    foreach (StatInitModel statInitModel in model.Stats)
                    {
                        PersonAttribute addedStat = new PersonAttribute
                        {
                            AttributeType = AttributeType.Stat,
                            Name = statInitModel.StatName.ToString(),
                            Description = statInitModel.StatName.ToString(),
                            CurrentValue = statInitModel.CurrentValue,
                            InitialValue = statInitModel.CurrentValue,
                            IsPrivate = true
                        };
                        person.PersonAttributes.Add(addedStat);
                    }

                    foreach (MagicInitModel magic in MagicStats)
                    {
                        PersonAttribute addedStat = new PersonAttribute
                        {
                            AttributeType = AttributeType.Magic,
                            Name = magic.Name.ToString(),
                            Description = magic.Description,
                            CurrentValue = magic.InitialValue,
                            InitialValue = magic.InitialValue,
                            IsPrivate = true
                        };
                        person.PersonAttributes.Add(addedStat);
                    }

                    foreach (MagicInitModel magic in MagicItems)
                    {
                        PersonAttribute addedStat = new PersonAttribute
                        {
                            AttributeType = AttributeType.MagicItem,
                            Name = magic.Name.ToString(),
                            Description = magic.Description,
                            CurrentValue = magic.InitialValue,
                            InitialValue = magic.InitialValue,
                            IsPrivate = true
                        };
                        person.PersonAttributes.Add(addedStat);
                    }

                    person.PersonAttributes.Add(new PersonAttribute
                    {
                        AttributeType = AttributeType.Role,
                        Name = model.InitialRoleName,
                        Description = model.InitialRoleName,
                        InitialValue = skillLevel,
                        CurrentValue = skillLevel
                    });

                    SetInitialSkill(model.InitialFirstSkillName, playerIsGm, person, skillLevel);

                    if (skillLevel > 1)
                    {
                        skillLevel--;
                    }

                    SetInitialSkill(model.InitialSecondSkillName, playerIsGm, person, skillLevel);

                    if (skillLevel > 1)
                    {
                        skillLevel--;
                    }

                    SetInitialSkill(model.InitialThirdSkillName, playerIsGm, person, skillLevel);


                    SetInitialDescriptor(model.InitialFirstDescriptorName, playerIsGm, person);
                    SetInitialDescriptor(model.InitialSecondDescriptorName, playerIsGm, person);
                    SetInitialDescriptor(model.InitialThirdDescriptorName, playerIsGm, person);

                    if (_featureProvider.SiteHasFeature(Feature.ResolveAndResilience))
                    {
                        AddResolveAndResilience(person);
                    }
                    else
                    {
                        AddDefaultWearAndHarm(person);
                    }


                    _repo.Save(person);

                    return true;
                }

                return false;
            }
        }

        private void AddResolveAndResilience(Person person)
        {
            if (!person.IsNpc)
            {
                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Edge,
                    Name = "",
                    Description = "",
                    InitialValue = 1,
                    CurrentValue = 1
                });

                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Harm,
                    Name = "",
                    Description = "",
                    InitialValue = 4,
                    CurrentValue = 4
                });

                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Resolve,
                    Name = "Resolve",
                    Description = "Resolve",
                    InitialValue = 3,
                    CurrentValue = 3
                });
            }

            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Resilience,
                Name = "",
                Description = "",
                InitialValue = 2,
                CurrentValue = 2
            });


            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Harm,
                Name = "",
                Description = "",
                InitialValue = 2,
                CurrentValue = 2
            });

        }

        private static void SetInitialSkill(string skillName, bool playerIsGm, Person person, int skillLevel)
        {
            if (string.IsNullOrEmpty(skillName))
            {
                if (!playerIsGm)
                {
                    skillName = "Unknown Skill";
                }
            }
            if (!string.IsNullOrEmpty(skillName))
            {
                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Skill,
                    Name = skillName,
                    Description = skillName,
                    InitialValue = skillLevel,
                    CurrentValue = skillLevel,
                    IsPrivate = true
                });
            }
        }

        private static void SetInitialDescriptor(string descriptor, bool playerIsGm, Person person)
        {
            if (string.IsNullOrEmpty(descriptor))
            {
                if (!playerIsGm)
                {
                    descriptor = "Unknown Descriptor";
                }
            }
            if (!string.IsNullOrEmpty(descriptor))
            {
                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Descriptor,
                    Name = descriptor,
                    Description = descriptor,
                    InitialValue = 1,
                    CurrentValue = 1
                });
            }
        }

        private int GetAverageStatValue()
        {
            int? averageStatDefault = _repo.CampaignDetails().Select(c => c.AverageStat).FirstOrDefault();

            int averageStat = 3;

            if (averageStatDefault.HasValue)
            {
                averageStat = averageStatDefault.Value;
            }
            return averageStat;
        }

        public void ResetAttributes(int id)
        {
            lock (_editLock)
            {
                Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == id);
                if (person != null)
                {
                    foreach (PersonAttribute personPersonAttribute in person.PersonAttributes.ToList())
                    {
                        _repo.Delete(personPersonAttribute);
                    }

                    person.XpSpent = 0;
                    person.TotalAdvancesTaken = 0;
                    _repo.Save(person);
                }
            }

        }

        public bool RefreshWear(int personId)
        {
            Player player = _repo.Players().FirstOrDefault(p => p.UserName == _user.UserName);
            Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                foreach (PersonAttribute personPersonAttribute in person.PersonAttributes.Where(a => a.AttributeType == AttributeType.Wear || a.AttributeType == AttributeType.Edge))
                {
                    personPersonAttribute.Name = null;
                }


                if (_featureProvider.SiteHasFeature(Feature.ResolveAndResilience))
                {
                    PersonAttribute resolve =
                        person.PersonAttributes.FirstOrDefault(r => r.AttributeType == AttributeType.Resolve);
                    if (resolve != null)
                    {
                        person.CurrentResolve = resolve.CurrentValue;
                    }

                    PostToTextSessions(person, $"Resolve and Edge refreshed for {person.FullName}.");
                }
                else
                {
                    PostToTextSessions(person, $"Wear and Edge refreshed for {person.FullName}.");
                }


                _repo.Save(person);

                return true;
            }
            return false;
        }

        public bool ApplyWear(int personId, int attributeId)
        {
            
            Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                

                foreach (PersonAttribute personPersonAttribute in person.PersonAttributes.Where(a => a.Id == attributeId))
                {
                    personPersonAttribute.Name = "Exhausted";

                    string message =
                        $"{personPersonAttribute.AttributeType} ({personPersonAttribute.CurrentValue}) Exhaused for {person.FullName}";

                    PostToTextSessions(person, message);
                }

                _repo.Save(person);

                return true;
            }
            return false;
        }

        public bool ApplyHarm(int personId, int attributeId, string harmMessage)
        {
            Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                foreach (PersonAttribute personPersonAttribute in person.PersonAttributes.Where(a => a.Id == attributeId))
                {
                    personPersonAttribute.Name = harmMessage;
                    string message = $"{personPersonAttribute.AttributeType} ({personPersonAttribute.CurrentValue}) Exhaused for {person.FullName}: {harmMessage}";
                    PostToTextSessions(person, message);
                }

                _repo.Save(person);

                return true;
            }
            return false;
        }

        public bool RefreshHarm(int personId, int attributeId)
        {
            Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                foreach (PersonAttribute personPersonAttribute in person.PersonAttributes.Where(a => a.Id == attributeId))
                {
                    personPersonAttribute.Name = null;
                    string message = $"{personPersonAttribute.AttributeType} ({personPersonAttribute.CurrentValue}) refreshed for {person.FullName}";
                    PostToTextSessions(person, message);
                }

                _repo.Save(person);

                return true;
            }
            return false;
        }

        private void PostToTextSessions(Person person, string message)
        {
            int playerId = _repo.Players().Single(p => p.UserName == _user.UserName).Id;
            List<Session> openTextSessions = person.Sessions.Where(s => s.IsTextSession && !s.IsClosed).ToList();
            foreach (Session session in openTextSessions)
            {
                session.Posts.Add(new Post
                {
                    PostType = (int) PostType.OutOfCharacter,
                    OriginalContent = message,
                    SessionId = session.Id,
                    CampaignId = person.CampaignId,
                    DatePosted = DateTime.UtcNow,
                    PlayerId = playerId,
                    RevisedContent = "",
                    RollValues = "",
                });
                _repo.Save(session);
            }
        }

        public bool SetDefaultWearAndHarm(int personId)
        {
            Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                foreach (PersonAttribute personPersonAttribute in person.PersonAttributes.Where(a => a.AttributeType == AttributeType.Harm || a.AttributeType == AttributeType.Wear).ToList())
                {
                    _repo.Delete(personPersonAttribute);
                }

                AddDefaultWearAndHarm(person);
                _repo.Save(person);
                return true;
            }


            return false;
        }

        public bool SetAttributeVisibility(int personId, int attributeId, bool isVisible)
        {
            PersonAttribute attribute = _repo.PersonAttributes().FirstOrDefault(a => a.Id == attributeId);
            if (attribute != null)
            {
                attribute.IsPrivate = !isVisible;

                _repo.Save(attribute);

                return true;
            }
            return false;
        }

        public bool AlterWishingWell(int personId, int amount)
        {
            Person person = _repo.People().FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                person.WishingWell = person.WishingWell + amount;
                _repo.Save(person);
                string message;
                if (_featureProvider.SiteHasFeature(Feature.Version3))
                {
                    message = $"Wyrd changed by {amount} for {person.FullName}. New Value: {person.WishingWell}";
                }
                else
                {
                    message = $"Wishing Well changed by {amount} for {person.FullName}. New Value: {person.WishingWell}";
                }
               
                PostToTextSessions(person, message);

                return true;
            }

            return false;
        }

        public bool SetCurrentResolve(int personId, int updatedResolve)
        {
            Person person = _repo.People().FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                person.CurrentResolve = updatedResolve;
                _repo.Save(person);

                string message;

                if (_featureProvider.SiteHasFeature(Feature.Version3))
                {
                    message = $"WEAR set to {updatedResolve} for {person.FullName}";
                }
                else
                {
                    message = $"Resolve set to {updatedResolve} for {person.FullName}";
                }
                 
                PostToTextSessions(person, message);
                return true;
            }

            return false;
        }

        public bool AddXp(int personId, decimal amount)
        {
            lock (_editLock)
            {
                Person person = _repo.People().FirstOrDefault(p => p.Id == personId);
                if (person != null)
                {
                    person.XPAwarded = person.XPAwarded + amount;
                    _repo.Save(person);

                    CharacterAttributeModel model = GetCharacterAttributes(personId);
                    person.XpSpendAvailable = model.NpcAdvanceAvailable;
                    _repo.Save(person);

                    return true;
                }

                return false;
            }
        }

        public bool SellAttributePoint(int personId, int targetAttributeId)
        {
            lock (_editLock)
            {
                CharacterAttributeModel model = GetCharacterAttributes(personId);

                PersonAttributeAdvanceModel attribute =
                    model.PersonAttributes.FirstOrDefault(a => a.Id == targetAttributeId);

                if (attribute == null || !attribute.CanSell)
                {
                    return false;
                }

                Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
                if (person != null)
                {
                    PersonAttribute personAttribute = person.PersonAttributes.Single(p => p.Id == targetAttributeId);
                    int saleValue = attribute.SaleValue;

                    if (personAttribute.CurrentValue == 1)
                    {
                        _repo.Delete(personAttribute);
                    }
                    else
                    {
                        personAttribute.XpSpent -= saleValue;
                        personAttribute.CurrentValue--;
                    }
                    person.XpSpent -= saleValue;
                    person.TotalAdvancesTaken--;
                    person.HasAttributeMoveAvailable = false;
                    _repo.Save(person);
                    model = GetCharacterAttributes(personId);
                    person.XpSpendAvailable = model.NpcAdvanceAvailable;
                    _repo.Save(person);

                    return true;
                }
                return false;
            }
        }


        private readonly Random _dice = new Random();
        private int Roll(int sides = 6, int count = 1)
        {
            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total = total + _dice.Next(1, sides + 1);
            }
            return total;
        }

        public bool InitRandomAttributes(RandomInitialStatsModel model)
        {
            bool isV4 = _featureProvider.SiteHasFeature(Feature.Version4);
            Person person = _repo.People().FirstOrDefault(p => p.Id == model.PersonId);
            if(person != null && !person.PersonAttributes.Any())
            {
                SkillLevel level;
                if (model.SkillLevel.HasValue)
                {
                    level = model.SkillLevel.Value;
                }
                else
                {
                    int randomLevel = 5;
                    randomLevel = Roll(randomLevel);
                    randomLevel = Roll(randomLevel);
                    level = (SkillLevel) randomLevel;
                }

                AgeBracket age;
                if(model.AgeBracket.HasValue)
                {
                    age = model.AgeBracket.Value;
                }
                else
                {
                    List<int> bracket = new List<int>
                    {
                        Roll(5),
                        Roll(5),
                        Roll(5)
                    };
                    age = (AgeBracket)bracket.OrderBy(b => b).Skip(1).Take(1).First();
                }


                AddRandomStats(model, person);
                if (!isV4)
                {
                    AddRandomRoles(model, level, age, person);
                }
                AddRandomSkills(age, person);
                AddRandomDescriptors(person);
                AddDefaultHarmV3(person);

                if (!person.IsNpc)
                {
                    person.PersonAttributes.Add(new PersonAttribute
                    {
                        AttributeType = AttributeType.Resolve,
                        Name = "Resolve",
                        Description = "Resolve",
                        InitialValue = 3,
                        CurrentValue = 3
                    });
                }

                _repo.Save(person);

                return true;
            }

            return false;
        }

        private void AddDefaultHarmV3(Person person)
        {
            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Harm,
                Name = "",
                Description = "Minor",
                InitialValue = 1,
                CurrentValue = 1
            });
            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Harm,
                Name = "",
                Description = "Serious",
                InitialValue = 2,
                CurrentValue = 2
            });
            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Harm,
                Name = "",
                Description = "Severe",
                InitialValue = 3,
                CurrentValue = 3
            });
            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Harm,
                Name = "",
                Description = "Critical",
                InitialValue = 4,
                CurrentValue = 4
            });
            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Harm,
                Name = "",
                Description = "Dead",
                InitialValue = 5,
                CurrentValue = 5
            });
        }

        private void AddRandomSkills(AgeBracket age, Person person)
        {
            bool isV4 = _featureProvider.SiteHasFeature(Feature.Version4);

            int skillBase = 10;
            if (isV4)
            {
                skillBase = 3;
            }

            List<string> skills = new List<string>();
            int numberOfSkills = Roll(4, (int) age) + skillBase;

            for (int i = 0; i <= numberOfSkills; i++)
            {
                skills.Add(GetRandomAttribute(AttributeType.Skill));
            }

            int rounds = Roll(2, (int) age);

            if (isV4)
            {
                rounds = rounds / 4;
                if (rounds < 1)
                {
                    rounds = 1;
                }
            }

            for (int i = 0; i <= rounds; i++)
            {
                int randomIndex = Roll(skills.Count) - 1;
                string skill = skills[randomIndex];
                skills.Add(skill);
            }

            var levels = from x in skills
                group x by x into g
                let count = g.Count()
                orderby count descending
                select new { Name = g.Key, Value = count - 1 };

            foreach (var skill in levels.Where(l => l.Value > 0))
            {
                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Skill,
                    Name = skill.Name,
                    Description = skill.Name,
                    InitialValue = skill.Value,
                    CurrentValue = skill.Value
                });
            }
        }

        private void AddRandomDescriptors(Person person)
        {
            string descriptor = GetRandomAttribute(AttributeType.Descriptor);
            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Descriptor,
                Name = descriptor,
                Description = descriptor,
                InitialValue = 1,
                CurrentValue = 1
            });
            descriptor = GetRandomAttribute(AttributeType.Descriptor);
            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Descriptor,
                Name = descriptor,
                Description = descriptor,
                InitialValue = 1,
                CurrentValue = 1
            });
            descriptor = GetRandomAttribute(AttributeType.Descriptor);
            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Descriptor,
                Name = descriptor,
                Description = descriptor,
                InitialValue = 1,
                CurrentValue = 1
            });
        }

        private void AddRandomRoles(RandomInitialStatsModel model, SkillLevel level, AgeBracket age, Person person)
        {
            Dictionary<string, int> roles = new Dictionary<string, int>();

            string primaryRole = model.PrimaryRole;

            if (string.IsNullOrWhiteSpace(primaryRole))
            {
                primaryRole = GetRandomAttribute(AttributeType.Role);
            }

            roles.Add(primaryRole, (int) level);

            int tries = 0;
            while (roles.Sum(r => r.Value) < (int) age && tries <= 4)
            {
                int currentSum = roles.Sum(r => r.Value);
                int randomLevel = Roll((int) age - currentSum + Roll(3));
                if (randomLevel > (int) level)
                {
                    randomLevel = (int) level;
                }

                randomLevel = Roll(randomLevel);
                randomLevel = Roll(randomLevel);
                randomLevel = Roll(randomLevel);

                string role = GetRandomAttribute(AttributeType.Role);
                if (!roles.ContainsKey(role))
                {
                    roles.Add(role, randomLevel);
                }

                tries++;
            }

            foreach (var role in roles)
            {
                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Role,
                    Name = role.Key,
                    Description = role.Key,
                    InitialValue = role.Value,
                    CurrentValue = role.Value
                });
            }
        }

        private string GetRandomAttribute(AttributeType type)
        {
            int count = _repo.RandomAttributeOptions()
                .Count(e => e.AttributeTypeEnum == (int)type);
            int item = Roll(count);
            return _repo.RandomAttributeOptions().OrderBy(s => s.Name)
                .Where(e => e.AttributeTypeEnum == (int) type)
                .Skip(item).Take(1).Select(r => r.Name).FirstOrDefault();
        }

        private void AddRandomStats(RandomInitialStatsModel model, Person person)
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();

            foreach (string name in Enum.GetNames(typeof(CoreStat)))
            {
                if (name == model.PrimaryStat?.ToString())
                {
                    stats.Add(name, 3);
                }
                else
                {
                    stats.Add(name, 1);
                }
            }

            while (stats.Sum(s => s.Value) < 15)
            {
                CoreStat stat = (CoreStat) Roll();
                if (stat != model.DumpStat)
                {
                    stats[stat.ToString()]++;
                }
            }

            foreach (var stat in stats)
            {
                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Stat,
                    Name = stat.Key,
                    Description = stat.Key,
                    InitialValue = stat.Value,
                    CurrentValue = stat.Value
                });
            }
        }

        private void AddDefaultWearAndHarm(Person person)
        {
            if (!person.IsNpc)
            {
                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Edge,
                    Name = "",
                    Description = "",
                    InitialValue = 1,
                    CurrentValue = 1
                });

                person.PersonAttributes.Add(new PersonAttribute
                {
                    AttributeType = AttributeType.Harm,
                    Name = "",
                    Description = "",
                    InitialValue = 4,
                    CurrentValue = 4
                });
            }

            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Wear,
                Name = "",
                Description = "",
                InitialValue = 2,
                CurrentValue = 2
            });

            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Harm,
                Name = "",
                Description = "",
                InitialValue = 2,
                CurrentValue = 2
            });

        }
    }
}
