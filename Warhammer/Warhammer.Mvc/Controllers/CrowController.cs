using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class CrowController : BaseController
    {
        private readonly IViewModelFactory _factory;


        public CrowController(IAuthenticatedDataProvider data, IViewModelFactory factory) : base(data)
        {
            _factory = factory;
        }

        // GET: Crow
        public ActionResult TopStats(int id)
        {

            if (DataProvider.SiteHasFeature(Feature.CrowTopStatsPanel) && DataProvider.SiteHasFeature(Feature.SimpleStats))
            {
                Person person = DataProvider.GetPerson(id);

                if (person != null)
                {
                    if (person.PlayerId.HasValue || CurrentPlayerIsGm)
                    {
                        PersonStatViewModel model = _factory.MakeStatModel(person);
                        if (model.ShowStats && model.Stats.Sum(s => s.Value) > 3)
                        {
                            return PartialView(model);
                        }
                    }
                }
            }
            return null;

        }

    }
}