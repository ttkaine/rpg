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
    
    public partial class PostOrder
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int PlayerId { get; set; }
        public System.DateTime LastTurnEnded { get; set; }
        public int CampaignId { get; set; }
    
        public virtual Player Player { get; set; }
        public virtual Session Session { get; set; }
    }
}
