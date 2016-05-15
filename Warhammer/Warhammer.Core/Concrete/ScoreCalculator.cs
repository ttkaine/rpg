﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class ScoreCalculator : IScoreCalculator
    {
        readonly IRepository _repo;

        public ScoreCalculator(IRepository repo)
        {
            _repo = repo;
        }

        public void UpdateScoreHistories()
        {
            DateTime lastCalculated = DateTime.Now.Date.AddDays(-1);

            ScoreHistory mostRecent = _repo.ScoreHistories().OrderByDescending(d => d.DateTime).FirstOrDefault();
            if (mostRecent != null)
            {
                lastCalculated = mostRecent.DateTime;
            }

            if (lastCalculated < DateTime.Now.Date)
            {
                lastCalculated = lastCalculated.AddDays(1);
                List<ScoreHistory> scores = CalculateScores(lastCalculated);
                foreach (ScoreHistory scoreHistory in scores)
                {
                    _repo.Save(scoreHistory);
                }
            }

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

            List<Session> sessions = person.Related.OfType<Session>().ToList();
            List<SessionLog> logs = (from session in sessions where person.SessionLogs.Count(l => l.SessionId == session.Id) > 0 select person.SessionLogs.First(l => l.SessionId == session.Id)).ToList();
            List<Page> relatedPages = person.Related.ToList();
            relatedPages = relatedPages.Where(s => !sessions.Contains(s)).ToList();
            relatedPages = relatedPages.Where(s => !logs.Contains(s)).ToList();

            scoreHistories.Add(new ScoreHistory
            {
                ScoreType = ScoreType.Sessions,
                DateTime = scoreDate,
                PersonId = person.Id,
                PointsValue = (decimal)sessions.Sum(l => l.BaseScore)
            });

            scoreHistories.Add(new ScoreHistory
            {
                ScoreType = ScoreType.Logs,
                DateTime = scoreDate,
                PersonId = person.Id,
                PointsValue = (decimal)logs.Sum(l => l.BaseScore)
            });

            scoreHistories.Add(new ScoreHistory
            {
                ScoreType = ScoreType.Awards,
                DateTime = scoreDate,
                PersonId = person.Id,
                PointsValue = person.Awards.Sum(a => a.Trophy.PointsValue)
            });

            scoreHistories.Add(new ScoreHistory
            {
                ScoreType = ScoreType.Links,
                DateTime = scoreDate,
                PersonId = person.Id,
                PointsValue = (decimal)relatedPages.Sum(l => l.BaseScore)
            });

            scoreHistories.Add(new ScoreHistory
            {
                ScoreType = ScoreType.Image,
                DateTime = scoreDate,
                PersonId = person.Id,
                PointsValue = person.HasImage ? 1 : 0
            });

            if (person.Stats != null && person.Stats.Any())
            {
                scoreHistories.Add(new ScoreHistory
                {
                    ScoreType = ScoreType.Stats,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = person.Stats.Sum(l => l.Value) / 6.0m
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
                scoreHistories.Add(new ScoreHistory
                {
                    ScoreType = ScoreType.Roles,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = 0
                });
            }

            if (person.DescriptorNames != null && person.DescriptorNames.Count > 3)
            {
                scoreHistories.Add(new ScoreHistory
                {
                    ScoreType = ScoreType.Descriptors,
                    DateTime = scoreDate,
                    PersonId = person.Id,
                    PointsValue = person.DescriptorNames.Count * 0.25m
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


            return scoreHistories;
        }
    }
}