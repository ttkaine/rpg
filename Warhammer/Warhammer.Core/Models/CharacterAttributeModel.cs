using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
        public int CharacterLevel => CharacterInfo.CharacterLevel;
        public int TotalStats => CharacterInfo.TotalStats;
        public int TotalRoles => CharacterInfo.TotalRoles;
        public int TotalSkills => CharacterInfo.TotalSkills;
        public int TotalDescriptors => CharacterInfo.TotalDescriptors;

        public SelectList RenamableAttributes
        {
            get
            {
                return new SelectList(PersonAttributes
                    .Where(p => p.PersonAttribute.AttributeType == AttributeType.Skill || p.PersonAttribute.AttributeType == AttributeType.Role || p.PersonAttribute.AttributeType == AttributeType.Descriptor).Select(p => p), "Id", "Name");
            }
        }

        public SelectList PossibleSourceAttributes
        {
            get
            {
                return new SelectList(PersonAttributes.Where(p => p.PersonAttribute.AttributeType == AttributeType.Stat && p.PersonAttribute.CurrentValue > CharacterInfo.AverageStatValue - 2), "Id", "Name");
            }
        }
        public SelectList PossibleTargetAttributes
        {
            get
            {
                return new SelectList(PersonAttributes.Where(p => p.PersonAttribute.AttributeType == AttributeType.Stat), "Id", "Name");
            }
        }

        public CharacterLevelInfo CharacterInfo { get; set; }

        public List<PersonAttributeAdvanceModel> PersonAttributes { get; set; }

        public List<PersonAttributeAdvanceModel> Stats => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Stat).OrderBy(a => a.PersonAttribute.Id).ToList();
        public List<PersonAttributeAdvanceModel> Skills => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Skill).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Roles => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Role).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Descriptors => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Descriptor).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Wear => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Wear).OrderBy(a => a.PersonAttribute.CurrentValue).ToList();
        public List<PersonAttributeAdvanceModel> Harm => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Harm).OrderBy(a => a.PersonAttribute.CurrentValue).ToList();
        public List<PersonAttributeAdvanceModel> Edge => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Edge).OrderBy(a => a.PersonAttribute.CurrentValue).ToList();

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
                case AttributeType.Edge:
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
                    int skillCost = 1;
                    return skillCost + CharacterLevel;
                case AttributeType.Role:
                    int roleCost = 4;
                    return roleCost + CharacterLevel;
                case AttributeType.Descriptor:
                    int descCost = TotalDescriptors - 3;

                    if (descCost < 1)
                    {
                        descCost = 1;
                    }

                    return descCost + CharacterLevel;
                    case  AttributeType.Edge:
                        return 1 + (CharacterInfo.TotalEdge * CharacterInfo.TotalEdge * CharacterInfo.TotalEdge) + CharacterLevel;
                case AttributeType.Wear:
                    int wearValue = CharacterInfo.TotalWear / 4;
                    if (wearValue < 1)
                    {
                        wearValue = 1;
                    }
                    return wearValue + CharacterLevel;
                case AttributeType.Harm:
                    int harmValue = CharacterInfo.TotalHarm / 4;
                    if (harmValue < 1)
                    {
                        harmValue = 1;
                    }
                    return harmValue + CharacterLevel;
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
        public bool CanAddXp { get; set; }
        public bool ShowWearTrack { get; set; }
    }
}