using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Warhammer.Core;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Extensions;
using Warhammer.Core.Helpers;
using Warhammer.Core.Models;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class PersonController : BaseController
    {
        readonly IViewModelFactory _factory;
        private readonly ICharacterAttributeManager _attributeManager;

        [HttpPost, ValidateInput(false)]
        [Authorize(Roles = "Player")]
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
            if (!DataProvider.CheckStatPermissions(personId))
            {
                return ViewStatsSummary(personId);
            }

            var model = GetCleanModel(personId);
            return PartialView(model);
        }

        private ActionResult ViewStatsSummary(int personId)
        {
            if (!DataProvider.CheckStatSummaryPermissions())
            {
                return null;
            }
            var model = GetCleanModel(personId);
            return PartialView("StatSummary",model);
        }

        private PersonStatViewModel GetCleanModel(int personId)
        {
            Person person = DataProvider.GetPerson(personId);
            PersonStatViewModel model = _factory.MakeStatModel(person);
            ModelState.Remove("Posted");
            ModelState.Clear();
            model.Posted = false;
            return model;
        }

        public PersonController(IAuthenticatedDataProvider data, IViewModelFactory factory, ICharacterAttributeManager attributeManager) : base(data)
        {
            _factory = factory;
            _attributeManager = attributeManager;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult SetStats(PersonStatViewModel postedStats)
        {
            if (!DataProvider.SiteHasFeature(Feature.SimpleStats))
            {
                return null;
            }
            if (postedStats != null && postedStats.Stats != null)
            {
                Person person = DataProvider.GetPerson(postedStats.PersonId);

                if (person != null)
                {
                    if (DataProvider.SiteHasFeature(Feature.CrowStats))
                    {
                        if (person.IsNpc)
                        {
                            if (postedStats.Stats.Sum(s => s.Value) > 18)
                            {
                                ModelState.AddModelError("Stats", "Don't start with more than 18 in stats. Use XP to raise them.");
                            }
                        }
                        else
                        {
                            if (postedStats.Stats.Sum(s => s.Value) != 18)
                            {
                                ModelState.AddModelError("Stats", "Stats must add up to 18 points");
                            }
                        }

                        if (postedStats.Stats.Any(s => s.Value < 1))
                        {
                            ModelState.AddModelError("Stats", "All stats must be at least 1");
                        }
                    }

                    if (DataProvider.SiteHasFeature(Feature.FuHammerStats))
                    {
                        if (person.IsNpc)
                        {
                            if (postedStats.Stats.Sum(s => s.Value) > 8)
                            {
                                ModelState.AddModelError("Stats", "Don't start with more than 8 in stats. Use XP to raise them.");
                            }
                        }
                        else
                        {
                            //should be one at 3 
                            if (postedStats.Stats.Count(s => s.Value == 3) != 1)
                            {
                                ModelState.AddModelError("Stats", "Stats should have one value at 3");
                            }

                            //should be two at 2 
                            if (postedStats.Stats.Count(s => s.Value == 2) != 2)
                            {
                                ModelState.AddModelError("Stats", "Stats should have two values at 2");
                            }

                            //should be two at 1 
                            if (postedStats.Stats.Count(s => s.Value == 1) != 2)
                            {
                                ModelState.AddModelError("Stats", "Stats should have two values at 1");
                            }

                            //should be two at 0 
                            if (postedStats.Stats.Count(s => s.Value == 0) != 2)
                            {
                                ModelState.AddModelError("Stats", "Stats should have two values at 0");
                            }

                            //should be one at -1 
                            if (postedStats.Stats.Count(s => s.Value == -1) != 1)
                            {
                                ModelState.AddModelError("Stats", "Stats should have one values at -1");
                            }
                        }
                    }

                    if (!person.IsNpc && DataProvider.SiteHasFeature(Feature.PersonDescriptors))
                    {
                        if (string.IsNullOrWhiteSpace(postedStats.AddedDescriptor1))
                        {
                            ModelState.AddModelError("AddedDescriptor1", "First Descriptor must have a value");
                        }
                        if (string.IsNullOrWhiteSpace(postedStats.AddedDescriptor2))
                        {
                            ModelState.AddModelError("AddedDescriptor2", "Second Descriptor must have a value");
                        }
                        if (string.IsNullOrWhiteSpace(postedStats.AddedDescriptor3))
                        {
                            ModelState.AddModelError("AddedDescriptor3", "Third Descriptor must have a value");
                        }
                    }

                    if (DataProvider.SiteHasFeature(Feature.PersonRoles))
                    {
                        if (string.IsNullOrWhiteSpace(postedStats.AddedRole))
                        {
                            ModelState.AddModelError("AddedRole", "You must include a Role for this character");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        List<string> descriptors =
                            new List<string>
                            {
                                postedStats.AddedDescriptor1,
                                postedStats.AddedDescriptor2,
                                postedStats.AddedDescriptor3
                            };
                        DataProvider.SetStats(postedStats.PersonId, postedStats.Stats, postedStats.AddedRole,
                            descriptors);

                        if (DataProvider.SiteHasFeature(Feature.SimpleHitPoints))
                        {
                            DataProvider.SetDefaultHitPoints(postedStats.PersonId);
                        }

                        var model = GetCleanModel(postedStats.PersonId);
                        return PartialView("ViewStats", model);
                    }
                }
            }
            return PartialView("ViewStats", postedStats);
        }


        public ActionResult HitPoints(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.SimpleHitPoints))
            {
                Person person = DataProvider.GetPerson(id);
                if (person != null)
                {
                    SimpleHitPointsViewModel model = MakeSimpleHitPointsViewModel(person);
                    return PartialView(model);
                }
            }
            return null;
        }

        private SimpleHitPointsViewModel MakeSimpleHitPointsViewModel(Person person)
        {

            bool userCanBuy = person.PlayerId == CurrentPlayer.Id || !person.PlayerId.HasValue && CurrentPlayerIsGm;

            int baseCost = person.SimpleHitPoints.Count(h => h.Purchased.HasValue);

            SimpleHitPointsViewModel model = new SimpleHitPointsViewModel { CanEdit = userCanBuy, PersonId = person.Id };
            List<int> allLevels = SimpleHitPointLevel.Trivial.Numbers().OrderBy(i => i).ToList();

            model.WearTrack = new List<HitPointViewModel>();

            foreach (int level in allLevels)
            {
                SimpleHitPoint hitPoint =
                    person.SimpleHitPoints.FirstOrDefault(
                        s => s.HitPointLevelId == level && s.HitPointTypeId == (int) SimpleHitPointType.Wear);
            
                bool canBuy = person.CanBuyHitSlot((SimpleHitPointLevel)level, SimpleHitPointType.Wear) && userCanBuy;
                int cost = person.HitSlotCost((SimpleHitPointLevel)level, SimpleHitPointType.Wear);

                if (hitPoint != null)
                {
                    model.WearTrack.Add(new HitPointViewModel
                    {
                        PersonId = person.Id,
                        Type = (int)SimpleHitPointType.Wear,
                        Level = level,
                        CanBuy = canBuy,
                        CostToBuy =  cost,
                        Enabled = hitPoint.Purchased.HasValue,
                        Name = hitPoint.LongDisplayValue
                    });
                }
                else
                {
                    SimpleHitPoint temp = new SimpleHitPoint
                    {
                        HitPointLevelId = level,
                        HitPointTypeId = (int)SimpleHitPointType.Wear
                    };

                    model.WearTrack.Add(new HitPointViewModel
                    {
                        PersonId = person.Id,
                        Type = (int)SimpleHitPointType.Wear,
                        Level = level,
                        CanBuy = canBuy,
                        CostToBuy = cost,
                        Enabled = false,
                        Name = temp.LongDisplayValue
                    });
                }

            }

            model.HarmTrack = new List<HitPointViewModel>();

            foreach (int level in allLevels)
            {
                SimpleHitPoint hitPoint =
                    person.SimpleHitPoints.FirstOrDefault(
                        s => s.HitPointLevelId == level && s.HitPointTypeId == (int)SimpleHitPointType.Harm);

                bool canBuy = person.CanBuyHitSlot((SimpleHitPointLevel) level, SimpleHitPointType.Harm) && userCanBuy;
                int cost = person.HitSlotCost((SimpleHitPointLevel) level, SimpleHitPointType.Harm);

                if (hitPoint != null)
                {
                    model.HarmTrack.Add(new HitPointViewModel
                    {
                        PersonId = person.Id,
                        Type = (int)SimpleHitPointType.Harm,
                        Level = level,
                        CanBuy = canBuy,
                        CostToBuy = cost,
                        Enabled = hitPoint.Purchased.HasValue,
                        Name = hitPoint.LongDisplayValue
                    });
                }
                else
                {
                    SimpleHitPoint temp = new SimpleHitPoint
                    {
                        HitPointLevelId = level,
                        HitPointTypeId = (int)SimpleHitPointType.Harm
                    };

                    model.HarmTrack.Add(new HitPointViewModel
                    {
                        PersonId = person.Id,
                        Type = (int)SimpleHitPointType.Harm,
                        Level = level,
                        CanBuy = canBuy,
                        CostToBuy = cost,
                        Enabled = false,
                        Name = temp.LongDisplayValue
                    });
                }

            }

            return model;
        }


        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult AddRole(int personId, string role)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                DataProvider.AddRoleToPerson(personId, role);
            }
            var model = GetCleanModel(personId);
            return PartialView("ViewStats", model);
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult AddDescriptor(int personId, string descriptor)
        {
            if (!string.IsNullOrWhiteSpace(descriptor))
            {
                DataProvider.AddDescriptorToPerson(personId, descriptor);
            }
            var model = GetCleanModel(personId);
            return PartialView("ViewStats", model);
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult BuyStatIncrease(int personId, int statId)
        {
            StatName statName = (StatName) statId;
            DataProvider.BuyStatIncrease(personId, statName);
            var model = GetCleanModel(personId);
            return PartialView("ViewStats", model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult GiveXp(int personId, string xp)
        {
            int xpValue;
            if (int.TryParse(xp, out xpValue))
            {
                DataProvider.AddXp(personId, xpValue);
            }
            var model = GetCleanModel(personId);
            return PartialView("ViewStats", model);
        }

        public ActionResult AwardHistory(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.AwardHistory))
            {
                Person person = DataProvider.GetPerson(id);
                if (person != null)
                {
                    List<Award> awards = person.Awards.OrderByDescending(a => a.AwardedOn).ToList();
                    if (awards.Any())
                    {
                        return PartialView(awards);
                    }
                }
            }
            return null;
        }

        public ActionResult CurrentScorePie(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.CurrentScorePie))
            {
                Person person = DataProvider.GetPerson(id);
                if (person != null)
                {
                    if (!DataProvider.SiteHasFeature(Feature.PublicLeague))
                    {
                        if (person.PlayerId.HasValue && person.PlayerId != CurrentPlayer.Id)
                        {
                            return null;
                        }
                    }

                    List<ScoreHistory> scores = DataProvider.GetCurrentScoresForPerson(person.Id);
                        //person.ScoreHistories.OrderBy(a => a.DateTime).ToList();
                    if (scores.Any())
                    {

                        List<DateTime> dates = scores.Select(s => s.DateTime).ToList();
                        dates = dates.Distinct().ToList();

                        var vals = Enum.GetValues(typeof (ScoreType));

                        List<int> types = vals.Cast<int>().OrderByDescending(i => i).ToList();

                        types.Remove((int) ScoreType.Total);
                        if (!DataProvider.SiteHasFeature(Feature.SimpleStats))
                        {
                            types.Remove((int) ScoreType.Roles);
                            types.Remove((int) ScoreType.Descriptors);
                        }

                        if (!DataProvider.SiteHasFeature(Feature.SimpleStats) &
                            !DataProvider.SiteHasFeature(Feature.FateStats))
                        {
                            types.Remove((int) ScoreType.Stats);
                        }

                        Series pieSeries = new Series();
                        pieSeries.Name = "Current Score";

                        if (scores == null)
                        {
                            return null;
                        }
                        pieSeries.Data = new Data(scores.Where(s => s.ScoreType != ScoreType.Total).Where(s => s.PointsValue > 0).Select(s => new {name = s.ScoreType.ToString(), y = s.PointsValue }).ToArray());

                        DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("Points_Pie")
                            .InitChart(new Chart
                            {
                                DefaultSeriesType = ChartTypes.Pie
                            })
                            .SetTitle(new Title {Text = "Points Breakdown"})

                            .SetTooltip(new Tooltip
                            {
                                Shared = false,
                                ValueSuffix = " Points"
                            })
                            .SetXAxis(new XAxis
                            {
                                Categories = dates.Select(d => d.ToShortDateString()).ToArray(),
                                Title = new XAxisTitle {Text = "Type"}
                            }).SetYAxis(new YAxis
                            {
                                Title = new YAxisTitle {Text = "Points"}
                            })
                            .SetPlotOptions(new PlotOptions
                            {
                                Pie = new PlotOptionsPie()
                                { 
                                  Colors = scores.Where(s => s.ScoreType != ScoreType.Total).Where(s => s.PointsValue > 0).Select(s => GetColorForScoreType(s.ScoreType)).ToArray(),
                                  AllowPointSelect = true
                                 
                                }
                            })
                            .SetSeries(pieSeries);

                        return PartialView("Chart", chart);

                    }
                }
            }

            return null;
        }

        private Color GetColorForScoreType(ScoreType scoreType)
        {
            Color baseColor = new Color();
            switch (scoreType)
            {
                case ScoreType.Total:
                    baseColor =  Color.Black;
                    break;
                case ScoreType.Image:
                    baseColor = Color.BlueViolet;
                    break;
                case ScoreType.PageText:
                    baseColor = Color.Blue;
                    break;
                case ScoreType.Links:
                    baseColor = Color.DarkCyan;
                    break;
                case ScoreType.Sessions:
                    baseColor = Color.Green;
                    break;
                case ScoreType.Logs:
                    baseColor = Color.YellowGreen;
                    break;
                case ScoreType.Awards:
                    baseColor = Color.Gold;
                    break;
                case ScoreType.Stats:
                    baseColor = Color.Orange;
                    break;
                case ScoreType.Roles:
                    baseColor = Color.OrangeRed;
                    break;
                case ScoreType.Descriptors:
                    baseColor = Color.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scoreType), scoreType, null);


            }

            return Color.FromArgb(150, baseColor);
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult SetupHitPoints(int id)
        {
            Person person = DataProvider.GetPerson(id);
            if (person != null)
            {
                DataProvider.SetDefaultHitPoints(id);

                var model = GetCleanModel(id);
                return PartialView("ViewStats", model);
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult BuyHitPointSlot(int id, int level, int type)
        {
            Person person = DataProvider.GetPerson(id);
            if (person != null)
            {
                DataProvider.BuyHitPointSlot(id, (SimpleHitPointLevel)level, (SimpleHitPointType)type);
                var model = GetCleanModel(id);
                return PartialView("ViewStats", model);
            }
            return null;
        }

        [HttpGet]
        public ActionResult AssetsPanel(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.Assets))
            {
                Person person = DataProvider.GetPerson(id);
                if (person != null)
                {
                    PersonAssetsViewModel model = _factory.MakePersonAssetsViewModel(person);
                    return PartialView(model);
                }
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult AddAsset(PersonAssetsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.Assets))
            {
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    DataProvider.AddAsset(model.PersonId, model.AddAssetTitle, model.AddAssetDescription, model.AddAssetUpkeep);
                    ModelState.Clear();
                    PersonAssetsViewModel updatedModel = _factory.MakePersonAssetsViewModel(person);
                    updatedModel.AssetsJustSet = true;
                    return PartialView("AssetsPanel", updatedModel);
                }
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult AssetsPanel(PersonAssetsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.Assets))
            {
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    DataProvider.SetAssets(model.PersonId, model.Assets);

                    ModelState.Clear();
                    PersonAssetsViewModel updatedModel = _factory.MakePersonAssetsViewModel(person);
                    updatedModel.AssetsJustSet = true;
                    return PartialView(updatedModel);
                }
            }
            return null;
        }

        public ActionResult DetailsPanel(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonDetails))
            {
                Person person = DataProvider.GetPerson(id);
                if (person != null)
                {
                    CampaignDetail campagin = DataProvider.GetCampaginDetails();
                    PersonDetailsViewModel model = MakePersonDetailsViewModel(person, campagin);
                    return PartialView(model);
                }
            }
            return null;
        }


        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult SetAge(PersonDetailsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonDetails))
            {
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    DataProvider.SetAge(model.PersonId, model.Age);

                    ModelState.Remove("Age");
                    ModelState.Remove("ShowAge");
                    ModelState.Remove("DateOfBirthString");
                    ModelState.Remove("DateOfBirth");
                    CampaignDetail campagin = DataProvider.GetCampaginDetails();
                    PersonDetailsViewModel updatedModel = MakePersonDetailsViewModel(person, campagin);
                    updatedModel.AgeJustSet = true;
                    return PartialView("DetailsPanel", updatedModel);
                }
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult SetHeight(PersonDetailsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonDetails))
            {
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    DataProvider.SetHeight(model.PersonId, model.Height);

                    ModelState.Remove("Height");
                    ModelState.Remove("ShowHeight");
                    CampaignDetail campagin = DataProvider.GetCampaginDetails();
                    PersonDetailsViewModel updatedModel = MakePersonDetailsViewModel(person, campagin);
                    updatedModel.HeightJustSet = true;
                    return PartialView("DetailsPanel", updatedModel);
                }
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult SetMoney(PersonDetailsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonDetails))
            {
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    DataProvider.SetMoney(model.PersonId, model.Crowns, model.Shillings, model.Pennies, model.Upkeep);

                    ModelState.Clear();
                    CampaignDetail campagin = DataProvider.GetCampaginDetails();
                    PersonDetailsViewModel updatedModel = MakePersonDetailsViewModel(person, campagin);
                    updatedModel.MoneyJustSet = true;
                    return PartialView("DetailsPanel", updatedModel);
                }
            }
            return null;
        }


        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult SpendMoney(PersonDetailsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonDetails))
            {
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    DataProvider.SpendMoney(model.PersonId, model.SpendCrowns, model.SpendShillings, model.SpendPence);

                    ModelState.Clear();
                    CampaignDetail campagin = DataProvider.GetCampaginDetails();
                    PersonDetailsViewModel updatedModel = MakePersonDetailsViewModel(person, campagin);
                    updatedModel.MoneySpent = true;
                    return PartialView("DetailsPanel", updatedModel);
                }
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult AddMoney(PersonDetailsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonDetails))
            {
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    DataProvider.AddMoney(model.PersonId, model.AddCrowns, model.AddShillings, model.AddPence);

                    ModelState.Clear();
                    CampaignDetail campagin = DataProvider.GetCampaginDetails();
                    PersonDetailsViewModel updatedModel = MakePersonDetailsViewModel(person, campagin);
                    updatedModel.MoneyAdded = true;
                    return PartialView("DetailsPanel", updatedModel);
                }
            }
            return null;
        }


        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult SetDob(PersonDetailsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonDetails))
            {
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    DataProvider.SetDob(model.PersonId, model.DateOfBirth);

                    ModelState.Clear();
                    CampaignDetail campagin = DataProvider.GetCampaginDetails();
                    PersonDetailsViewModel updatedModel = MakePersonDetailsViewModel(person, campagin);
                    updatedModel.AgeJustSet = true;
                    return PartialView("DetailsPanel", updatedModel);
                }
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult DetailsPanel(PersonDetailsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonDetails))
            {
                Person person = DataProvider.GetPerson(model.PersonId);
                if (person != null)
                {
                    DataProvider.SetDetails(model.PersonId, model.Crowns, model.Shillings, model.Pennies,
                        model.DateOfBirth, model.Height, model.Upkeep);

                    CampaignDetail campagin = DataProvider.GetCampaginDetails();
                    PersonDetailsViewModel updatedModel = MakePersonDetailsViewModel(person, campagin);
                    return PartialView(updatedModel);
                }
            }
            return null;
        }

        public ActionResult AttributesPanel(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                CharacterAttributeModel model = _attributeManager.GetCharacterAttributes(id);
                if (model != null)
                {
                    if (model.HasStats)
                    {
                        return PartialView(model);
                    }
                    else
                    {
                        if (model.CharacterInfo.CanEdit)
                        {
                            CharacterInitialStatsModel initModel = _attributeManager.GetDefaultStats(id);
                            return PartialView("InitStats", initModel);
                        }

                    }
                    
                }
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult InitAttributes(CharacterInitialStatsModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                bool success = _attributeManager.InitializeStats(model);
                CharacterAttributeModel updatedModel = _attributeManager.GetCharacterAttributes(model.PersonId);
                if (updatedModel != null)
                {
                    return PartialView("AttributesPanel", updatedModel);
                }
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult BuyAttributeAdvance(int personId, int attributeId)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                bool success = _attributeManager.BuyAttributeAdvance(personId, attributeId);
                CharacterAttributeModel model = _attributeManager.GetCharacterAttributes(personId);
                if (model != null)
                {
                    return PartialView("AttributesPanel", model);
                }
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult BuyNewAttribute(int personId, AttributeType attributeType, string name, string description)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                bool success = _attributeManager.BuyNewAttribute(personId, attributeType, name, description);
                CharacterAttributeModel model = _attributeManager.GetCharacterAttributes(personId);
                if (model != null)
                {
                    return PartialView("AttributesPanel", model);
                }
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult MoveAttributePoint(int personId, int sourceAttributeId, int targetAttributeId)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                bool success = _attributeManager.MoveAttributePoint(personId, sourceAttributeId, targetAttributeId);
                CharacterAttributeModel model = _attributeManager.GetCharacterAttributes(personId);
                if (model != null)
                {
                    return PartialView("AttributesPanel", model);
                }
            }
            return null;
        }

        private PersonDetailsViewModel MakePersonDetailsViewModel(Person person, CampaignDetail campagin)
        {
            PersonDetailsViewModel model = new PersonDetailsViewModel
            {
                PersonId = person.Id,
                AllowEdit = CurrentPlayerIsGm || CurrentPlayer.Id == person.Id
            };

            if (person.TotalPennies != 0)
            {
                model.ShowMoney = true;
                model.Pennies = person.Pennies ?? 0;
                model.Shillings = person.Shillings ?? 0;
                model.Crowns = person.Crowns ?? 0;
                model.TotalPennies = person.TotalPennies;
                model.MoneyDisplayString = CurrencyHelper.DisplayString(person.TotalPennies);
            }

            if (person.Upkeep.HasValue)
            {
                model.Upkeep = person.Upkeep.Value;
            }

            model.TotalUpkeep = (0 - model.Upkeep) + person.Assets.Sum(a => a.Upkeep);

            if (person.DateOfBirth.HasValue && campagin.CurrentGameDate.HasValue)
            {
                model.ShowAge = true;
                model.DateOfBirth = person.DateOfBirth.Value;
                model.GameDate = campagin.CurrentGameDate.Value;
                DateTime today = campagin.CurrentGameDate.Value.Date;
                // Calculate the age.
                var age = today.Year - person.DateOfBirth.Value.Year;
                // Do stuff with it.
                if (person.DateOfBirth.Value > today.AddYears(-age))
                {
                    age--;
                }
                model.Age = age;
                model.DateOfBirthString = $"Born on the {person.DateOfBirth.Value.ToWarhammerDateString()} ({person.DateOfBirth:dddd dd MMMM yyyy})";
                model.DateOfBirth = person.DateOfBirth.Value;
            }

            if (!string.IsNullOrWhiteSpace(person.Height))
            {
                model.ShowHeight = true;
                model.Height = person.Height;
            }


            return model;
        }



    }
}