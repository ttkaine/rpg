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

        readonly IAuthenticatedDataProvider _data;

        public ViewModelFactory(IAuthenticatedDataProvider data)
        {
            _data = data;
        }

        public ActiveTextSessionViewModel MakeActiveTextSessionViewModel()
        {
            ActiveTextSessionViewModel model = new ActiveTextSessionViewModel();

            List<Session> myOpenTestSessions = _data.MyOpenTextSessions().OrderByDescending(s => s.DateTime).ToList();

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
            PersonStatViewModel model = new PersonStatViewModel {PersonId = person.Id, CharacterName = person.ShortName };

            model.Stats = person.Stats;
            model.Descriptors = person.DescriptorNames;
            model.Roles = person.RoleNames;
            model.CurrentXp = person.CurrentXp;
            model.StatCost = person.StatCost;
            model.CanBuyStat = person.CanBuyStat;
            model.RoleCost = person.RoleCost;
            model.CanBuyRole = person.CanBuyRole;
            model.DescriptorCost = person.DescriptorCost;
            model.CanBuyDescriptor = person.CanBuyDescriptor;
            model.XpSpent = person.XpSpent;
            model.MaySpendXp = !person.IsDead && model.StatsCreated;

            return model;

        }
    }
}