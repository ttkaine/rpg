using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Core.Concrete
{
    public class ScoreCalculator : IScoreCalculator
    {
        private int _scoreUpdateInterval = 1440;
        private readonly IRepository _repo;

        public ScoreCalculator(IRepository repo)
        {
            _repo = repo;
        }

        private IEnumerable<ScoreHistory> CalculateScoreForPerson(Person person, DateTime scoreDate)
        {

            List<ScoreHistory> scoreHistories = new List<ScoreHistory>();

            if (person.Created < scoreDate)
            {

                List<Session> sessions = person.Related.OfType<Session>().ToList();
                List<SessionLog> logs = (from session in sessions
                    where person.SessionLogs.Count(l => l.SessionId == session.Id) > 0
                    select person.SessionLogs.First(l => l.SessionId == session.Id)).ToList();
                List<Page> relatedPages = person.Related.ToList();
                relatedPages = relatedPages.Where(s => !sessions.Contains(s)).ToList();
                relatedPages = relatedPages.Where(s => !logs.Contains(s)).ToList();

                scoreHistories.Add(new ScoreHistory
                {
                    ScoreType = ScoreType.Sessions,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = (decimal) sessions.Where(l => l.DateTime < scoreDate).Sum(l => l.BaseScore)       
                });

                scoreHistories.Add(new ScoreHistory
                {
                    ScoreType = ScoreType.Logs,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = (decimal) logs.Where(l => l.Created < scoreDate).Sum(l => l.BaseScore)
                });

                scoreHistories.Add(new ScoreHistory
                {
                    ScoreType = ScoreType.PageText,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = person.WordCount/100.0m
                });

                scoreHistories.Add(new ScoreHistory
                {
                    ScoreType = ScoreType.Awards,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = person.Awards.Where(l => l.AwardedOn < scoreDate).Sum(a => a.Trophy.PointsValue)
                });

                scoreHistories.Add(new ScoreHistory
                {
                    ScoreType = ScoreType.Links,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = (decimal) relatedPages.Where(l => l.Created < scoreDate).Sum(l => l.BaseScore)
                });

                scoreHistories.Add(new ScoreHistory
                {
                    ScoreType = ScoreType.Image,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = person.HasImage ? 1 : 0
                });

                if (person.HeroLevel > 0)
                {
                    scoreHistories.Add(new ScoreHistory
                    {
                        ScoreType = ScoreType.Level,
                        DateTime = scoreDate,
                        PersonId = person.Id,
                        PointsValue = (int)person.HeroLevel
                    });
                }
                ScoreHistory total = new ScoreHistory
                {
                    ScoreType = ScoreType.Total,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = scoreHistories.Sum(s => s.PointsValue)
                };

                scoreHistories.Add(total);

            }
            return scoreHistories;
        }

        public void UpdatePersonScore(int personId)
        {
            
            Person person = _repo.AllPeople()
                .Include(p => p.Awards.Select(a => a.Trophy))
                .Include(p => p.PersonStats)
                .Include(p => p.Pages)
                
                .FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                int campaignId = person.CampaignId;

                List<ScoreHistory> scores = CalculateScoreForPerson(person, DateTime.Now).ToList();
                decimal current = scores.Where(s => s.ScoreType == ScoreType.Total).Sum(s => s.PointsValue);
                person.CurrentScore = current;


                List<ScoreHistory> original = _repo.AllScoreHistories().Where(s => s.PersonId == personId).ToList();

                foreach (ScoreHistory scoreHistory in original)
                {
                    _repo.Delete(scoreHistory);
                }

                foreach (ScoreHistory scoreHistory in scores)
                {
                    scoreHistory.CampaignId = campaignId;
                    _repo.Save(scoreHistory);
                }

                person.LastScoreCalculation = DateTime.Now;

                _repo.Save(person);
            }
        }

        public void UpdateScores()
        {
            DateTime cutOff = DateTime.Now.AddMinutes(-_scoreUpdateInterval);
            List<int> personIds = _repo.AllPeople()
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