using System.Collections.Generic;
using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class TrophyNominationViewModel
    {
        public SelectList Trophies { get; set; }
        public bool CanSetAnNemesis { get; set; }
        public bool CanSetAsFavourite { get; set; }
        public int PersonId { get; set; }
        public int SelectedTrophy { get; set; }
        public IEnumerable<AwardNomination> ExistingNominations { get; set; }
        public bool IsPrivate { get; set; }
        public bool JustNominated { get; set; }
        public List<Session> Sessions { get; set; }
        public int SelectedSession { get; set; }
    }
}