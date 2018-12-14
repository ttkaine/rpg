using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    public interface IBackgroundRepo
    {
        IQueryable<Page> Pages();
        IQueryable<Award> Awards();
        int Save(Person person);
        void Delete(ScoreBreakdown scoreBreakdown);
        IQueryable<PageImage> PageImages();
        IQueryable<PersonAttribute> PersonAttributes();
    }
}