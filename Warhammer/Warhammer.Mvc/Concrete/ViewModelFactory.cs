using System;
using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Concrete;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Concrete
{
    public class ViewModelFactory : IViewModelFactory
    {
        private const char Seperator = '¬';
        readonly IAuthenticatedDataProvider _data;

        public ViewModelFactory(IAuthenticatedDataProvider data)
        {
            _data = data;
        }

        public ActiveTextSessionViewModel MakeActiveTextSessionViewModel()
        {
            ActiveTextSessionViewModel model = new ActiveTextSessionViewModel();

            List<Session> myOpenTestSessions = _data.MyOpenTextSessions().OrderByDescending(s => s.LastPostTime).ToList();

            foreach (Session myOpenTestSession in myOpenTestSessions)
            {
                OpenSessionViewModel sessionViewModel = new OpenSessionViewModel {Session = myOpenTestSession, Status = OpenSessionStatus.Stale };

                if (_data.ModifiedTextSessions().Contains(myOpenTestSession))
                {
                    sessionViewModel.Status = OpenSessionStatus.Updated;
                    sessionViewModel.IsUpdated = true;
                }

                if (_data.TextSessionsWhereItisMyTurn().Contains(myOpenTestSession))
                {
                    sessionViewModel.Status = OpenSessionStatus.MyTurn;
                }
                model.OpenSessions.Add(sessionViewModel);
            }

            return model;
        }

        public PersonStatViewModel MakeStatModel(Person person)
        {
            PersonStatViewModel model = new PersonStatViewModel {PersonId = person.Id};


            foreach (PersonStat personStat in person.PersonStats)
            {
                model.Stats.Add((StatName)personStat.StatId, personStat.CurrentValue);
            }

            foreach (int statId in Enum.GetValues(typeof(StatName)))
            {
                StatName stat = (StatName) statId;
                if (!model.Stats.ContainsKey(stat))
                {
                    model.Stats.Add(stat, 0);
                }
            }
            if (person.Descriptors != null)
            {
                string[] descriptors = person.Descriptors.Split(Seperator);

                foreach (string descriptor in descriptors)
                {
                    if (!string.IsNullOrWhiteSpace(descriptor) && !model.Descriptors.Contains(descriptor))
                    {
                        model.Descriptors.Add(descriptor);
                    }
                }
            }
            if (person.Roles != null)
            {
                string[] roles = person.Roles.Split(Seperator);

                foreach (string role in roles)
                {
                    if (!string.IsNullOrWhiteSpace(role) && !model.Roles.Contains(role))
                    {
                        model.Roles.Add(role);
                    }
                }
            }
            model.CurrentXp = person.CurrentXp;

            model.MaySpendXp = !person.IsDead && model.CurrentXp >= model.NextXpSpend && model.StatsCreated;

            return model;

        }

        public string Combine(List<string> list)
        {
            return string.Join(Seperator.ToString(), list);
        }
    }
}