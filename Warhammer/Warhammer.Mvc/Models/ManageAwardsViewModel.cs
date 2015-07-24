using System.Collections.Generic;
using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class ManageAwardsViewModel
    {
        public SelectList Trophies { get; set; }
        public int SelectedTrophy { get; set; }
        public Person Person { get; set; }
        public string Reason { get; set; }
    }
}