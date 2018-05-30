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
    }
}