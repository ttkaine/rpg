using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Extensions;
using Warhammer.Core.Models;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    [Authorize(Roles = "Player")]
    public class CreateController : BaseController
    {
        // GET: Create
        public CreateController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EditMode(string returnUrl)
        {
            Session["EditMode"] = true;
            return Redirect(returnUrl);
        }

        public ActionResult ReadOnlyMode(string returnUrl)
        {
            Session["EditMode"] = false;
            return Redirect(returnUrl);
        }

        public ActionResult Person()
        {
            Person person = new Person();
            return View(person);
        }

        [HttpPost]
        public ActionResult Page(Page page)
        {
            if (ModelState.IsValid)
            {
                int personId = DataProvider.AddPage(page.ShortName, page.FullName, page.Description);
                return RedirectToAction("Index", "Page", new {id = personId});
            }
            return View(page);
        }

        public ActionResult Page()
        {
            Page page = new Page();
            return View(page);
        }

        [HttpPost]
        public ActionResult Person(Person person)
        {
            if (ModelState.IsValid)
            {
                int pageId = DataProvider.AddPerson(person.ShortName, person.FullName, person.Description, person.CreateAsNpc, person.Gender);
                return RedirectToAction("Index", "Page", new { id = pageId });
            }
            return View(person);
        }

        public ActionResult Creature()
        {
            CreateCreatureViewModel model = new CreateCreatureViewModel();
            model.ParentOptions = new SelectList(DataProvider.Creatures().OrderBy(l => l.Breadcrumb), "Id", "Breadcrumb");
            return View(model);
        }

        [HttpPost]
        public ActionResult Creature(CreateCreatureViewModel creatureModel)
        {
            if (ModelState.IsValid)
            {
                int pageId = DataProvider.AddCreature(creatureModel);
                return RedirectToAction("Index", "Page", new { id = pageId });
            }
            return View(creatureModel);
        }

        public ActionResult Organisation()
        {
            CreateOrganisationViewModel model = new CreateOrganisationViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Organisation(CreateOrganisationViewModel organisationModel)
        {
            if (ModelState.IsValid)
            {
                int pageId = DataProvider.AddOrganisation(organisationModel.Name, organisationModel.Description);
                return RedirectToAction("Index", "Page", new { id = pageId });
            }
            return View(organisationModel);
        }


        public ActionResult GameSession()
        {
            List<Arc> arcs = DataProvider.Arcs().OrderByDescending(a => a.CurrentGameDate.Year).ThenByDescending(a => a.CurrentGameDate.Month).ThenByDescending(a => a.CurrentGameDate.Day).ToList();
            List<PageToggleModel> suggestedPageLinks = DataProvider.GetSuggestedPageLinksForNewSession();
            CreateSessionViewModel model = new CreateSessionViewModel
            {
                Session = new Session(),
                LinkPages = suggestedPageLinks
            };

            arcs.Insert(0, new Arc() { Id = 0, ShortName = "None", FullName = "None" });
            model.Arcs = new SelectList(arcs, "Id", "FullName");
            model.SelectedArcId = 0;

            return View(model);
        }

        [HttpPost]
        public ActionResult GameSession(CreateSessionViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!model.Session.DateTime.HasValue)
                {
                    model.Session.DateTime = DateTime.UtcNow;
                }
                GameDate gameDate = model.GameDate?.ToWarhammerGameDate();
                Arc arc = DataProvider.GetArc(model.SelectedArcId);
                if (gameDate == null)
                {
                    if (arc?.CurrentGameDate != null)
                    {
                        gameDate = new GameDate() { Year = arc.CurrentGameDate.Year, Month = arc.CurrentGameDate.Month, Day = arc.CurrentGameDate.Day, Comment = "Set from Arc Date"};
                    }
                    else
                    {
                        CampaignDetail campaignDetail = DataProvider.GetCampaginDetails();
                        DateTime date = campaignDetail.CurrentGameDate ?? DateTime.UtcNow;
                        gameDate = new GameDate() {Year = date.Year, Month = date.Month, Day = date.Day, Comment = "Set from Campaign Date"};
                    }
                }
                
                int sessionId = DataProvider.AddSession(model.Session.FullName, model.Session.ShortName, model.Session.Description, model.Session.DateTime.Value, model.Session.CreateWithPreviousCharacterList, model.LinkPages, gameDate, arc?.Id);
                return RedirectToAction("Index", "Page", new { id = sessionId });
            }

            return View(model);
        }

        public ActionResult SessionLog(int? personid, int? sessionId)
        {
            int playerId = DataProvider.CurrentPlayerId;
            CreateSessionLogViewModel model = new CreateSessionLogViewModel
            {
                Person = new SelectList(DataProvider.People().OrderBy(p => p.IsDead).ThenByDescending(p => p.PlayerId == playerId).ThenBy(p => p.ShortName), "Id", "ShortName"),
                Session = new SelectList(DataProvider.AllSessions().OrderByDescending(s => s.DateTime), "Id", "ShortName")
                
            };
            SessionLog sessionLog = new SessionLog();
            if (personid.HasValue)
            {
                sessionLog.Person = DataProvider.GetPage(personid.Value) as Person;

                if (sessionLog.Person != null)
                {
                    model.SelectedPersonId = sessionLog.Person.Id;
                }
            }

            if (sessionId.HasValue)
            {
                sessionLog.Session = DataProvider.GetPage(sessionId.Value) as Session;

                if (sessionLog.Session != null)
                {
                    model.SelectedSessionId = sessionLog.Session.Id;
                }
            }

            string defaultName = string.Empty;
            if (sessionLog.Person != null && sessionLog.Session != null)
            {
                defaultName = string.Format("{0} Log for {1}", sessionLog.Person.ShortName, sessionLog.Session.ShortName);
            }

            if (sessionLog.Person != null && sessionLog.Session == null)
            {
                defaultName = string.Format("{0} Log for Session {1}", sessionLog.Person.ShortName, "???");
            }
            sessionLog.ShortName = defaultName;

            model.Log = sessionLog;
            return View(model);
        }

        [HttpPost]
        public ActionResult SessionLog(CreateSessionLogViewModel model)
        {
            if (ModelState.IsValid)
            {
               int logId = DataProvider.AddSessionLog(model.SelectedSessionId, model.SelectedPersonId, model.Log.ShortName, model.Log.FullName,model.Log.Description);
               return RedirectToAction("Index", "Page", new { id = logId });
            }

            model.Person = new SelectList(DataProvider.People(), "Id", "ShortName");
            model.Session = new SelectList(DataProvider.AllSessions(), "Id", "ShortName");
            return View(model);
        }

        public ActionResult Place()
        {
            CreatePlaceViewModel model = new CreatePlaceViewModel();
            model.Place = new Place();
            model.ParentPlace = new SelectList(DataProvider.Places().OrderBy(l => l.Breadcrumb), "Id", "Breadcrumb");
            return View(model);
        }

        [HttpPost]
        public ActionResult Place(CreatePlaceViewModel model)
        {
            if (ModelState.IsValid)
            {
                int placeId = DataProvider.AddPlace(model.Place.ShortName, model.Place.ShortName, model.Place.Description, model.ParentId);
                return RedirectToAction("Index", "Page", new { id = placeId });
            }

            model.ParentPlace = new SelectList(DataProvider.Places().OrderBy(l => l.Breadcrumb), "Id", "Breadcrumb");
            return View(model);
        }

        public ActionResult Arc()
        {
            CreateArcViewModel model = new CreateArcViewModel();
            model.Arc = new Arc();
            
            return View(model);
        }

        [HttpPost]
        public ActionResult Arc(CreateArcViewModel model)
        {
            if (ModelState.IsValid)
            {
                GameDate startDate = model.StartDate?.ToWarhammerGameDate();
                if (startDate == null)
                {
                    CampaignDetail campaignDetail = DataProvider.GetCampaginDetails();
                    DateTime date = campaignDetail.CurrentGameDate ?? DateTime.UtcNow;
                    startDate = new GameDate() {Year = date.Year, Month = date.Month, Day = date.Day, Comment = "Set from Campaign Date"};
                }

                int arcId = DataProvider.AddArc(model.Arc.ShortName, model.Arc.FullName, model.Arc.Description, startDate);
                return RedirectToAction("Index", "Page", new { id = arcId });
            }

            return View(model);
        }
    }
}