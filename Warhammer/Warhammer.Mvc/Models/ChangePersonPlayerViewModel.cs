using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class ChangePersonPlayerViewModel
    {
        public int PersonId { get; set; }
        public int? PlayerId { get; set; }
        public List<Player> AvailablePlayers { get; set; }
        public string PersonName { get; set; }
    }
}