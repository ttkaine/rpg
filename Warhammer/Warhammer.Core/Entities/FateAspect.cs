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
    
    public partial class FateAspect
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int AspectType { get; set; }
        public string AspectName { get; set; }
        public bool IsVisible { get; set; }
    
        public virtual Person Person { get; set; }
    }
}
