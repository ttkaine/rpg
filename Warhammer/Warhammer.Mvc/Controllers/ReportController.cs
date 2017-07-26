using System;
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

        private Highcharts GetHighchartsPie(List<ChartDataItem> data, string name, string title, string seriesName, string tooltipSuffix = "", string tooltipPrefix = "")
        {
            Series pieSeries = new Series();
            pieSeries.Name = seriesName;

            Color[] colors = DataProvider.GetDefaultColors(data.Count);
            if (data.Any(d => !d.Color.IsEmpty))
            {
                colors = data.Select(d => d.Color).ToArray();
            }

            pieSeries.Data = new Data(data.Where(s => s.Value > 0).Select(s => new {name = s.Name, y = s.Value})
                .ToArray());

            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts(name)
                .InitChart(new Chart
                {
                    DefaultSeriesType = ChartTypes.Pie,
                    
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
            chart.SetCredits(new Credits {Enabled = false});
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

                if (data.Count > 1)
                {
                    var chart = GetHighchartsPie(data, "playerwordcount_pie", "Wordcount by Player (approx)", "Words", " words", "About ");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }

        public ActionResult PlayerTextPostChart()
        {
            if (DataProvider.SiteHasFeature(Feature.Reports) && DataProvider.SiteHasFeature(Feature.PlayerTextPostChart))
            {
                List<ChartDataItem> data = DataProvider.GetPlayerTextPostReportData();

                if (data.Count > 1)
                {
                    int sum = data.Sum(d => d.Value);
                    var chart = GetHighchartsPie(data, "playertextpost_pie", $"{sum} total posts in Text Sessions", "Posts", " posts", "About ");
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

                if (data.Count > 1)
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

                if (data.Count > 1)
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

                if (data.Count > 1)
                {
                    int sum = data.Sum(d => d.Value);
                    var chart = GetHighchartsPie(data, "player_page_pie", $"{sum} pages on the site", "Created ", " pages");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }

        public ActionResult TopAwardsChart()
        {
            if (DataProvider.SiteHasFeature(Feature.Reports) && DataProvider.SiteHasFeature(Feature.TopAwardsChart))
            {
                List<ChartDataItem> data = DataProvider.GetTopAwardsReportData();

                if (data.Count > 1)
                {
                    int sum = DataProvider.TotalAwardCount();
                    var chart = GetHighchartsPie(data, "top_awards_pie", $"{sum} Trophies Awarded", "Trophy ", " times", "Awarded ");
                    return PartialView("Chart", chart);
                }

            }
            return null;
        }
    }
}