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
    
    public partial class PriceListItem
    {
        public PriceListItem()
        {
            this.Children = new HashSet<PriceListItem>();
        }
    
        public int Id { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> PriceInPence { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<PriceListItem> Children { get; set; }
        public virtual PriceListItem Parent { get; set; }
    }
}
