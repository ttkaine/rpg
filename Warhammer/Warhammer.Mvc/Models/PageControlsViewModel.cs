﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class PageControlsViewModel
    {
        public bool PlayerIsGm { get; set; }
        public bool PlayerIsAdmin { get; set; }
        public int CurrentPlayerId { get; set; }
        public int CurrentCampaignId { get; set; }
        public bool ShowManagePeopleEnabled { get; set; }

        public bool IsLocalToCampaign => Page.CampaignId == CurrentCampaignId;

        public bool PlayerIsSessionGm
        {
            get
            {
                if (IsSession)
                {
                    Session session = Page as Session;
                    if (session?.GmId != null)
                    {
                        return session.GmId == CurrentPlayerId;
                    }
                }
                    return PlayerIsGm;
            }
        }

        public bool PlayerIsCreator => Page.CreatedById == CurrentPlayerId;

        public bool CanEdit
        {
            get
            {
                if (PlayerIsGm && IsLocalToCampaign)
                {
                    return true;
                }

                if (PlayerIsCreator && IsLocalToCampaign)
                {
                    return true;
                }

                return false;
            }
        }

        public bool CanEditPerson
        {
            get
            {
                if (!IsLocalToCampaign)
                {
                    return false;
                }
                Person person = Page as Person;
                if (person != null)
                {
                    if (person.PlayerId == CurrentPlayerId)
                    {
                        return true;
                    }

                    if (PlayerIsCreator)
                    {
                        return true;
                    }

                    if (PlayerIsGm && !person.PlayerId.HasValue)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool CanAddSessionLog => CanEditPerson;
        public bool CanKill => (CanEditPerson || PlayerIsGm) && IsLivePerson && IsLocalToCampaign;
        public bool CanResurrect => PlayerIsGm && IsDeadPerson;

        public bool IsSession => Page is Session;
        public bool IsPerson => Page is Person;

        public bool CanUseSessionControls => IsLocalToCampaign && IsSession && (PlayerIsGm || PlayerIsCreator || PlayerIsSessionGm);

        public bool IsDeadPerson
        {
            get
            {
                Person person = Page as Person;
                if (person != null)
                {
                    return person.IsDead;
                }

                return false;
            }
        }

        public bool IsLivePerson
        {
            get
            {
                Person person = Page as Person;
                if (person != null)
                {
                    return !person.IsDead;
                }

                return false;
            }
        }



        public Page Page { get; set; }

        public bool CanResetPersonAttributes
        {
            get
            {
                Person person = Page as Person;
                if (person != null)
                {
                    return CanEditPerson && person.PersonAttributes.Any();
                }

                return false;
            }
        }

        public bool CanManageAwards => PlayerIsGm && IsPerson && IsLocalToCampaign;


        public bool CanSetAsPrivate
        {
            get
            {
                Session session = Page as Session;
                if (session != null)
                {
                    return CanUseSessionControls && IsTextSession && !session.IsPrivate;
                }
                return false;
            }
        }

        public bool CanSetAsPublic
        {
            get
            {
                Session session = Page as Session;
                if (session != null)
                {
                    return CanUseSessionControls && IsTextSession && session.IsPrivate;
                }
                return false;
            }
        }

        public bool CanOpenTextSession
        {
            get
            {
                Session session = Page as Session;
                if (session != null)
                {
                    return CanUseSessionControls && IsTextSession && session.IsClosed;
                }
                return false;
            }
        }

        public bool CanCloseTextSession
        {
            get
            {
                Session session = Page as Session;
                if (session != null)
                {
                    return CanUseSessionControls && IsTextSession && !session.IsClosed;
                }
                return false;
            }
        }

        public bool IsTextSession
        {
            get
            {
                Session session = Page as Session;
                if (session != null)
                {
                    return session.IsTextSession;
                }
                return false;
            }
        }

        public bool CanSetAsTextSession => CanUseSessionControls && IsSession && !IsTextSession;
        public bool CanSetAsNotTextSession => CanUseSessionControls && IsTextSession;
        public bool CanSetSessionGm => IsTextSession && CanUseSessionControls;
        public bool GmJustSet { get; set; }
        public int? SelectedGm { get; set; }
        public SelectList Players { get; set; }
        public bool CanPin => ((PlayerIsGm && IsLocalToCampaign) || PlayerIsAdmin) && !Page.Pinned;
        public bool CanUnpin => ((PlayerIsGm && IsLocalToCampaign) || PlayerIsAdmin) && Page.Pinned;
        public bool CanDelete => (PlayerIsGm || PlayerIsAdmin) && IsLocalToCampaign;
        public bool CanApplyShift => IsCrowMk2 && (PlayerIsAdmin || PlayerIsGm) && (IsSession || IsLivePerson) && IsLocalToCampaign;
        public bool ShiftJustApplied { get; set; }
        public bool IsCrowMk2 { get; set; }
        public bool CanChangePlayer => PlayerIsAdmin && IsPerson;
        public bool CanChangeImage => IsLocalToCampaign;
        public bool CanEditLinks => IsLocalToCampaign;
        public bool ShowManagePeopleLink => IsSession && ShowManagePeopleEnabled;
    }
}