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
    
    public partial class CreatureAbility
    {
        public int Id { get; set; }
        public int CreatureId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AbilityThreatLevelId { get; set; }
    
        public virtual Creature Creature { get; set; }
    }
}
