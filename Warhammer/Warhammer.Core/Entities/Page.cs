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
    
    public partial class Page
    {
        public Page()
        {
            this.Comments = new HashSet<Comment>();
            this.PageViews = new HashSet<PageView>();
            this.Related = new HashSet<Page>();
            this.Pages = new HashSet<Page>();
            this.PageImages = new HashSet<PageImage>();
        }
    
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageMime { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public int CreatedById { get; set; }
        public int ModifedById { get; set; }
        public int SignificantUpdateById { get; set; }
        public System.DateTime SignificantUpdate { get; set; }
        public bool Pinned { get; set; }
        public string PlainText { get; set; }
        public int CampaignId { get; set; }
    
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual Player ModifiedBy { get; set; }
        public virtual Player CreatedBy { get; set; }
        public virtual ICollection<PageView> PageViews { get; set; }
        public virtual ICollection<Page> Related { get; set; }
        public virtual ICollection<Page> Pages { get; set; }
        public virtual ICollection<PageImage> PageImages { get; set; }
    }
}
