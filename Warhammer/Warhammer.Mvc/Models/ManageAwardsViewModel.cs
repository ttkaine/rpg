using System.Collections.Generic;
using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class ManageAwardsViewModel
    {
        public SelectList Trophies { get; set; }
        public int? SelectedTrophy { get; set; }
        public List<Award> Awards { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string Reason { get; set; }
        public List<Session> Sessions { get; set; }
        public int? SelectedSessionId { get; set; }
    }
}