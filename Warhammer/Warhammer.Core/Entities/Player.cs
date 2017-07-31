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
    
    public partial class Player
    {
        public Player()
        {
            this.Awards = new HashSet<Award>();
            this.Comments = new HashSet<Comment>();
            this.Pages = new HashSet<Page>();
            this.Pages1 = new HashSet<Page>();
            this.PageViews = new HashSet<PageView>();
            this.People = new HashSet<Person>();
            this.Posts = new HashSet<Post>();
            this.UserSettings = new HashSet<UserSetting>();
            this.PlayerCampaigns = new HashSet<PlayerCampaign>();
            this.PostOrders = new HashSet<PostOrder>();
            this.AwardNominations = new HashSet<AwardNomination>();
        }
    
        public int Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public byte[] ImageData { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<Award> Awards { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Page> Pages { get; set; }
        public virtual ICollection<Page> Pages1 { get; set; }
        public virtual ICollection<PageView> PageViews { get; set; }
        public virtual ICollection<Person> People { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<UserSetting> UserSettings { get; set; }
        public virtual ICollection<PlayerCampaign> PlayerCampaigns { get; set; }
        public virtual ICollection<PostOrder> PostOrders { get; set; }
        public virtual ICollection<AwardNomination> AwardNominations { get; set; }
    }
}
