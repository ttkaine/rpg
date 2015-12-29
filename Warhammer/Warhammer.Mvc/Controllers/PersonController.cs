using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class PersonController : BaseController
    {
        readonly IViewModelFactory _factory;

        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(int personId, string fullName, string shortName, string description, string saveAction)
        {
            if (saveAction == "Save")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");              
            }
        }

        public ActionResult ViewStats(int personId)
        {
            if (!DataProvider.SiteHasFeature("SimpleStats"))
            {
                return null;
            }


            
            Person person = DataProvider.GetPerson(personId);
            PersonStatViewModel model = _factory.MakeStatModel(person);
            ModelState.Remove("Posted");
            ModelState.Clear();
            model.Posted = false;
            return PartialView(model);
        }

        //public ActionResult EditStats(int personId)
        //{

        //}

        //public ActionResult EditStats(int personId)
        //{

        //}
        public PersonController(IAuthenticatedDataProvider data, IViewModelFactory factory) : base(data)
        {
            _factory = factory;
        }

        [HttpPost]
        public ActionResult SetStats(PersonStatViewModel postedStats)
        {
            if (!DataProvider.SiteHasFeature("SimpleStats"))
            {
                return null;
            }
            if (postedStats != null)
            {
                if (postedStats.Stats != null && postedStats.Stats.Sum(s => s.Value) != 18)
                {
                    ModelState.AddModelError("Stats", "Stats must add up to 18 points");
                }

                if (ModelState.IsValid)
                {
                    string descriptors =
                        _factory.Combine(new List<string>
                        {
                            postedStats.AddedDescriptor1,
                            postedStats.AddedDescriptor2,
                            postedStats.AddedDescriptor3
                        });
                    DataProvider.SetStats(postedStats.PersonId, postedStats.Stats, postedStats.AddedRole, descriptors);

                    Person person = DataProvider.GetPerson(postedStats.PersonId);
                    PersonStatViewModel model = _factory.MakeStatModel(person);
                    ModelState.Clear();
                    ModelState.Remove("Posted");
                    return PartialView("ViewStats", model);
                }

            }
            return PartialView("ViewStats", postedStats);
        }
    }
}