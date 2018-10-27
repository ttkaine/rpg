using System.Collections.Generic;
using System.Web.Mvc;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class CreateSessionViewModel
    {
        public Session Session { get; set; }
        public List<PageToggleModel> LinkPages { get; set; }
        public string GameDate { get; set; }
        public int SelectedArcId { get; set; }
        public SelectList Arcs { get; set; }
    }
}