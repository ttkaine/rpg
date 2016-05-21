using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
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

            if (DataProvider.SiteHasFeature(Feature.FateStats))
            {
                Person person = DataProvider.GetPerson(id);

                if (person != null)
                {
                    var aspects = FateAspectViewModels(person);

                    return PartialView(aspects);
                }
            }
            return null;
        }

        private List<FateAspectViewModel> FateAspectViewModels(Person person)
        {
            List<FateAspectViewModel> aspects = DataProvider.GetAspects(person.Id).Select(a => new FateAspectViewModel
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
                            PersonId = person.Id,
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
            if (model != null)
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
                            Person person = DataProvider.GetPerson(id);
                            if (person != null)
                            {
                                var aspects = FateAspectViewModels(person);
                                return PartialView(aspects);
                            }
                        }
                    }
                }
            }
            return null;
        }

        public ActionResult Stats(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.FateStats))
            {
                Person person = DataProvider.GetPerson(id);
                if (person != null)
                {
                    List<FateStatViewModel> model = GetFateStatsViewModel(person);
                    return PartialView(model);
                }
            }
            return null;
        }
        [HttpPost]
        public ActionResult Stats(List<FateStatViewModel> model)
        {
            if (model != null && model.Any())
            {
                if (DataProvider.SiteHasFeature(Feature.FateStats))
                {
                    FateStatViewModel fateStatViewModel = model.FirstOrDefault();
                    if (fateStatViewModel != null)
                    {
                        DataProvider.SaveFateStats(model.Select(s => s.Stat));

                        int id = fateStatViewModel.Stat.PersonId;
                        Person person = DataProvider.GetPerson(id);
                        if (person != null)
                        {
                            List<FateStatViewModel> stats = GetFateStatsViewModel(person);
                            ModelState.Clear();
                            return PartialView(stats);
                        }
                    }
                }
            }

            return null;
        }


        private List<FateStatViewModel> GetFateStatsViewModel(Person person)
        {
            List<int> values = StatLevel.Average.Numbers();

            List<FateStat> stats = DataProvider.GetFateStats(person.Id);

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
                            PersonId = person.Id,
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
                Person person = DataProvider.GetPerson(id);
                if (person != null)
                {
                    var viewModel = FateStuntsViewModel(person);

                    return PartialView(viewModel);
                }
            }
            return null;
        }

        private FateStuntsViewModel FateStuntsViewModel(Person person)
        {
            List<FateStunt> stunts = DataProvider.GetStunts(person.Id);
            List<FateStuntViewModel> models = stunts.Select(s => new FateStuntViewModel
            {
                Stunt = s,
                ShowHidden = CurrentPlayer.IsGm || CurrentPlayer.Id == person.PlayerId
            }).ToList();
            FateStuntsViewModel viewModel = new FateStuntsViewModel
            {
                StuntModels = models,
                PersonId = person.Id,
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
                Person person = DataProvider.GetPerson(stunt.PersonId);
                if (person != null)
                {
                    var viewModel = FateStuntsViewModel(person);
                    ModelState.Clear();
                    return PartialView("Stunts", viewModel);
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult EditStunt(FateStunt model)
        {
            if (ModelState.IsValid)
            {
                DataProvider.SaveStunt(model);
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    var viewModel = FateStuntsViewModel(person);
                    ModelState.Clear();
                    return PartialView("Stunts", viewModel);
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult DeleteStunt(int stuntId, int personId)
        {
            DataProvider.DeleteStunt(stuntId);
            Person person = DataProvider.GetPerson(personId);
            if (person != null)
            {
                var viewModel = FateStuntsViewModel(person);
                ModelState.Clear();
                return PartialView("Stunts", viewModel);
            }
            return null;
        }


        [HttpPost]
        public ActionResult ToggleStuntVisibility(int stuntId, int personId)
        {
            DataProvider.ToggleStuntVisibility(stuntId);
            Person person = DataProvider.GetPerson(personId);
            if (person != null)
            {
                var viewModel = FateStuntsViewModel(person);
                ModelState.Clear();
                return PartialView("Stunts", viewModel);
            }
            return null;
        }
    }
}