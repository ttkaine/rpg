using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    /// <summary>
    /// The IRepository interface is the lowest data interface and connects directly to the data source
    /// 
    /// Any code inside implementations of IRepository are very hard to unit test and for this reason implemenatations of IRepository should be entirely bolier-plate code
    /// No business logic or filtering of data should happen in the IRepository
    ///  </summary>
    public interface IRepository
    {
        IQueryable<Person> People();
        IQueryable<Player> Players();
        int Save(Page page);
        IQueryable<Page> Pages();
        void Delete(Page page);
        IQueryable<Trophy> Trophies();
        int Save(Trophy trophy);
        void Delete(Award award);
        IQueryable<Award> Awards();
        IQueryable<PageView> PageViews();
        void Delete(PageView pageView);
		int Save(Post post);
		IQueryable<Post> Posts();
		void Delete(Post post);
        IQueryable<Comment> Comments();
        void Delete(Comment comment);
        IQueryable<SiteFeature> SiteFeatures();
        int Save(SiteFeature feature);
    }
}
