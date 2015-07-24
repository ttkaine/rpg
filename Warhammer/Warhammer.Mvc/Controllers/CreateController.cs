using System;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
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
                if (DataProvider.PageExists(page.ShortName))
                {
                    Page existingPage = DataProvider.GetPage(page.ShortName);
                    return RedirectToAction("Index", "Page", new { id = existingPage.Id });
                }
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
        public ActionResult Person(Page page)
        {
            if (ModelState.IsValid)
            {
                if (DataProvider.PageExists(page.ShortName))
                {
                    Page existingPage = DataProvider.GetPage(page.ShortName);
                    return RedirectToAction("Index", "Page", new { id = existingPage.Id });
                }
                int pageId = DataProvider.AddPerson(page.ShortName, page.FullName, page.Description);
                return RedirectToAction("Index", "Page", new { id = pageId });
            }
            return View(page as Person);
        }

        public ActionResult GameSession()
        {
            Session session = new Session();
            return View(session);
        }

        [HttpPost]
        public ActionResult GameSession(Session session)
        {
            if (ModelState.IsValid)
            {
                if (DataProvider.PageExists(session.ShortName))
                {
                    Page existingPage = DataProvider.GetPage(session.ShortName);
                    return RedirectToAction("Index", "Page", new { id = existingPage.Id });
                }

                if (!session.DateTime.HasValue)
                {
                    session.DateTime = DateTime.Now;
                }
                int sessionId = DataProvider.AddSession(session.FullName, session.ShortName, session.Description,
                    session.DateTime.Value);
                return RedirectToAction("Index", "Page", new { id = sessionId });
            }


            return View(session);
        }

        public ActionResult SessionLog(int? personid, int? sessionId)
        {
            CreateSessionLogViewModel model = new CreateSessionLogViewModel
            {
                Person = new SelectList(DataProvider.People(), "Id", "ShortName"),
                Session = new SelectList(DataProvider.Sessions(), "Id", "ShortName")
                
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
                if (DataProvider.PageExists(model.Log.ShortName))
                {
                    Page existingPage = DataProvider.GetPage(model.Log.ShortName);
                    return RedirectToAction("Index", "Page", new { id = existingPage.Id }); 
                }

               int logId = DataProvider.AddSessionLog(model.SelectedSessionId, model.SelectedPersonId, model.Log.ShortName, model.Log.FullName,model.Log.Description);
               return RedirectToAction("Index", "Page", new { id = logId });
            }


            model.Person = new SelectList(DataProvider.People(), "Id", "ShortName");
            model.Session = new SelectList(DataProvider.Sessions(), "Id", "ShortName");
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
                if (DataProvider.PageExists(model.Place.ShortName))
                {
                    Page existingPage = DataProvider.GetPage(model.Place.ShortName);
                    return RedirectToAction("Index", "Page", new { id = existingPage.Id });
                }
                int placeId = DataProvider.AddPlace(model.Place.ShortName, model.Place.ShortName, model.Place.Description, model.ParentId);
                return RedirectToAction("Index", "Page", new { id = placeId });
            }

            model.ParentPlace = new SelectList(DataProvider.Places().OrderBy(l => l.Breadcrumb), "Id", "Breadcrumb");
            return View(model);
        }
    }
}