using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Extensions;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class FateController : BaseController
    {
        public FateController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        public ActionResult Aspects(int id)
        {
            var aspects = FateAspectViewModels(id);

            return PartialView(aspects);
        }

        private List<FateAspectViewModel> FateAspectViewModels(int id)
        {
            Person person = DataProvider.GetPerson(id);
            List<FateAspectViewModel> aspects = DataProvider.GetAspects(id).Select(a => new FateAspectViewModel
            {
                Aspect = a,
                ShowHidden = CurrentPlayer.IsGm || CurrentPlayer.Id == person.PlayerId
            }).ToList();

            List<int> allTypes = AspectType.Concept.Numbers().OrderBy(i => i).ToList();

            foreach (int i in allTypes)
            {
                if (aspects.All(a => a.Aspect.AspectType != i))
                {
                    aspects.Add(new FateAspectViewModel
                    {
                        Aspect = new FateAspect
                        {
                            AspectType = i,
                            PersonId = id
                        },
                        ShowHidden = CurrentPlayer.IsGm || CurrentPlayer.Id == person.PlayerId
                    });
                }
            }
            return aspects;
        }

        [HttpPost]
        public ActionResult Aspects(List<FateAspectViewModel> model)
        {
            if (model.Any())
            {
                FateAspectViewModel fateAspectViewModel = model.FirstOrDefault();
                if (fateAspectViewModel != null)
                {
                    int id = fateAspectViewModel.Aspect.Id;
                    DataProvider.SaveAspects(model.Select(m => m.Aspect).ToList());
                    var aspects = FateAspectViewModels(id);
                    return PartialView(aspects);
                }

            }
            return null;
        }

    }
}