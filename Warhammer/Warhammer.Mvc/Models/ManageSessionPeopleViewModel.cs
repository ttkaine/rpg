using System.Collections.Generic;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class ManageSessionPeopleViewModel
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public List<PageToggleModel> People { get; set; }
    }
}