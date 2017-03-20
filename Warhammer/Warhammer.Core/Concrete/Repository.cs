﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    /// <summary>
    /// No business logic or filtering should happen in an implemntation of IRepository
    /// Well understood boiler plate code only.
    /// </summary>
    public class Repository : IRepository, IDisposable
    {
        private readonly WarhammerDataEntities _entities = new WarhammerDataEntities();
        private readonly ICurrentCampaignProvider _campaign;

        public Repository(ICurrentCampaignProvider campaign)
        {
            _campaign = campaign;
        }

        #region Accessors

        public int CurrentCampaignId
        {
            get { return _campaign.CurrentCampaignId; }
        }

        //public IQueryable<ChangeLog> ChangeLogs()
        //{
        //    return _entities.ChangeLogs;
        //}

        public IQueryable<ChangeLog> ChangeLogs()
        {
            return _entities.ChangeLogs;
        }

        public IQueryable<Person> People()
        {
            return _entities.Pages.OfType<Person>().Where(e => e.CampaignId == CurrentCampaignId);
        }

        public IQueryable<Player> Players()
        {
            return _entities.Players;
        }

        public int Save(Page page)
        {
            if (page.Id == 0)
            {
                _entities.Pages.Add(page);
            }
            else
            {
                _entities.Entry(page).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return page.Id;
        }

        public IQueryable<Page> Pages()
        {
            return _entities.Pages.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public void Delete(Page page)
        {
            _entities.Pages.Remove(page);
            _entities.SaveChanges();
        }

        public IQueryable<Trophy> Trophies()
        {
            return _entities.Trophies;
        }

        public int Save(Trophy trophy)
        {
            if (trophy.Id == 0)
            {
                _entities.Trophies.Add(trophy);
            }
            else
            {
                _entities.Entry(trophy).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return trophy.Id;
        }

        public void Delete(Award award)
        {
            _entities.Awards.Remove(award);
            _entities.SaveChanges();
        }

        public IQueryable<Award> Awards()
        {
            return _entities.Awards.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public IQueryable<PageView> PageViews()
        {
            return _entities.PageViews.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public void Delete(PageView pageView)
        {
            _entities.PageViews.Remove(pageView);
            _entities.SaveChanges();
        }

        public int Save(Post post)
        {
            if (post.Id == 0)
            {
                _entities.Posts.Add(post);
            }
            else
            {
                _entities.Entry(post).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return post.Id;
        }

        public IQueryable<Post> Posts()
        {
            return _entities.Posts.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public void Delete(Post post)
        {
            _entities.Posts.Remove(post);
            _entities.SaveChanges();
        }

        public IQueryable<Comment> Comments()
        {
            return _entities.Comments.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public void Delete(Comment comment)
        {
            _entities.Comments.Remove(comment);
            _entities.SaveChanges();
        }

        public IQueryable<SiteFeature> SiteFeatures()
        {
            return _entities.SiteFeatures.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public int Save(SiteFeature feature)
        {
            if (feature.Id == 0)
            {
                _entities.SiteFeatures.Add(feature);
            }
            else
            {
                _entities.Entry(feature).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return feature.Id;
        }

        public IQueryable<UserSetting> UserSettings()
        {
            return _entities.UserSettings.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public IQueryable<Setting> Settings()
        {
            return _entities.Settings;
        }

        public int Save(UserSetting setting)
        {
            if (setting.Id == 0)
            {
                _entities.UserSettings.Add(setting);
            }
            else
            {
                _entities.Entry(setting).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return setting.Id;
        }

        public int Save(Comment comment)
        {
            if (comment.Id == 0)
            {
                _entities.Comments.Add(comment);
            }
            else
            {
                _entities.Entry(comment).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return comment.Id;
        }

        public IQueryable<ScoreHistory> ScoreHistories()
        {
            return _entities.ScoreHistories.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public int Save(ScoreHistory scoreHistory)
        {
            if (scoreHistory.Id == 0)
            {
                _entities.ScoreHistories.Add(scoreHistory);
            }
            else
            {
                _entities.Entry(scoreHistory).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return scoreHistory.Id;
        }

        public IQueryable<Person> PeopleForScoring()
        {
            return _entities.Pages.OfType<Person>()
                .Include("Related")
                .Include("Awards")
                .Include("Awards.Trophy")
                .Include("PersonStats")
                .Include("FateAspects")
                .Include("FateStats")
                .Include("FateStunts").Where(e => e.CampaignId == CurrentCampaignId)

                .AsNoTracking();
        }

        public IQueryable<PageImage> PageImages()
        {
            return _entities.PageImages.Where(e => e.CampaignId == CurrentCampaignId);
        }

        #endregion

        #region Save

        //public int Save(Example example)
        //{
        //    if (example.Id == 0)
        //    {
        //        _entities.Examples.Add(example);
        //    }
        //    else
        //    {
        //        _entities.Entry(example).State = EntityState.Modified;       
        //    } 
        //    _entities.SaveChanges();

        //    return example.Id;
        //}

        #endregion

        #region Bulk Innsert

        public void BulkInsert<T>(IList<T> list)
        {
            string connection = _entities.Database.Connection.ConnectionString;
            string tableName = typeof(T).Name;
            BulkInsert<T>(connection, tableName, list);
        }

        public IQueryable<FateAspect> FateAspects()
        {
            return _entities.FateAspects.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public int Save(FateAspect fateAspect)
        {

            if (fateAspect.Id == 0)
            {
                _entities.FateAspects.Add(fateAspect);
            }
            else
            {
                _entities.Entry(fateAspect).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return fateAspect.Id;

        }

        public void Delete(FateAspect fateAspect)
        {
            _entities.FateAspects.Remove(fateAspect);
            _entities.SaveChanges();
        }

        public IQueryable<FateStat> FateStats()
        {
            return _entities.FateStats.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public int Save(FateStat fateStat)
        {

            if (fateStat.Id == 0)
            {
                _entities.FateStats.Add(fateStat);
            }
            else
            {
                _entities.Entry(fateStat).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return fateStat.Id;
        }

        public IQueryable<FateStunt> FateStunts()
        {
            return _entities.FateStunts.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public int Save(FateStunt fateStunt)
        {

            if (fateStunt.Id == 0)
            {
                _entities.FateStunts.Add(fateStunt);
            }
            else
            {
                _entities.Entry(fateStunt).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return fateStunt.Id;
        }

        public void Delete(FateStunt fateStunt)
        {
            _entities.FateStunts.Remove(fateStunt);
            _entities.SaveChanges();
        }

        public IQueryable<AdminSetting> AdminSettings()
        {
            return _entities.AdminSettings.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public int Save(AdminSetting setting)
        {
            if (setting.Id == 0)
            {
                _entities.AdminSettings.Add(setting);
            }
            else
            {
                _entities.Entry(setting).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return setting.Id;
        }

        public int Save(Player player)
        {
            if (player.Id == 0)
            {
                _entities.Players.Add(player);
            }
            else
            {
                _entities.Entry(player).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return player.Id;
        }

        public int Save(ExceptionLog exceptionLog)
        {
            if (exceptionLog.Id == 0)
            {
                _entities.ExceptionLogs.Add(exceptionLog);
            }
            else
            {
                _entities.Entry(exceptionLog).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return exceptionLog.Id;
        }

        public IQueryable<ExceptionLog> ExceptionLogs()
        {
            return _entities.ExceptionLogs;
        }

        public void Delete(PersonStat personStat)
        {
            _entities.PersonStats.Remove(personStat);
            _entities.SaveChanges();
        }

        public int Save(PageView pageView)
        {
            if (pageView.Id == 0)
            {
                _entities.PageViews.Add(pageView);
            }
            else
            {
                _entities.Entry(pageView).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return pageView.Id;
        }

        public void Delete(ScoreHistory scoreHistory)
        {
            _entities.ScoreHistories.Remove(scoreHistory);
            _entities.SaveChanges();
        }

        public int Save(PageImage pageImage)
        {
            if (pageImage.Id == 0)
            {
                _entities.PageImages.Add(pageImage);
            }
            else
            {
                _entities.Entry(pageImage).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return pageImage.Id;
        }

        public void Delete(PageImage pageImage)
        {
            _entities.PageImages.Remove(pageImage);
            _entities.SaveChanges();
        }

        public IQueryable<PriceListItem> PriceListItems()
        {
            return _entities.PriceListItems.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public int Save(PriceListItem priceListItem)
        {
            if (priceListItem.Id == 0)
            {
                _entities.PriceListItems.Add(priceListItem);
            }
            else
            {
                _entities.Entry(priceListItem).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return priceListItem.Id;
        }

        public IQueryable<CampaignDetail> CampaignDetails()
        {
            return _entities.CampaignDetails.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public IQueryable<Rumour> Rumours()
        {
            return _entities.Rumours.Where(e => e.CampaignId == CurrentCampaignId);
        }

        public int Save(Rumour rumour)
        {
            if (rumour.Id == 0)
            {
                _entities.Rumours.Add(rumour);
            }
            else
            {
                _entities.Entry(rumour).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return rumour.Id;
        }

        public void Delete(Rumour rumour)
        {
            _entities.Rumours.Remove(rumour);
            _entities.SaveChanges();
        }

        public int Save(CampaignDetail campaignDetail)
        {
            if (campaignDetail.Id == 0)
            {
                _entities.CampaignDetails.Add(campaignDetail);
            }
            else
            {
                _entities.Entry(campaignDetail).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return campaignDetail.Id;
        }

        public void Delete(Asset asset)
        {
            _entities.Assets.Remove(asset);
            _entities.SaveChanges();
        }

        public int Save(Asset asset)
        {
            if (asset.Id == 0)
            {
                _entities.Assets.Add(asset);
            }
            else
            {
                _entities.Entry(asset).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return asset.Id;
        }

        public IQueryable<Asset> Assets()
        {
            return _entities.Assets.Where(e => e.CampaignId == CurrentCampaignId);
        }

        private void BulkInsert<T>(string connection, string tableName, IList<T> list)
        {
            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.BatchSize = list.Count;
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();
                var props = TypeDescriptor.GetProperties(typeof(T))

                                           // Dirty hack to make sure we only have system data types 
                                           // i.e. filter out the relationships/collections
                                           .Cast<PropertyDescriptor>()
                                           .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                                           .ToArray();

                foreach (var propertyInfo in props)
                {
                    bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                    table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Length];
                foreach (var item in list)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = props[i].GetValue(item);
                    }

                    table.Rows.Add(values);
                }

                bulkCopy.WriteToServer(table);
            }
        }
        #endregion
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_entities != null)
                {
                    _entities.Dispose();
                }
            }
        }


    }
}
