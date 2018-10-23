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
    
    public partial class CampaignDetail
    {
        public CampaignDetail()
        {
            this.PlayerCampaigns = new HashSet<PlayerCampaign>();
        }
    
        public int Id { get; set; }
        public Nullable<System.DateTime> CurrentGameDate { get; set; }
        public int CampaignId { get; set; }
        public int GmId { get; set; }
        public Nullable<int> AverageStat { get; set; }
        public string CustomCss { get; set; }
        public string Url { get; set; }
        public byte[] BackgroundImage { get; set; }
        public byte[] IconImage { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<int> ThemeId { get; set; }
    
        public virtual ICollection<PlayerCampaign> PlayerCampaigns { get; set; }
    }
}
