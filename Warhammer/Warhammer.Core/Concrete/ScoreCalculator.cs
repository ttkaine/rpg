using System;
using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class ScoreCalculator : IScoreCalculator
    {
        private int _scoreUpdateInterval = 1440;
        private readonly IBackgroundRepo _repo;
        

        public ScoreCalculator(IBackgroundRepo repo)
        {
            _repo = repo;
        }

        private IEnumerable<ScoreBreakdown> CalculateScore(int personId, int campaignId)
        {
            DateTime calcDate = DateTime.Now;
            
            List<ScoreBreakdown> scores = new List<ScoreBreakdown>();

            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.Sessions,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = GetScoreForLinks<Session>(personId),
                CampaignId = campaignId
            });
            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.Places,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = GetScoreForLinks<Place>(personId),
                CampaignId = campaignId
            });
            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.OtherSessionLogs,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = GetScoreForLinks<SessionLog>(personId),
                CampaignId = campaignId
            });
            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.People,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = GetScoreForLinks<Person>(personId),
                CampaignId = campaignId
            });

            int? logWords = _repo.Pages().OfType<SessionLog>().Where(s => s.PersonId == personId)
                .Sum(p => (int?)p.WordCount);
            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.Logs,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = (decimal) (logWords.GetValueOrDefault(0) / 1000.0),
                CampaignId = campaignId
            });

            int? pageWords = _repo.Pages().OfType<Person>().Where(s => s.Id == personId)
                .Sum(p => (int?)p.WordCount);
            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.PageText,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = (decimal)(pageWords.GetValueOrDefault(0) / 100.0),
                CampaignId = campaignId
            });

            int? awardValue = _repo.Awards().Where(a => a.PersonId == personId).Sum(a => (int?)a.Trophy.PointsValue);
            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.Awards,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = awardValue.GetValueOrDefault(0),
                CampaignId = campaignId
            });

            return scores;
        }

        private decimal GetScoreForLinks<T>(int id) where T : Page
        {
            var query = _repo.Pages().OfType<T>();

            query = query.Where(p => p.WordCount > 0);
            query = query.Where(p => p.Pages.Any());
            query = query.Where(p => p.Pages.Any(r => r.Id == id));

            int? words = query.Sum(s => (int?)s.WordCount);

            return (decimal) (words.GetValueOrDefault(0) / 2000.0);
        }

        public void UpdatePersonScore(int personId)
        {
            Person person = _repo.Pages().OfType<Person>().FirstOrDefault(p => p.Id == personId);
                
            if (person != null)
            {
                int campaignId = person.CampaignId;
                List<ScoreBreakdown> scores = CalculateScore(personId, campaignId).ToList();
                decimal current = scores.Sum(s => s.PointsValue);
                person.CurrentScore = current;
                person.LastScoreCalculation = DateTime.Now;

                _repo.Save(person);
            }
        }

        public void UpdateScores()
        {
            DateTime cutOff = DateTime.Now.AddMinutes(-_scoreUpdateInterval);
            List<int> personIds = _repo.Pages().OfType<Person>()
                .Where(p => p.LastScoreCalculation < cutOff || p.LastScoreCalculation < p.Modified || p.LastScoreCalculation == null)
                .OrderByDescending(s => s.LastScoreCalculation == null)
                .ThenByDescending(p => p.SignificantUpdate)
                .ThenBy(p => p.LastScoreCalculation)
                .Take(5).Select(p => p.Id).ToList();
            foreach (var id in personIds)
            {
                UpdatePersonScore(id);
            }
        }
    }
}