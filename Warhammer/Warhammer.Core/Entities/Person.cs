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
    
    public partial class Person : Page
    {
        public Person()
        {
            this.Awards = new HashSet<Award>();
            this.PersonComments = new HashSet<Comment>();
            this.SessionLogs = new HashSet<SessionLog>();
            this.PersonStats = new HashSet<PersonStat>();
            this.FateAspects = new HashSet<FateAspect>();
            this.FateStats = new HashSet<FateStat>();
            this.FateStunts = new HashSet<FateStunt>();
            this.SimpleHitPoints = new HashSet<SimpleHitPoint>();
            this.Reputations = new HashSet<Reputation>();
            this.Assets = new HashSet<Asset>();
            this.ScoreHistories = new HashSet<ScoreHistory>();
            this.PersonAttributes = new HashSet<PersonAttribute>();
        }
    
        public bool IsDead { get; set; }
        public string Obiturary { get; set; }
        public Nullable<int> PlayerId { get; set; }
        public string CauseOfDeath { get; set; }
        public bool IsInMainParty { get; set; }
        public string Roles { get; set; }
        public string Descriptors { get; set; }
        public int XpSpent { get; set; }
        public decimal CurrentScore { get; set; }
        public decimal XPAwarded { get; set; }
        public Nullable<int> Crowns { get; set; }
        public Nullable<int> Shillings { get; set; }
        public Nullable<int> Pennies { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string Height { get; set; }
        public Nullable<int> Upkeep { get; set; }
        public int TotalAdvancesTaken { get; set; }
        public bool HasAttributeMoveAvailable { get; set; }
    
        public virtual ICollection<Award> Awards { get; set; }
        public virtual ICollection<Comment> PersonComments { get; set; }
        public virtual Player Player { get; set; }
        public virtual ICollection<SessionLog> SessionLogs { get; set; }
        public virtual ICollection<PersonStat> PersonStats { get; set; }
        public virtual ICollection<FateAspect> FateAspects { get; set; }
        public virtual ICollection<FateStat> FateStats { get; set; }
        public virtual ICollection<FateStunt> FateStunts { get; set; }
        public virtual ICollection<SimpleHitPoint> SimpleHitPoints { get; set; }
        public virtual ICollection<Reputation> Reputations { get; set; }
        public virtual ICollection<Asset> Assets { get; set; }
        public virtual ICollection<ScoreHistory> ScoreHistories { get; set; }
        public virtual ICollection<PersonAttribute> PersonAttributes { get; set; }
    }
}
