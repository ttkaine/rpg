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

        public decimal CurrentXp => CharacterInfo.CurrentXp;
        public int TotalAdvancesTaken => CharacterInfo.TotalAdvancesTaken;
        public int TotalStats => CharacterInfo.TotalStats;
        public int TotalRoles => CharacterInfo.TotalRoles;
        public int TotalSkills => CharacterInfo.TotalSkills;
        public int TotalDescriptors => CharacterInfo.TotalDescriptors;

        public CharacterLevelInfo CharacterInfo { get; set; }

        public List<PersonAttributeAdvanceModel> PersonAttributes { get; set; }

        public List<PersonAttributeAdvanceModel> Stats => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Stat).ToList();
        public List<PersonAttributeAdvanceModel> Skills => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Skill).ToList();
        public List<PersonAttributeAdvanceModel> Roles => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Role).ToList();
        public List<PersonAttributeAdvanceModel> Descriptors => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Descriptor).ToList();

        public bool CanAddNew(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Stat:
                    return false;
                case AttributeType.Skill:
                    return NewCost(type) <= CurrentXp;
                case AttributeType.Role:
                    return NewCost(type) <= CurrentXp;
                case AttributeType.Descriptor:
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
                    return TotalAdvancesTaken + TotalSkills + 1;
                case AttributeType.Role:
                    return (TotalAdvancesTaken + TotalRoles + 1);
                case AttributeType.Descriptor:
                    return TotalAdvancesTaken + (TotalDescriptors*3) + 3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return -1;
        }

        public CampaignDetail CampaignDetail { get; set; }

    }
}