using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models
{
    public class LeagueEntryViewModel
    {
        public int Position { get; set; }
        public decimal Score { get; set; }
        public int PersonId { get; set; }
        public string Name { get; set; }
        public List<Award> Awards { get; set; } 
    }
}