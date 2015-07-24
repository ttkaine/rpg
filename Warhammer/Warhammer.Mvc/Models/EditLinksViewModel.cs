using System.Collections.Generic;
using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class EditLinksViewModel
    {
        public Page Page { get; set; }
        public SelectList LinkToList { get; set; }
        public int AddLinkTo { get; set; }
        public List<Page> CurrentLinks { get; set; } 
    }
}