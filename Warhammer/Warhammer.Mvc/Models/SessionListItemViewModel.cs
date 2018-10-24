using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class SessionListItemViewModel
    {
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public GameDate Date { get; set; }
        public bool IsCompleted { get; set; }
        public List<Person> People { get; set; }
        public List<SessionLog> SessionLogs { get; set; }

        public Person LogButtonPerson { get; set; }

        public SessionListItemViewModel()
        {
            People = new List<Person>();
            SessionLogs = new List<SessionLog>();
        }

        public SessionListItemViewModel(Session session)
        {
            Id = session.Id;
            ShortName = session.ShortName;
            FullName = session.FullName;
            Description = session.Description;
            Date = session.GameDate;
            IsCompleted = session.IsTextSession ? session.IsClosed : session.XpAwarded != null;
            People = session.People.ToList();
            SessionLogs = session.SessionLogs.ToList();
        }
    }
}