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
    
    public partial class PageView
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public int PlayerId { get; set; }
        public System.DateTime Viewed { get; set; }
    
        public virtual Page Page { get; set; }
        public virtual Player Player { get; set; }
    }
}
