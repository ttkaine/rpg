using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class ChangePlaceParentViewModel
    {
        public int PlaceId { get; set; }
        public SelectList ParentPlace { get; set; }
        public int? ParentId { get; set; }
    }
}