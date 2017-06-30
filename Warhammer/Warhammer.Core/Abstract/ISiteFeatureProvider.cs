using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    public interface ISiteFeatureProvider
    {
        bool SiteHasFeature(Feature featureName);
        void EnableFeature(string featureName);
        void DisableFeature(string featureName);
    }
}