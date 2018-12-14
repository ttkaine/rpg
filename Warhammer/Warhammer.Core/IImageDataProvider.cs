using Warhammer.Core.Entities;

namespace Warhammer.Core
{
    public interface IImageDataProvider
    {
        PageImage GetPageImageForPage(int id);
        string GetPageName(int id);
    }
}