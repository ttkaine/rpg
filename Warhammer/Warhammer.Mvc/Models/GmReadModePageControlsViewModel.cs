using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class GmReadModePageControlsViewModel
    {
        public int Id { get; set; }
        public List<PageLinkModel> SessionLinksToOffer { get; set; }
        public bool LinksAvailable => SessionLinksToOffer != null && SessionLinksToOffer.Any();
        public bool ShowControlsPanel => LinksAvailable;
    }
}