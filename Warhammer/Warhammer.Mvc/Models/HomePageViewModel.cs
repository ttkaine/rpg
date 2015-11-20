using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class HomePageViewModel
    {
        public HomePageViewModel()
        {
            RecentChanges = new List<Page>();
            MyStuff = new List<Page>();
            MyPeople = new List<Person>();
            AllPeople = new List<Person>();
        }

       public List<Page> RecentChanges { get; set; }
       public List<Page> MyStuff { get; set; }
       public List<Person> MyPeople { get; set; }
       public List<Person> AllPeople { get; set; }
       public IOrderedEnumerable<Page> NewPages { get; set; }
       public IOrderedEnumerable<Page> UpdatedPages { get; set; }
       public List<Person> TopNpcs { get; set; }
       public List<Session> UpdatedTextSessions { get; set; }
    }
}