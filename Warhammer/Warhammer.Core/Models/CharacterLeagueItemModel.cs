using System;
using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models
{
    public class CharacterLeagueItemModel
    {
        public List<AwardSummaryModel> Awards { get; set; }
        public string FullName { get; set; }
        public decimal CurrentScore { get; set; }
        public string PlainText { get; set; }
        public int Id { get; set; }
        public decimal XpAwarded { get; set; }
        public string Player { get; set; }
        public string PlayerName => Player ?? "NPC";
        public string Campaign { get; set; }
        public bool IsSiteVersion { get; set; }
        public HeroLevel HeroLevel => Person.GetHeroLevel(XpAwarded);
        public int PointsValue => (int)Math.Ceiling(CurrentScore);

        public string SearchString
        {
            get
            {
                string[] awards = Awards.Select(a => a.TrophyName).ToArray();
                string[] reasons = Awards.Select(a => a.Reason).ToArray();
                return string.Format("{0} - {1} - {2}", PlainText,
                    string.Join(",", awards),
                    string.Join(",", reasons));
            }
        }

        public string MiniSummary {
            get
            {
                int length = 250;
                const string str = "...";
                string text = PlainText;

                if (!string.IsNullOrWhiteSpace(text) && text.Length > length)
                {
                    text = text.Substring(0, length - str.Length) + str;
                }
                return text;
            }
        }

        public int Rank { get; set; }
        public string Url { get; set; }
        public int CampaignId { get; set; }
        public string ImageUrl { get; set; }
        public string ImageFile { get; set; }
    }
}