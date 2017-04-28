using System;
using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class SiteFeatureProvider : ISiteFeatureProvider
    {
        private readonly IRepository _repository;
        private List<Feature> _enabledFeatures = null;

        public SiteFeatureProvider(IRepository repository)
        {
            _repository = repository;
        }

        public bool SiteHasFeature(Feature feature)
        {
            if (_enabledFeatures == null)
            {
                List<string> names = _repository.SiteFeatures().Where(e => e.IsEnabled).Select(e => e.Name).ToList();
                _enabledFeatures = new List<Feature>();
                foreach (string name in names)
                {
                    Feature f;
                    if (Enum.TryParse(name, true, out f))
                    {
                        _enabledFeatures.Add(f);
                    }
                }
            }
            return _enabledFeatures.Contains(feature);
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
                }
            }
        }
    }
}