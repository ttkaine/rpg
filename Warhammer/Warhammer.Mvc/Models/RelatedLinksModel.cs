using System.Collections.Generic;
using System.Linq;
using Warhammer.Core;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class RelatedLinksModel
    {
        private List<PageLinkModel> _links;

        public RelatedLinksModel(List<PageLinkModel> links)
        {
            _links = links ?? new List<PageLinkModel>();
        }

        public List<PageLinkModel> Sessions => _links.Where(l => l.Type == PageLinkType.Session).ToList();
        public List<PageLinkModel> SessionLogs => _links.Where(l => l.Type == PageLinkType.SessionLog).ToList();
        public List<PageLinkModel> People => _links.Where(l => l.Type == PageLinkType.Person).ToList();
        public List<PageLinkModel> Places => _links.Where(l => l.Type == PageLinkType.Place).ToList();
        public List<PageLinkModel> Others => _links.Where(l => l.Type == PageLinkType.Other).ToList();
    }
}