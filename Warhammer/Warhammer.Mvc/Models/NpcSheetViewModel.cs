using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class NpcSheetViewModel
    {
        public Person Person { get; set; }
        public Dictionary<string, int> Stats { get; set; }
        public Dictionary<string, int> Roles { get; set; }
        public Dictionary<string, int> Skills { get; set; }
        public List<string> Descriptors { get; set; }
    }
}