using System.Web.Mvc;

namespace Warhammer.Core.Models
{
    public class CreateCreatureViewModel
    {
        public  string Name { get; set; }
        
        public string Description { get; set; }

        public ThreatLevel ThreatLevel { get; set; }

        public int? ParentId { get; set; }

        public SelectList ParentOptions { get; set; }

    }
}