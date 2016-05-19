using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
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
            if (!DataProvider.SiteHasFeature(Feature.SimpleStats))
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
                    List<string> descriptors =
                       new List<string>
                        {
                            postedStats.AddedDescriptor1,
                            postedStats.AddedDescriptor2,
                            postedStats.AddedDescriptor3
                        };
                    DataProvider.SetStats(postedStats.PersonId, postedStats.Stats, postedStats.AddedRole, descriptors);

                    var model = GetCleanModel(postedStats.PersonId);
                    return PartialView("ViewStats", model);
                }

            }
            return PartialView("ViewStats", postedStats);
        }

        [HttpPost]
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

        public ActionResult ScoreHistory(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.ScoreHistory))
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

                    List<ScoreHistory> scores = DataProvider.PersonScoreHistory(person.Id);//person.ScoreHistories.OrderBy(a => a.DateTime).ToList();
                    if (scores.Any())
                    {

                        List<DateTime> dates = scores.Select(s => s.DateTime).ToList();
                          dates = dates.Distinct().ToList();

                        var vals = Enum.GetValues(typeof (ScoreType));

                        List<int> types = vals.Cast<int>().OrderByDescending(i => i).ToList();

                        types.Remove((int)ScoreType.Total);
                        if (!DataProvider.SiteHasFeature(Feature.SimpleStats))
                        {
                            types.Remove((int) ScoreType.Roles);
                            types.Remove((int)ScoreType.Descriptors);
                            }

                        if (!DataProvider.SiteHasFeature(Feature.SimpleStats) & !DataProvider.SiteHasFeature(Feature.FateStats))
                        {
                            types.Remove((int)ScoreType.Stats);
                        }

                        List<Series> allSeries = new List<Series>();

                        foreach (int type in types)
                        {
                            Series series = new Series();
                            series.Name = ((ScoreType)type).ToString();

                            decimal[] points = dates.Select(dateTime => scores.Where(s => s.DateTime == dateTime && s.ScoreTypeId == type).Sum(t => t.PointsValue)).ToArray();

                            series.Data = new Data(points.Cast<object>().ToArray());

                            allSeries.Add(series);
                        }


                        DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("Points_History")
                           .InitChart(new Chart
                           {
                               DefaultSeriesType = ChartTypes.Area
                           })
                                .SetTitle(new Title { Text = "Points History" })
                                
                            .SetTooltip(new Tooltip
                            {
                                Shared = false,
                                ValueSuffix = " Points"
                            })
                 .SetXAxis(new XAxis
                 {
                     Categories = dates.Select(d => d.ToShortDateString()).ToArray(),
                     Title = new XAxisTitle { Text = "Date" }
                     }).SetYAxis(new YAxis
                     {
                         Title = new YAxisTitle {  Text = "Points" }
                     })
                 .SetPlotOptions(new PlotOptions
                 {
                     Area = new PlotOptionsArea
                     {
                         
                         Stacking = Stackings.Normal,

                         ConnectNulls = true,
                         ConnectEnds = true,
                         LineColor = Color.AliceBlue,
                         LineWidth = 1,
                         Marker = new PlotOptionsAreaMarker
                         {
                             Enabled = false
                         }
                         
                     }
                 })
                 .SetSeries(allSeries.ToArray());

                return PartialView(chart);

                      //  return PartialView(scores);
                    }
                }
            }
            return null;

        }
    }
}