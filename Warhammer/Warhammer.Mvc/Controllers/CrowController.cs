using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Core.Models.Crow;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class CrowController : BaseController
    {
        private readonly IViewModelFactory _factory;
        private readonly ICrowCharacterManager _characterManager;

        public CrowController(IAuthenticatedDataProvider data, IViewModelFactory factory, ICrowCharacterManager characterManager) : base(data)
        {
            _factory = factory;
            _characterManager = characterManager;
        }

        // GET: Crow
        public ActionResult TopStats(int id)
        { 

            if (DataProvider.SiteHasFeature(Feature.CrowTopStatsPanel) && DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                CrowCharacterAttributesModel model = _characterManager.GetCharacterAttributes(id);
                return PartialView(model);
            }
            return null;

        }

        public ActionResult ManageDefaultAttributes()
        {
            if (DataProvider.CurrentPlayerIsGm || DataProvider.CurrentUserIsAdmin)
            {
                if (DataProvider.SiteHasFeature(Feature.PersonAttributes))
                {
                    ManageDefaultPersonAttributesViewModel model = new ManageDefaultPersonAttributesViewModel();
                    model.Attributes = _characterManager.GetDefaultPersonAttributes();
                    return View(model);
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult ManageDefaultAttributes(ManageDefaultPersonAttributesViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (DataProvider.CurrentPlayerIsGm || DataProvider.CurrentUserIsAdmin)
                {
                    if (model.NewAttributeIsPopulated && model.NewAttributeType.HasValue)
                    {
                        DataProvider.AddDefaultPersonAttribute(model.NewAttributeType.Value, model.NewAttributeName, model.NewAttributeDescription, model.NewAttributeValue, model.NewAttributeIsPrivate, model.NewAttributeIsNpc);
                    }
                    if (model.Attributes != null)
                    {
                        foreach (DefaultPersonAttribute attribute in model.Attributes)
                        {
                            if (attribute.Remove)
                            {
                                DataProvider.RemoveDefaultPersonAttribute(attribute.Id);
                            }
                            else
                            {
                                DataProvider.UpdateDefaultPersonAttribute(attribute.Id, attribute.InitialValue, attribute.Name, attribute.Description, attribute.AttributeType, attribute.IsPrivate, attribute.IncludeForNpc);
                            }

                        }
                    }
                    return RedirectToAction("ManageDefaultAttributes", "Crow");
                }
                else
                {
                    return null;
                }
            }
            return RedirectToAction("ManageDefaultAttributes", "Crow");
        }


        public ActionResult CharacterGeneration(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.CharacterGeneration) && DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                CrowCharacterGenerationModel model = _characterManager.GetCharacterGenerationModel(id);
                return View(model);
            }
            return null;
        }

        [HttpPost]
        public ActionResult InitializeCharacter(int id)
        {
            _characterManager.InitializeCharacter(id);
            return RedirectToAction("CharacterGeneration", new { id});
        }

        [HttpPost]
        public ActionResult CharacterGeneration(CrowCharacterGenerationModel model)
        {
            if (model.AddNewTerm)
            {
                _characterManager.AddTerm(model.NextTerm);
            }



            return RedirectToAction("CharacterGeneration", new { model.Person.Id });
        }

    }
}