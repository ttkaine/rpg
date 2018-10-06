namespace Warhammer.Core.Abstract
{
    public interface IPublicDataProvider
    {
        string GetOverrideCssContent();
        bool UserHasAccessToDomain(string username, string domain);
    }
}