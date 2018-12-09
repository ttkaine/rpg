using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class ImageRepository : IImageRepository
    {
        readonly WarhammerDataEntities _entities = new WarhammerDataEntities();
        public IQueryable<PageImage> PageImages()
        {
            return _entities.PageImages;
        }

        public IQueryable<Page> Pages()
        {
            return _entities.Pages;
        }
    }
}