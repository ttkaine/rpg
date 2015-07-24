using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class CreatePlaceViewModel
    {
        public Place Place { get; set; }
        public SelectList  ParentPlace { get; set; }
        public int? ParentId { get; set; }
    }
}