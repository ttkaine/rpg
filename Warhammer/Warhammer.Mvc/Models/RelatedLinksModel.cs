using System.Collections.Generic;
using System.Linq;
using Warhammer.Core;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class RelatedLinksModel
    {
        public int Id { get; }

        public RelatedLinksModel(int id, List<PageLinkModel> links)
        {
            Id = id;
            var links1 = links ?? new List<PageLinkModel>();

            Sessions = links1.Where(l => l.Type == PageLinkType.Session).OrderByDescending(l => l.Created).ToList();
            SessionLogs = links1.Where(l => l.Type == PageLinkType.SessionLog).OrderByDescending(l => l.Created).ToList();
            People = links1.Where(l => l.Type == PageLinkType.Person).OrderBy(l => l.ShortName).ToList();
            Places = links1.Where(l => l.Type == PageLinkType.Place).OrderBy(l => l.ShortName).ToList();
            Others = links1.Where(l => l.Type == PageLinkType.Other).OrderBy(l => l.ShortName).ToList();
        }

        public List<PageLinkModel> Sessions { get; }
        public List<PageLinkModel> SessionLogs { get; }
        public List<PageLinkModel> People { get;  }
        public List<PageLinkModel> Places { get; }
        public List<PageLinkModel> Others { get; }
    }
}