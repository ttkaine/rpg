using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class PublicDataProvider : IPublicDataProvider
    {
        private readonly IRepository _repository;

        public PublicDataProvider(IRepository repository)
        {
            _repository = repository;
        }

        public string GetOverrideCssContent()
        {
            return _repository.CampaignDetails().FirstOrDefault()?.CustomCss;
        }

        public bool UserHasAccessToDomain(string username, string domain)
        {
            int campaginId = _repository.CampaignDetails().Where(c => c.Url == domain).Select(c => c.CampaignId).FirstOrDefault();
            return  _repository.Players()
                    .Any(p => p.UserName == username && p.PlayerCampaigns.Any(c => c.CampaginId == campaginId));
        }

        public PageImage GetPageImage(int id)
        {
            return _repository.PageImages().Where(p => p.Public).FirstOrDefault(p => p.Id == id);
        }

        public SiteIcon GetSiteIcon(int size)
        {
            return _repository.SiteIcons().FirstOrDefault(s => s.Size == size);
        }

        public List<int> GetSiteIconSizes()
        {
            return _repository.SiteIcons().Select(s => s.Size).ToList();
        }
    }
}