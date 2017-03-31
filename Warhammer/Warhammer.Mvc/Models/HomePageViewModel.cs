using System;
using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class HomePageViewModel
    {
        public HomePageViewModel()
        {
            RecentChanges = new List<PageLinkModel>();
            MyStuff = new List<Page>();
            MyPeople = new List<Person>();
            AllPeople = new List<Person>();
            OtherPeople = new List<PageLinkModel>();
            TopNpcs = new List<PageLinkModel>();
        }

       public string GameDateDisplay { get; set; }
       public List<PageLinkModel> RecentChanges { get; set; }
       public List<Page> MyStuff { get; set; }
       public List<Person> MyPeople { get; set; }
       public List<Person> AllPeople { get; set; }
       public ICollection<PageLinkModel> NewPages { get; set; }
       public ICollection<PageLinkModel> UpdatedPages { get; set; }
       public List<PageLinkModel> TopNpcs { get; set; }
       public List<Session> UpdatedTextSessions { get; set; }
       public List<Session> MyTurnTextSessions { get; set; }
       public string SiteName { get; set; }
       public List<PageLinkModel> OtherPeople { get; set; }
        public DateTime? GameDate { get; set; }
    }
}