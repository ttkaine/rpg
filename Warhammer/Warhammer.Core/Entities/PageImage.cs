//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Warhammer.Core.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class PageImage
    {
        public PageImage()
        {
            this.Posts = new HashSet<Post>();
        }
    
        public int Id { get; set; }
        public int PageId { get; set; }
        public bool IsPrimary { get; set; }
        public int CampaignId { get; set; }
        public bool Public { get; set; }
        public string FileIdentifier { get; set; }
    
        public virtual Page Page { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
