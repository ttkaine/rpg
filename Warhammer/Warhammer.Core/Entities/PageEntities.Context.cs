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
        public virtual DbSet<SiteFeature> SiteFeatures { get; set; }
        public virtual DbSet<PersonStat> PersonStats { get; set; }
        public virtual DbSet<UserSetting> UserSettings { get; set; }
        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<FateAspect> FateAspects { get; set; }
        public virtual DbSet<FateStat> FateStats { get; set; }
        public virtual DbSet<FateStunt> FateStunts { get; set; }
        public virtual DbSet<SimpleHitPoint> SimpleHitPoints { get; set; }
        public virtual DbSet<AdminSetting> AdminSettings { get; set; }
        public virtual DbSet<ExceptionLog> ExceptionLogs { get; set; }
        public virtual DbSet<PageImage> PageImages { get; set; }
        public virtual DbSet<CreatureAbility> CreatureAbilities { get; set; }
        public virtual DbSet<Reputation> Reputations { get; set; }
        public virtual DbSet<PriceListItem> PriceListItems { get; set; }
        public virtual DbSet<CampaignDetail> CampaignDetails { get; set; }
        public virtual DbSet<Rumour> Rumours { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<PlayerCampaign> PlayerCampaigns { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<ScoreHistory> ScoreHistories { get; set; }
        public virtual DbSet<PostOrder> PostOrders { get; set; }
    }
}
