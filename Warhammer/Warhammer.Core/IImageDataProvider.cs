using Warhammer.Core.Entities;

namespace Warhammer.Core
{
    public interface IImageDataProvider
    {
        PageImage GetPageImageForPage(int id, bool includeAll = false);
        string GetPageName(int id);
    }
}