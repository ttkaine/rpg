using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;
using Warhammer.Core.Extensions;

namespace Warhammer.Mvc.Models
{
    public class ArcListItemViewModel
    {
        public Arc Arc { get; set; }
        public List<Person> People { get; set; }
        public string Duration { get; set; }
        public bool ShowDates { get; set; }

        public ArcListItemViewModel(Arc arc, bool showDates)
        {
            Arc = arc;
            People = arc.Sessions.SelectMany(s => s.People).Distinct().ToList();
            ShowDates = showDates;
            if (arc.StartGameDate != null && arc.CurrentGameDate != null && !arc.StartGameDate.IsLaterThan(arc.CurrentGameDate))
            {
                Duration = $"{arc.StartGameDate.ToShortDateString()} - {arc.CurrentGameDate.ToShortDateString()}";
            }
        }
    }
}