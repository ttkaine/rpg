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
        public bool FixedWearAndHarm { get; set; }
        public bool IncludeMagic { get; set; }
        public bool IncludeDisciplines { get; set; }

        public decimal CurrentXp => CharacterInfo.CurrentXp;
        public int XpSpent => CharacterInfo.XpSpent;
        public int TotalAdvancesTaken => CharacterInfo.TotalAdvancesTaken;
      //  public int CharacterLevel => CharacterInfo.CharacterLevel;
        public int TotalStats => CharacterInfo.TotalStats;
        public int TotalRoles => CharacterInfo.TotalRoles;
        public int TotalSkills => CharacterInfo.TotalSkills;
        public int TotalDescriptors => CharacterInfo.TotalDescriptors;

        public SelectList RenamableAttributes
        {
            get
            {
                return new SelectList(PersonAttributes
                    .Where(p => p.PersonAttribute.AttributeType == AttributeType.Skill 
                    || p.PersonAttribute.AttributeType == AttributeType.Role 
                    || p.PersonAttribute.AttributeType == AttributeType.Descriptor
                    || p.PersonAttribute.AttributeType == AttributeType.Magic
                    || p.PersonAttribute.AttributeType == AttributeType.MagicItem
                    ).Select(p => p), "Id", "Name");
            }
        }

        public SelectList PossibleAdvanceSales
        {
            get
            {
                return new SelectList(PersonAttributes.Where(p => p.CanSell), "Id", "SaleName");
            }
        }

        public SelectList PossibleSourceAttributes
        {
            get
            {
                return new SelectList(PersonAttributes.Where(p => p.PersonAttribute.IsStatType && p.PersonAttribute.CurrentValue > CharacterInfo.AverageStatValue - 2), "Id", "Name");
            }
        }
        public SelectList PossibleTargetAttributes
        {
            get
            {
                return new SelectList(PersonAttributes.Where(p => p.PersonAttribute.IsStatType), "Id", "Name");
            }
        }

        public CharacterLevelInfo CharacterInfo { get; set; }

        public List<PersonAttributeAdvanceModel> PersonAttributes { get; set; }

        public List<PersonAttributeAdvanceModel> Stats => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Stat).OrderBy(a => a.PersonAttribute.Id).ToList();
        public List<PersonAttributeAdvanceModel> Magic => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Magic).OrderBy(a => a.PersonAttribute.Id).ToList();
        public List<PersonAttributeAdvanceModel> MagicItems => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.MagicItem).OrderBy(a => a.PersonAttribute.Id).ToList();
        public List<PersonAttributeAdvanceModel> Skills => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Skill).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Roles => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Role).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Disciplines => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Discipline).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Descriptors => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Descriptor).OrderBy(a => a.PersonAttribute.Name).ToList();
        public List<PersonAttributeAdvanceModel> Wear => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Wear).OrderBy(a => a.PersonAttribute.CurrentValue).ToList();
        public List<PersonAttributeAdvanceModel> Harm => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Harm).OrderBy(a => a.PersonAttribute.CurrentValue).ToList();
        public List<PersonAttributeAdvanceModel> Edge => PersonAttributes.Where(a => a.PersonAttribute.AttributeType == AttributeType.Edge).OrderBy(a => a.PersonAttribute.CurrentValue).ToList();

        public bool CanAddNew(AttributeType type, int level = 1)
        {
            switch (type)
            {
                case AttributeType.Stat:
                    return false;
                case AttributeType.Magic:
                case AttributeType.MagicItem:
                case AttributeType.Skill:
                case AttributeType.Role:
                case AttributeType.Discipline:
                case AttributeType.Descriptor:
                case AttributeType.Harm:
                case AttributeType.Wear:
                case AttributeType.Edge:
                    return NewCost(type, level) <= CurrentXp;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public int NewCost(AttributeType type, int level = 1)
        {
            switch (type)
            {
                case AttributeType.Stat:
                    break;
                case AttributeType.Magic:
                case AttributeType.MagicItem:
                    var aStat = PersonAttributes.FirstOrDefault(s => s.PersonAttribute.IsStatType);
                    if (aStat != null)
                    {
                        return aStat.Cost;
                    }
                    return 99;
                case AttributeType.Skill:
                    int skillCost = 1;
                    return skillCost;
                case AttributeType.Role:
                case AttributeType.Discipline:
                    int roleCost = 4;
                    return roleCost;
                case AttributeType.Descriptor:
                    int descCost = TotalDescriptors;
                    descCost++;
                    if (descCost < 1)
                    {
                        descCost = 1;
                    }
                    return descCost;
                    case  AttributeType.Edge:
                        int edgeLevel = CharacterInfo.TotalEdge;
                        edgeLevel++;
                        return (edgeLevel * edgeLevel * edgeLevel);
                case AttributeType.Wear:
                    if (FixedWearAndHarm)
                    {
                        return level + CharacterInfo.NumberOfWear;
                    }
                    else
                    {
                        var aWear = PersonAttributes.FirstOrDefault(s => s.PersonAttribute.AttributeType == AttributeType.Wear);
                        if (aWear != null)
                        {
                            return aWear.Cost;
                        }

                        return 1;
                    }
                case AttributeType.Harm:
                    if (FixedWearAndHarm)
                    {
                        return level + CharacterInfo.NumberOfHarm;
                    }
                    else
                    {
                        var aHarm = PersonAttributes.FirstOrDefault(s => s.PersonAttribute.AttributeType == AttributeType.Harm);
                        if (aHarm != null)
                        {
                            return aHarm.Cost;
                        }

                        return 1;
                    }
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
        public bool CanBuyAny
        {
            get
            {
                return PersonAttributes.Any(c => c.CanBuy);
            }
        }

        private readonly List<AttributeType> _npcAdvanceAttributesToWatch = new List<AttributeType>
        {
            AttributeType.Descriptor,
            AttributeType.Magic,
            AttributeType.MagicItem,
            AttributeType.Role,
            AttributeType.Discipline,
            AttributeType.Skill,
            AttributeType.Stat
        };
        public bool NpcAdvanceAvailable
        {
            get
            {
                return PersonAttributes
                    .Where(p => _npcAdvanceAttributesToWatch.Contains(p.PersonAttribute.AttributeType)).Any(c => c.CanBuy);
            }
        }


        public int WishingWell { get; set; }
        public bool PlayerIsGm => PlayerId == CampaignDetail?.GmId;
        public bool CanAddXp { get; set; }
        public bool ShowWearTrack { get; set; }
        public bool ShowWishingWell { get; set; }
        public bool CanSellAdvance => PossibleAdvanceSales.Any();
    }
}