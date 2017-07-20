﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models.Reports;

namespace Warhammer.Mvc.Controllers
{
    public class ReportController : BaseController
    {
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WordcountChart()
        {
            if (DataProvider.SiteHasFeature(Feature.Reports) && DataProvider.SiteHasFeature(Feature.WordCountChart))
            {
                List<ChartDataItem> data = DataProvider.GetWordcountReportData();

                if (data.Any())
                {
                    int sum = data.Sum(d => d.Value);
                    var chart = GetHighchartsPie(data, "wordcount_pie", $"{sum} total words in the site", "Words", " words");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }

        private static Highcharts GetHighchartsPie(List<ChartDataItem> data, string name, string title, string seriesName, string tooltipSuffix = "", string tooltipPrefix = "")
        {
            Series pieSeries = new Series();
            pieSeries.Name = seriesName;

            Color[] colors = GetDefaultColors(data.Count);
            if (data.Any(d => !d.Color.IsEmpty))
            {
                colors = data.Select(d => d.Color).ToArray();
            }

            pieSeries.Data = new Data(data.Where(s => s.Value > 0).Select(s => new {name = s.Name, y = s.Value})
                .ToArray());

            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts(name)
                .InitChart(new Chart
                {
                    DefaultSeriesType = ChartTypes.Pie
                })
                .SetTitle(new Title {Text = title })
                .SetTooltip(new Tooltip
                {
                    Shared = false,
                    ValueSuffix = tooltipSuffix,
                    ValuePrefix = tooltipPrefix
                })
                .SetPlotOptions(new PlotOptions
                {
                    Pie = new PlotOptionsPie()
                    {
                        Colors = colors.Select(c => Color.FromArgb(150, c)).ToArray(),
                        AllowPointSelect = true
                    }
                })
                .SetSeries(pieSeries);
            return chart;
        }

        private static Color[] GetDefaultColors(int count)
        {
            List<Color> colors = new List<Color>();
            for (int i = 0; i < count; i++)
            {
                switch (i % 10)
                {
                    case 0:
                        colors.Add(Color.Black);
                        break;
                    case 1:
                        colors.Add(Color.BlueViolet);
                        break;
                    case 2:
                        colors.Add(Color.Blue);
                        break;
                    case 3:
                        colors.Add(Color.DarkCyan);
                        break;
                    case 4:
                        colors.Add(Color.Green);
                        break;
                    case 5:
                        colors.Add(Color.YellowGreen);
                        break;
                    case 6:
                        colors.Add(Color.Gold);
                        break;
                    case 7:
                        colors.Add(Color.Orange);
                        break;
                    case 8:
                        colors.Add(Color.OrangeRed);
                        break;
                    case 9:
                        colors.Add(Color.Red);
                        break;
                    default:
                        break;


                }
            }
            return colors.ToArray();
        }

        public ReportController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        public ActionResult PlayerWordcountChart()
        {
            if (DataProvider.SiteHasFeature(Feature.Reports) && DataProvider.SiteHasFeature(Feature.PlayerWordCountChart))
            {
                List<ChartDataItem> data = DataProvider.GetPlayerWordcountReportData();

                if (data.Any())
                {
                    var chart = GetHighchartsPie(data, "playerwordcount_pie", "Wordcount by Player (approx)", "Words", " words", "About ");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }

        public ActionResult CharacterGenderChart()
        {
            if (DataProvider.SiteHasFeature(Feature.Reports) && DataProvider.SiteHasFeature(Feature.CharacterGenderChart))
            {
                List<ChartDataItem> data = DataProvider.GetCharacterGenderReportData();

                if (data.Any())
                {
                    int sum = data.Sum(d => d.Value);
                    var chart = GetHighchartsPie(data, "characterGender_pie", $"{sum} characters on the site", "Number of Characters", " characters");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }

        public ActionResult GenderScoresChart()
        {
            if (DataProvider.SiteHasFeature(Feature.Reports) && DataProvider.SiteHasFeature(Feature.GenderScoresChart))
            {
                List<ChartDataItem> data = DataProvider.GetGenderScoresReportData();

                if (data.Any())
                {
                    int sum = data.Sum(d => d.Value);
                    var chart = GetHighchartsPie(data, "gender_score_pie", $"{sum} points awarded to characters", "Total Points for Characters", " points");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }

        public ActionResult PagesByPlayerChart()
        {
            if (DataProvider.SiteHasFeature(Feature.Reports) && DataProvider.SiteHasFeature(Feature.PlayerPageChart))
            {
                List<ChartDataItem> data = DataProvider.GetPagesByPlayerReportData();

                if (data.Any())
                {
                    int sum = data.Sum(d => d.Value);
                    var chart = GetHighchartsPie(data, "player_page_pie", $"{sum} pages on the site", "Created ", " pages");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }

        public ActionResult PcNpcScoresChart()
        {
            if (DataProvider.SiteHasFeature(Feature.Reports) && DataProvider.SiteHasFeature(Feature.PcNpcScoresChart))
            {
                List<ChartDataItem> data = DataProvider.GetPcNpcScoresReportData();

                if (data.Any())
                {
                    var chart = GetHighchartsPie(data, "pc_npc_score_pie", "Character Engagement by Player Type", "Total Points for Characters", " points");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }
    }
}