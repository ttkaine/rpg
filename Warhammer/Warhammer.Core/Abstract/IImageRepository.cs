using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    public interface IImageRepository
    {
        IQueryable<PageImage> PageImages();
        IQueryable<Page> Pages();
        IQueryable<PageImage> AllPageImages();
        IQueryable<Trophy> Trophies();
    }
}