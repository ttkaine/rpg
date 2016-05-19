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
            if(DataProvider.SiteHasFeature(Feature.FateStats))
            {
                var aspects = FateAspectViewModels(id);

                return PartialView(aspects);
            }
            return null;
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
                            PersonId = id,
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
            if (DataProvider.SiteHasFeature(Feature.FateStats))
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
            }
            return null;
        }

        public ActionResult Stats(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.FateStats))
            {
                List<FateStatViewModel> model = GetFateStatsViewModel(id);
                return PartialView(model);
            }
            return null;
        }
        [HttpPost]
        public ActionResult Stats(List<FateStatViewModel> model)
        {
            if (model.Any())
            {
                if (DataProvider.SiteHasFeature(Feature.FateStats))
                {
                    FateStatViewModel fateStatViewModel = model.FirstOrDefault();
                    if (fateStatViewModel != null)
                    {
                        DataProvider.SaveFateStats(model.Select(s => s.Stat));

                        int id = fateStatViewModel.Stat.PersonId;
                        List<FateStatViewModel> stats = GetFateStatsViewModel(id);
                        ModelState.Clear();
                        return PartialView(stats);
                    }
                }
            }

            return null;
        }


        private List<FateStatViewModel> GetFateStatsViewModel(int id)
        {
            Person person = DataProvider.GetPerson(id);

            List<int> values = StatLevel.Average.Numbers();        

            List<FateStat> stats = DataProvider.GetFateStats(id);

            List<FateStatViewModel> models = stats.Select(f => new FateStatViewModel
            {
                Stat = f,
                Options = new SelectList(values.Select(v => new { value = v, display = GetLevelDisplayName(v) }).ToList(), "value", "display"),
                ShowHidden = CurrentPlayer.IsGm || CurrentPlayer.Id == person.PlayerId,
                CanEdit = CurrentPlayer.IsGm || CurrentPlayer.Id == person.PlayerId
            }).ToList();


            List<int> allTypes = StatType.Ag.Numbers().OrderBy(i => i).ToList();

            foreach (int i in allTypes)
            {
                if (models.All(a => a.Stat.StatType != i))
                {
                    models.Add(new FateStatViewModel
                    {
                        Stat = new FateStat
                        {
                            StatType = i,
                            PersonId = id,
                            IsVisible = IsEditMode && !CurrentPlayer.IsGm
                        },
                        Options = new SelectList(values.Select(v => new { value = v, display = GetLevelDisplayName(v) }).ToList(), "value", "display"),
                        ShowHidden = CurrentPlayer.IsGm || CurrentPlayer.Id == person.PlayerId,
                        CanEdit = CurrentPlayer.IsGm || CurrentPlayer.Id == person.PlayerId
                    });
                }
            }

            return models;

        }

        private string GetLevelDisplayName(int i)
        {
                if (i >= 0)
                {
                    return string.Format($"+{i} {(StatLevel)i}");
                }
                else
                {
                    return string.Format($"{i} {(StatLevel)i}");
                }
        }

        public ActionResult Stunts(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.FateStats))
            {
                var viewModel = FateStuntsViewModel(id);

                return PartialView(viewModel);
            }
            return null;
        }

        private FateStuntsViewModel FateStuntsViewModel(int id)
        {
            Person person = DataProvider.GetPerson(id);

            List<FateStunt> stunts = DataProvider.GetStunts(id);
            List<FateStuntViewModel> models = stunts.Select(s => new FateStuntViewModel
            {
                Stunt = s,
                ShowHidden = CurrentPlayer.IsGm || CurrentPlayer.Id == person.PlayerId
            }).ToList();
            FateStuntsViewModel viewModel = new FateStuntsViewModel
            {
                StuntModels = models,
                PersonId = id,
                CanEdit = CurrentPlayer.IsGm || CurrentPlayer.Id == person.PlayerId
            };
            return viewModel;
        }

        [HttpPost]
        public ActionResult AddStunt(FateStunt stunt)
        {
            if (ModelState.IsValid)
            {
                stunt.IsVisible = !CurrentPlayer.IsGm;
                DataProvider.SaveStunt(stunt);
                var viewModel = FateStuntsViewModel(stunt.PersonId);
                ModelState.Clear();
                return PartialView("Stunts", viewModel);
            }
            return null;
        }

        [HttpPost]
        public ActionResult EditStunt(FateStunt model)
        {
            if (ModelState.IsValid)
            {
                DataProvider.SaveStunt(model);
                var viewModel = FateStuntsViewModel(model.PersonId);
                ModelState.Clear();
                return PartialView("Stunts", viewModel);
            }
            return null;
        }

        [HttpPost]
        public ActionResult DeleteStunt(int stuntId, int personId)
        {
            DataProvider.DeleteStunt(stuntId);
            var viewModel = FateStuntsViewModel(personId);
            ModelState.Clear();
            return PartialView("Stunts", viewModel);
        }


        [HttpPost]
        public ActionResult ToggleStuntVisibility(int stuntId, int personId)
        {
            DataProvider.ToggleStuntVisibility(stuntId);
            var viewModel = FateStuntsViewModel(personId);
            ModelState.Clear();
            return PartialView("Stunts", viewModel);
        }
    }
}