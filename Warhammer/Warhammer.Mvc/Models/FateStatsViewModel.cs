using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class FateStatViewModel
    {
        public bool ShowHidden { get; set; }
        public FateStat Stat { get; set; }
        public SelectList Options { get; set; } 
    }
}