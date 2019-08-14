using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class ImageRepository : IImageRepository
    {
        private readonly List<int> _campaignIds;
        readonly WarhammerDataEntities _entities = new WarhammerDataEntities();

        public ImageRepository(IRepository repository)
        {
            _campaignIds = repository.VisibleCampagins;
        }

        public IQueryable<PageImage> PageImages()
        {
            return _entities.PageImages.Where(p => p.Public || _campaignIds.Contains(p.CampaignId));
        }

        public IQueryable<Page> Pages()
        {      
            return _entities.Pages.Where(p => _campaignIds.Contains(p.CampaignId));
        }

        public IQueryable<PageImage> AllPageImages()
        {
            return _entities.PageImages;
        }

        public IQueryable<Trophy> Trophies()
        {
            return _entities.Trophies;
        }
    }
}