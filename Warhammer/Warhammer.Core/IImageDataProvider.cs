using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Core
{
    public interface IImageDataProvider
    {
        PageImage GetPageImageForPage(int id, bool includeAll = false);
        string GetPageName(int id);
        TrophyImage GetPageImageForTrophy(int id);
        string GetImagefilenameForPage(int id);
    }
}