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
    }
}