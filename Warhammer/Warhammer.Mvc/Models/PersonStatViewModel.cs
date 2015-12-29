using System;
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

        public bool Posted { get; set; }

        public bool MaySpendXp { get; set; }

        [Display(Name = "Stats (Total: 18)")]
        public Dictionary<StatName, int> Stats { get; set; }

        public int NextXpSpend
        {
            get { return Stats.Sum(s => s.Value) + Descriptors.Count + Roles.Count - 21; }
        }

        public int CurrentXp { get; set; }
        public List<string> Descriptors { get; set; }

        [Required]
        [Display(Name = "First Descriptor")]
        public string AddedDescriptor1 { get; set; }
        [Required]
        [Display(Name = "Second Descriptor")]
        public string AddedDescriptor2 { get; set; }
        [Required]
        [Display(Name = "Third Descriptor")]
        public string AddedDescriptor3 { get; set; }

        public StatName XpStatSpend { get; set; }

        public bool StatsCreated
        {
            get
            {
               return Stats.All(s => s.Value > 0);
            }
        }

        public bool ShowXpSpend
        {
            get { return NextXpSpend <= CurrentXp && StatsCreated; }
        }

        public List<string> Roles { get; set; }
        [Required]
        [Display(Name = "Role")]
        public string AddedRole { get; set; }

        public int PersonId { get; set; }
    }
}