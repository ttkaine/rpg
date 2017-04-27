using System;
using System.Data.Entity;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Core.Concrete
{
    public class CharacterAttributeManager : ICharacterAttributeManager
    {
        private readonly IRepository _repo;
        private readonly IAuthenticatedUserProvider _user;

        public CharacterAttributeManager(IRepository repo, IAuthenticatedUserProvider user)
        {
            _repo = repo;
            _user = user;
        }

        public CharacterAttributeModel GetCharacterAttributes(int personId)
        {
            Person person = _repo.People().Where(p => p.Id == personId)
                .Include(p => p.Player)
                .Include(p => p.PersonAttributes).FirstOrDefault();

            CampaignDetail campaignDetail = _repo.CampaignDetails().FirstOrDefault();

            Player player = _repo.Players().FirstOrDefault(p => p.UserName == _user.UserName);

            if (person != null && player != null && campaignDetail != null)
            {
                int totalRoles = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Role).Sum(p => p.CurrentValue);
                int totalSkills = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Skill).Sum(p => p.CurrentValue);
                int totalStats = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Stat).Sum(p => p.CurrentValue);
                int totalDescriptors = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Descriptor).Sum(p => p.CurrentValue);
                int totalWear = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Wear).Sum(p => p.CurrentValue);
                int totalHarm = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Harm).Sum(p => p.CurrentValue);
                int numberOfWear = person.PersonAttributes.Count(p => p.AttributeType == AttributeType.Wear);
                int numbeOfHarm = person.PersonAttributes.Count(p => p.AttributeType == AttributeType.Harm);
                var averageStat = GetAverageStatValue();
                CharacterLevelInfo info = new CharacterLevelInfo
                {
                    TotalAdvancesTaken = person.TotalAdvancesTaken,
                    CurrentXp = person.CurrentXp,
                    XpSpent = person.XpSpent,
                    TotalRoles = totalRoles,
                    TotalStats = totalStats,
                    TotalSkills = totalSkills,
                    TotalWear = totalWear,
                    TotalHarm = totalHarm,
                    NumberOfWear = numberOfWear,
                    NumberOfHarm = numbeOfHarm,
                    TotalDescriptors = totalDescriptors,
                    CanEdit = person.Player?.UserName == player.UserName || campaignDetail.GmId == player.Id,
                    AverageStatValue = averageStat,
                    NumberOfStats =  person.PersonAttributes.Count(c => c.AttributeType == AttributeType.Stat)
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
                    PlayerId = player.Id,
                    CampaignDetail = campaignDetail
                };

                return model;
            }

            return null;
        }

        public bool BuyAttributeAdvance(int personId, int attributeId)
        {
            CharacterAttributeModel model = GetCharacterAttributes(personId);
            PersonAttributeAdvanceModel attribute =
                model.PersonAttributes.FirstOrDefault(p => p.PersonAttribute.Id == attributeId);

            if (attribute != null && attribute.CanBuy)
            {
                Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
                PersonAttribute personAttribute = person?.PersonAttributes.FirstOrDefault(a => a.Id == attributeId);
                if (personAttribute != null)
                {
                    personAttribute.CurrentValue++;
                    personAttribute.XpSpent += attribute.Cost;
                    person.XpSpent += attribute.Cost;
                    person.TotalAdvancesTaken++;
                    person.XpSpendAvailable = model.CanBuyAll;
                    _repo.Save(person);
                    return true;
                }
            }

            return false;
        }

        public bool BuyNewAttribute(int personId, AttributeType attributeType, string name, string description)
        {
            CharacterAttributeModel model = GetCharacterAttributes(personId);

            if (!model.CanAddNew(attributeType))
            {
                return false;
            }

            if (model.PersonAttributes.Any(a => a.PersonAttribute.Name == name && a.PersonAttribute.AttributeType == attributeType))
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
                    CurrentValue = 1,
                    InitialValue = 1,
                };

                personAttribute.XpSpent += model.NewCost(attributeType);
                person.XpSpent += model.NewCost(attributeType);
                person.TotalAdvancesTaken++;
                person.PersonAttributes.Add(personAttribute);
                person.XpSpendAvailable = model.CanBuyAll;
                _repo.Save(person);

                return true;
            }
            return false;
        }

        public bool MoveAttributePoint(int personId, int sourceAttributeId, int targetAttributeId)
        {
            Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                if (person.HasAttributeMoveAvailable)
                {
                    PersonAttribute sourceAttribute = person?.PersonAttributes.FirstOrDefault(a => a.Id == sourceAttributeId && a.AttributeType == AttributeType.Stat);
                    PersonAttribute targetAttribute = person?.PersonAttributes.FirstOrDefault(a => a.Id == targetAttributeId && a.AttributeType == AttributeType.Stat);

                    if (sourceAttribute != null && targetAttribute != null)
                    {
                        sourceAttribute.CurrentValue--;
                        targetAttribute.CurrentValue++;
                        person.AttributeMoveTaken = true;
                        _repo.Save(person);
                        return true;
                    }
                }
            }

            return false;
        }

        public CharacterInitialStatsModel GetDefaultStats(int personId)
        {
            CharacterInitialStatsModel model = new CharacterInitialStatsModel();
            model.PersonId = personId;


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
            Player player = _repo.Players().Single(p => p.UserName == _user.UserName);
            CampaignDetail campaignDetail = _repo.CampaignDetails().FirstOrDefault();

            bool playerIsGm = player.Id == campaignDetail?.GmId;

            Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == model.PersonId);
            if (person != null)
            {
                var averageStat = GetAverageStatValue();

                int expectedTotal = averageStat * model.Stats.Count;
                if (!playerIsGm && expectedTotal != model.Stats.Sum(s => s.CurrentValue))
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

                AddDefaultWearAndHarm(person);

                _repo.Save(person);

                return true;
            }

            return false;
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
            if (_user.IsAdmin)
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
            Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                foreach (PersonAttribute personPersonAttribute in person.PersonAttributes.Where(a => a.AttributeType == AttributeType.Wear))
                {
                    personPersonAttribute.Name = null;
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
                foreach (PersonAttribute personPersonAttribute in person.PersonAttributes.Where(a => a.AttributeType == AttributeType.Wear && a.Id == attributeId))
                {
                    personPersonAttribute.Name = "Exhausted";
                }

                _repo.Save(person);

                return true;
            }
            return false;
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
            Person person = _repo.People().Include(p => p.PersonAttributes).FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                person.WishingWell = person.WishingWell + amount;
                _repo.Save(person);
                return true;
            }

            return false;
        }

        private void AddDefaultWearAndHarm(Person person)
        {
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
                AttributeType = AttributeType.Wear,
                Name = "",
                Description = "",
                InitialValue = 4,
                CurrentValue = 4
            });

            person.PersonAttributes.Add(new PersonAttribute
            {
                AttributeType = AttributeType.Harm,
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
                InitialValue = 4,
                CurrentValue = 4
            });
        }
    }
}