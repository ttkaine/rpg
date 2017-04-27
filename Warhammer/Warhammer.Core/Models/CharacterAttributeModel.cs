using System;
using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models
{
    public class CharacterAttributeModel
    {
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public int PlayerId { get; set; }
        public decimal CurrentXp => CharacterInfo.CurrentXp;
        public int XpSpent => CharacterInfo.XpSpent;
        public int TotalAdvancesTaken => CharacterInfo.TotalAdvancesTaken;
        public int TotalStats => CharacterInfo.TotalStats;
        public int TotalRoles => CharacterInfo.TotalRoles;
        public int TotalSkills => CharacterInfo.TotalSkills;
        public int TotalDescriptors => CharacterInfo.TotalDescriptors;

        public CharacterLevelInfo CharacterInfo { get; set; }

        public List<PersonAttributeAdvanceModel> PersonAttributes { get; set; }

        public List<PersonAttributeAdvanceModel> Stats => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Stat).OrderBy(a => a.PersonAttribute.Id).ToList();
        public List<PersonAttributeAdvanceModel> Skills => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Skill).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Roles => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Role).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Descriptors => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Descriptor).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Wear => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Wear).OrderBy(a => a.PersonAttribute.CurrentValue).ToList();
        public List<PersonAttributeAdvanceModel> Harm => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Harm).OrderBy(a => a.PersonAttribute.CurrentValue).ToList();

        public bool CanAddNew(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Stat:
                    return false;
                case AttributeType.Skill:
                case AttributeType.Role:
                case AttributeType.Descriptor:
                case AttributeType.Harm:
                case AttributeType.Wear:
                    return NewCost(type) <= CurrentXp;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public int NewCost(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Stat:
                    break;
                case AttributeType.Skill:
                    int skillCost = TotalAdvancesTaken + 1;

                    if (skillCost < 1)
                    {
                        skillCost = 1;
                    }

                    return skillCost;
                case AttributeType.Role:
                    int roleCost = TotalAdvancesTaken + 2;

                    if (roleCost < 1)
                    {
                        roleCost = 1;
                    }

                    return roleCost;
                case AttributeType.Descriptor:
                    return TotalAdvancesTaken + TotalDescriptors + 3;
                case AttributeType.Wear:
                    return TotalAdvancesTaken + (CharacterInfo.NumberOfWear * 2);
                case AttributeType.Harm:
                    return TotalAdvancesTaken + (CharacterInfo.NumberOfHarm * 2);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return -1;
        }

        public CampaignDetail CampaignDetail { get; set; }
        public bool HasStats => Stats.Any();

        public bool CanBuyAll
        {
            get
            {
                int max = PersonAttributes.Max(a => a.Cost);
                return CharacterInfo.CurrentXp >= max;         
            }
        }

        public int WishingWell { get; set; }
        public bool PlayerIsGm => PlayerId == CampaignDetail?.GmId;
    }
}