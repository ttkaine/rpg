using System;

namespace Warhammer.Core.Models
{
    public class PageLinkWithUpdateDateModel : PageLinkModel
    {
        public DateTime LastUpdate { get; set; }
    }
}