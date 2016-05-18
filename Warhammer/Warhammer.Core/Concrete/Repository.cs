using System;
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

        #region Accessors

        //public IQueryable<ChangeLog> ChangeLogs()
        //{
        //    return _entities.ChangeLogs;
        //}

        public IQueryable<Person> People()
        {
            return _entities.Pages.OfType<Person>();
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
            return _entities.Pages;
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
            return _entities.Awards;
        }

        public IQueryable<PageView> PageViews()
        {
            return _entities.PageViews;
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
            return _entities.Posts;
        }

        public void Delete(Post post)
        {
            _entities.Posts.Remove(post);
            _entities.SaveChanges();
        }

        public IQueryable<Comment> Comments()
        {
            return _entities.Comments;
        }

        public void Delete(Comment comment)
        {
            _entities.Comments.Remove(comment);
            _entities.SaveChanges();
        }

        public IQueryable<SiteFeature> SiteFeatures()
        {
            return _entities.SiteFeatures;
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
            return _entities.UserSettings;
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
            return _entities.ScoreHistories;
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
                .AsNoTracking();
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
            return _entities.FateAspects;
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

        public void BulkInsert<T>(string connection, string tableName, IList<T> list)
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
