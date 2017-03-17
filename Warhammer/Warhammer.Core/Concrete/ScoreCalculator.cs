using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class ScoreCalculator : IScoreCalculator
    {
        private int _scoreUpdateInterval = 10000;
        readonly IRepository _repo;
        private static readonly object _lock = new object();
        public ScoreCalculator(IRepository repo)
        {
            _repo = repo;
        }

        private List<ScoreHistory> CalculateScores(DateTime scoreDate)
        {
            List<Person> people = _repo.PeopleForScoring().ToList();
            List<ScoreHistory> scores = new List<ScoreHistory>();
            foreach (Person person in people)
            {
                scores.AddRange(CalculateScoreForPerson(person, scoreDate));
            }
            return scores;
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

                decimal statScore = 0m;

                if (person.FateStunts != null && person.FateStunts.Any())
                {
                    statScore += person.FateStunts.Count;
                }
                if (person.FateStats != null && person.FateStats.Any())
                {
                    statScore += person.FateStats.Sum(s => s.StatValue);
                }

                if (person.Stats != null && person.Stats.Any())
                {
                    decimal statFactor = person.IsCrowCharacter ? 6.0m : 2.0m;
                    statScore += person.Stats.Sum(l => l.Value)/statFactor;
                }
                if(statScore > 0)
                { 
                scoreHistories.Add(new ScoreHistory
                    {
                        ScoreType = ScoreType.Stats,
                        DateTime = scoreDate,
                        PersonId = person.Id,
                        PointsValue = statScore
                    });
                }
                else
                {
                    scoreHistories.Add(new ScoreHistory
                    {
                        ScoreType = ScoreType.Stats,
                        DateTime = scoreDate,
                        PersonId = person.Id,
                        PointsValue = 0m
                    });
                }

                if (person.RoleNames != null && person.RoleNames.Count > 1)
                {
                    scoreHistories.Add(new ScoreHistory
                    {
                        ScoreType = ScoreType.Roles,
                        DateTime = scoreDate,
                        PersonId = person.Id,
                        PointsValue = person.RoleNames.Count - 1
                    });
                }
                else
                {
                    if (person.FateAspects != null && person.FateAspects.Any())
                    {
                        scoreHistories.Add(new ScoreHistory
                        {
                            ScoreType = ScoreType.Roles,
                            DateTime = scoreDate,
                            PersonId = person.Id,
                            PointsValue = person.FateAspects.Count(a => a.IsVisible)
                        });
                    }
                    else
                    {
                        scoreHistories.Add(new ScoreHistory
                        {
                            ScoreType = ScoreType.Roles,
                            DateTime = scoreDate,
                            PersonId = person.Id,
                            PointsValue = 0
                        });
                    }
                }

                if (person.DescriptorNames != null && person.DescriptorNames.Count > 0)
                {
                    scoreHistories.Add(new ScoreHistory
                    {
                        ScoreType = ScoreType.Descriptors,
                        DateTime = scoreDate,
                        PersonId = person.Id,
                        PointsValue = person.DescriptorNames.Count
                    });
                }
                else
                {
                    scoreHistories.Add(new ScoreHistory
                    {
                        ScoreType = ScoreType.Descriptors,
                        DateTime = scoreDate,
                        PersonId = person.Id,
                        PointsValue = 0
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
            
            Person person = _repo.People()
                .Include(p => p.Awards.Select(a => a.Trophy))
                .Include(p => p.PersonStats)
                .Include(p => p.Pages)
                
                .FirstOrDefault(p => p.Id == personId);
            if (person != null)
            {
                List<ScoreHistory> scores = CalculateScoreForPerson(person, DateTime.Now).ToList();
                decimal current = scores.Where(s => s.ScoreType == ScoreType.Total).Sum(s => s.PointsValue);
                person.CurrentScore = current;

                List<ScoreHistory> original = _repo.ScoreHistories().Where(s => s.PersonId == personId).ToList();

                foreach (ScoreHistory scoreHistory in original)
                {
                    _repo.Delete(scoreHistory);
                }

                foreach (ScoreHistory scoreHistory in scores)
                {
                    _repo.Save(scoreHistory);
                }

                _repo.Save(person);
            }
        }

        public void UpdateScores()
        {
            DateTime cutOff = DateTime.Now.AddMinutes(-_scoreUpdateInterval);
            List<int> personIds = _repo.People()
                .Where(p => p.ScoreHistories.All(s => s.DateTime < cutOff))
                .OrderByDescending(s => s.ScoreHistories.Count==0)
                .ThenBy(p => p.ScoreHistories.Min(s => s.DateTime))
                .Take(3).Select(p => p.Id).ToList();
            foreach (var id in personIds)
            {
                UpdatePersonScore(id);
            }
        }
    }
}