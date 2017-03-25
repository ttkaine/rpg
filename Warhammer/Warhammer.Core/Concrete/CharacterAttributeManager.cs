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

        public CharacterAttributeManager(IRepository repo)
        {
            _repo = repo;
        }

        public CharacterAttributeModel GetCharacterAttributes(int personId)
        {
            Person person = _repo.People().Where(p => p.Id == personId)
                .Include(p => p.PersonAttributes).FirstOrDefault();

            CampaignDetail campaignDetail = _repo.CampaignDetails().FirstOrDefault();

            if (person != null)
            {
                int totalRoles = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Role).Sum(p => p.CurrentValue);
                int totalSkills = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Skill).Sum(p => p.CurrentValue);
                int totalStats = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Stat).Sum(p => p.CurrentValue);
                int totalDescriptors = person.PersonAttributes.Where(p => p.AttributeType == AttributeType.Descriptor).Sum(p => p.CurrentValue);

                CharacterLevelInfo info = new CharacterLevelInfo
                {
                    TotalAdvancesTaken = person.TotalAdvancesTaken,
                    CurrentXp = person.CurrentXp,
                    TotalRoles = totalRoles,
                    TotalStats = totalStats,
                    TotalSkills = totalSkills,
                    TotalDescriptors = totalDescriptors
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
                    _repo.Save(person);
                    return true;
                }
            }

            return false;
        }

        public bool BuyNewAttribute(int personId, AttributeType attributeType, string name, string description)
        {
            throw new System.NotImplementedException();
        }
    }
}