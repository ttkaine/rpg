using System.Data.Entity;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class BackgroundRepo : IBackgroundRepo
    {
        private readonly WarhammerDataEntities _entities = new WarhammerDataEntities();

        public IQueryable<Page> Pages()
        {
            return _entities.Pages;
        }
        public IQueryable<Award> Awards()
        {
            return _entities.Awards;
        }

        public int InternalSave(Page page)
        {
            if (page.Id == 0)
            {
                _entities.Pages.Add(page);
            }
            else
            {
                _entities.Entry(page).State = EntityState.Modified;
            }
            _entities.SaveChanges();

            return page.Id;
        }

        public int Save(Person person)
        {
            return InternalSave(person);
        }
    }
}