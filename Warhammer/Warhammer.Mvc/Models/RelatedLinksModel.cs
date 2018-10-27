using System.Collections.Generic;
using System.Linq;
using Warhammer.Core;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class RelatedLinksModel
    {
        private List<PageLinkModel> _links;

        public RelatedLinksModel(List<PageLinkModel> links, Page owner)
        {
            _links = links ?? new List<PageLinkModel>();

            Owner = owner;
            Sessions = _links.Where(l => l.Type == PageLinkType.Session).OrderByDescending(l => l.Created).ToList();
            SessionLogs = _links.Where(l => l.Type == PageLinkType.SessionLog).OrderByDescending(l => l.Created).ToList();
            People = _links.Where(l => l.Type == PageLinkType.Person).OrderBy(l => l.ShortName).ToList();
            Places = _links.Where(l => l.Type == PageLinkType.Place).OrderBy(l => l.ShortName).ToList();
            Others = _links.Where(l => l.Type == PageLinkType.Other).OrderBy(l => l.ShortName).ToList();
        }

        public Page Owner { get; private set; }
        public List<PageLinkModel> Sessions { get; private set; }
        public List<PageLinkModel> SessionLogs { get; private set; }
        public List<PageLinkModel> People { get; private set; }
        public List<PageLinkModel> Places { get; private set; }
        public List<PageLinkModel> Others { get; private set; }
    }
}