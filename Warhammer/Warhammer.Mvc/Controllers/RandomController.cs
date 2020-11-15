using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Models;
using Warhammer.Mvc.Models;

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

        public ActionResult Monster()
        {
            RandomMonsterViewModel model = new RandomMonsterViewModel();

            model.FirstAnimal = _generator.MonsterCreature().Content;
            model.SecondAnimal = _generator.MonsterCreature().Content;
            model.Feature = _generator.MonsterFeature().Content;
            model.Trait = _generator.MonsterTrait().Content;
            model.Ability = _generator.MonsterAbility().Content;
            model.Tactic = _generator.MonsterTactic().Content;
            model.Personality = _generator.MonsterPersonality().Content;
            model.Weakness = _generator.MonsterWeakness().Content;

            return View(model);
        }
    }
}