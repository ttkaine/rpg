﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class WarhammerDataEntities : DbContext
    {
        public WarhammerDataEntities()
            : base("name=WarhammerDataEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Award> Awards { get; set; }
        public virtual DbSet<ChangeLog> ChangeLogs { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Page> Pages { get; set; }
        public virtual DbSet<PageView> PageViews { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<Trophy> Trophies { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<PostOrder> PostOrders { get; set; }
        public virtual DbSet<SiteFeature> SiteFeatures { get; set; }
        public virtual DbSet<PersonStat> PersonStats { get; set; }
        public virtual DbSet<UserSetting> UserSettings { get; set; }
    }
}
