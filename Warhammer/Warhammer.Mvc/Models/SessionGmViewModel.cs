using System.Web.Mvc;

namespace Warhammer.Mvc.Models
{
    public class SessionGmViewModel
    {
        public int SessionId { get; set; }
        public int? SelectedGm { get; set; }
        public SelectList Players { get; set; }
        public bool GmJustSet { get; set; }
    }
}