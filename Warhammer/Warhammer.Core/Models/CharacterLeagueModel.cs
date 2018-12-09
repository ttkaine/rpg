using System.Collections.Generic;

namespace Warhammer.Core.Models
{
    public class CharacterLeagueModel
    {
        public List<CharacterLeagueItemModel> Items { get; set; } = new List<CharacterLeagueItemModel>();
    }
}