using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class PersonStatViewModel
    {
        public PersonStatViewModel()
        {
            Stats = new Dictionary<StatName, int>();
            Descriptors = new List<string>();
            Roles = new List<string>();
        }

        public bool ShowRoles { get; set; }
        public bool ShowStats { get; set; }
        public bool ShowDescriptors { get; set; }
        public bool IsFuHammer { get; set;}
        public bool IsCrow { get; set; }

        public bool Posted { get; set; }

        public bool MaySpendXp { get; set; }
        public int XpSpent { get; set; }
        public bool CanBuyDescriptor { get; set; }
        public bool CanBuyStat { get; set; }
        public bool CanBuyRole { get; set; }
        public int StatCost { get; set; }

        public Dictionary<StatName, int> Stats { get; set; }

        public decimal CurrentXp { get; set; }
        public List<string> Descriptors { get; set; }

        [Display(Name = "First Descriptor")]
        public string AddedDescriptor1 { get; set; }
        [Display(Name = "Second Descriptor")]
        public string AddedDescriptor2 { get; set; }
        [Display(Name = "Third Descriptor")]
        public string AddedDescriptor3 { get; set; }

        public StatName XpStatSpend { get; set; }

        public bool StatsCreated
        {
            get
            {
               return Stats.Sum(s => s.Value) > 0;
            }
        }

        public List<string> Roles { get; set; }
        [Display(Name = "Role")]
        public string AddedRole { get; set; }

        public int PersonId { get; set; }
        public int DescriptorCost { get; set; }
        public string CharacterName { get; set; }
        public int RoleCost { get; set; }
        public bool IsNpc { get; set; }
    }
}