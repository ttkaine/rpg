using System;
using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public enum OpenSessionStatus
    {
        Stale,
        Updated,
        MyTurn
    }

    public class ActiveTextSessionViewModel
    {
        public ActiveTextSessionViewModel()
        {
            OpenSessions = new List<OpenSessionViewModel>();
        }

        public List<OpenSessionViewModel> OpenSessions { get; set; }
    }

    public class OpenSessionViewModel
    {
        public string CssClass
        {
            get
            {
                switch (Status)
                {
                    case OpenSessionStatus.Stale:
                        return "btn-default";
                    case OpenSessionStatus.Updated:
                        return "btn-default";
                    case OpenSessionStatus.MyTurn:
                        return "btn-success";
                    default:
                        return "btn-default";
                }
            }
        }

        public Session Session { get; set; }

        public bool IsUpdated { get; set; }

        public OpenSessionStatus Status { get; set; }

        public bool IsPrivate
        {
            get { return Session.IsPrivate; }
        }
    }
}