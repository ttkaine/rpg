﻿using System.Collections.Generic;
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
                    var chart = GetHighchartsPie(data, "wordcount_pie", "Wordcount by Page Type", "Words", " words");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }

        private static Highcharts GetHighchartsPie(List<ChartDataItem> data, string name, string title, string seriesName, string tooltipSuffix = "", string tooltipPrefix = "")
        {
            Series pieSeries = new Series();
            pieSeries.Name = seriesName;

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
                        //Colors = scores.Where(s => s.ScoreType != ScoreType.Total).Where(s => s.PointsValue > 0).Select(s => GetColorForScoreType(s.ScoreType)).ToArray(),
                        AllowPointSelect = true
                    }
                })
                .SetSeries(pieSeries);
            return chart;
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
                    var chart = GetHighchartsPie(data, "characterGender_pie", "Gender Representation", "Number of Characters", " characters");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }
    }
}