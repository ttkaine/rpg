using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Controllers
{
    public class RandomController : Controller
    {
        private readonly IRandomItemGenerator _generator;

        public RandomController(IRandomItemGenerator generator)
        {
            _generator = generator;
        }

        // GET: Random
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Person()
        {
            return View();
        }

        public ActionResult Age()
        {
            RandomItemResult item = _generator.PersonAge();
            return PartialView("Item", item);
        }

        public ActionResult Sex()
        {
            RandomItemResult item = _generator.PersonSex();
            return PartialView("Item", item);
        }

        public ActionResult Orientation()
        {
            RandomItemResult item = _generator.PersonOrientation();
            return PartialView("Item", item);
        }
    }
}