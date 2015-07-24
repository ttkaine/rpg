using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class CreateSessionLogViewModel
    {
        public SessionLog Log { get; set; }
        public SelectList Person { get; set; }
        public SelectList Session { get; set; }
        public int SelectedPersonId { get; set; }
        public int SelectedSessionId { get; set; }
    }
}