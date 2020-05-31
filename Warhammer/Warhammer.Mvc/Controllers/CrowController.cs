using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class CrowController : BaseController
    {
        private readonly IViewModelFactory _factory;
        private readonly ICharacterAttributeManager _manager;


        public CrowController(IAuthenticatedDataProvider data, IViewModelFactory factory, ICharacterAttributeManager manager) : base(data)
        {
            _factory = factory;
            _manager = manager;
        }

        // GET: Crow
        public ActionResult TopStats(int id)
        {

            if (DataProvider.SiteHasFeature(Feature.CrowTopStatsPanel) && DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                CharacterAttributeModel model = _manager.GetCharacterAttributes(id);
                return PartialView(model);
            }
            return null;

        }

    }
}