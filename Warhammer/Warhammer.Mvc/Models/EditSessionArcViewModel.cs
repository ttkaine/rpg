using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class EditSessionArcViewModel
    {
        public int SessionId { get; set; }
        public string CurrentArcTitle { get; set; }
        public int SelectedArcId { get; set; }
        public SelectList Arcs { get; set; }
    }
}