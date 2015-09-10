using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class SearchModel
    {
        public string SearchTerm { get; set; }
        public List<Page> Results { get; set; } 
    }
}