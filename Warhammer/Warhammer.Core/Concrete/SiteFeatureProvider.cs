using System;
using System.Collections.Generic;
using System.Linq;
using LazyCache;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class SiteFeatureProvider : ISiteFeatureProvider
    {
        private readonly IRepository _repository;
        private readonly IAppCache _cache;
        
        private string CacheKey => $"enabled-features-for-campaign-{_repository.CurrentCampaignId}";

        public SiteFeatureProvider(IRepository repository, IAppCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public bool SiteHasFeature(Feature feature)
        {
            List<Feature> enabledFeatures = _cache.GetOrAdd(CacheKey, GetEnabledFeatures);
            return enabledFeatures.Contains(feature);
        }

        private List<Feature> GetEnabledFeatures()
        {
            List<string> names = _repository.SiteFeatures().Where(e => e.IsEnabled).Select(e => e.Name).ToList();
            List<Feature> enabledFeatures = new List<Feature>();
            foreach (string name in names)
            {
                if (Enum.TryParse(name, true, out Feature f))
                {
                    enabledFeatures.Add(f);
                }
            }

            return enabledFeatures;
        }

        public void EnableFeature(string featureName)
        {
            SiteFeature feature = _repository.SiteFeatures().FirstOrDefault(f => f.Name == featureName);

            if (feature == null)
            {
                feature = new SiteFeature { Name = featureName, Description = featureName };
            }

            if (!feature.IsEnabled)
            {
                feature.IsEnabled = true;
                _repository.Save(feature);
                _cache.Remove(CacheKey);
            }
        }

        public void DisableFeature(string featureName)
        {
            SiteFeature feature = _repository.SiteFeatures().FirstOrDefault(f => f.Name == featureName);

            if (feature != null)
            {
                if (feature.IsEnabled)
                {
                    feature.IsEnabled = false;
                    _repository.Save(feature);
                    _cache.Remove(CacheKey);
                }
            }
        }
    }
}