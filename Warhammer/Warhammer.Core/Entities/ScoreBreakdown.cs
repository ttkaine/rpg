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
    
    public partial class ScoreBreakdown
    {
        public int Id { get; set; }
        public System.DateTime DateTime { get; set; }
        public int PersonId { get; set; }
        public int ScoreTypeId { get; set; }
        public decimal PointsValue { get; set; }
        public int CampaignId { get; set; }
    
        public virtual Person Person { get; set; }
    }
}
