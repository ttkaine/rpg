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


            decimal xp = _repo.Pages().OfType<Person>().Where(p => p.Id == personId).Select(p => p.XPAwarded)
                .FirstOrDefault();

            HeroLevel level = Person.GetHeroLevel(xp);
            if (level > 0)
            {
                scores.Add(new ScoreBreakdown
                {
                    ScoreType = ScoreType.Level,
                    DateTime = calcDate,
                    PersonId = personId,
                    PointsValue = (int)level,
                    CampaignId = campaignId
                });
            }


            int? roleValue = _repo.PersonAttributes()
                .Where(a => a.PersonId == personId).Where(a => a.PersonAttributeTypeEnum == (int)AttributeType.Role).Sum(a => (int?)a.CurrentValue);

            int rolePoints = (roleValue.GetValueOrDefault(0) - 3) * 3;

            if (rolePoints > 0)
            {
                scores.Add(new ScoreBreakdown
                {
                    ScoreType = ScoreType.Roles,
                    DateTime = calcDate,
                    PersonId = personId,
                    PointsValue = rolePoints,
                    CampaignId = campaignId
                });             
            }

            int? skillValue = _repo.PersonAttributes()
                .Where(a => a.PersonId == personId).Where(a => a.PersonAttributeTypeEnum == (int)AttributeType.Skill).Sum(a => (int?)a.CurrentValue);

            int skillPoints = (skillValue.GetValueOrDefault(0) - 6) * 2;

            if (skillPoints > 0)
            {
                scores.Add(new ScoreBreakdown
                {
                    ScoreType = ScoreType.Skills,
                    DateTime = calcDate,
                    PersonId = personId,
                    PointsValue = skillPoints,
                    CampaignId = campaignId
                });            
            }

            int? statValue = _repo.PersonAttributes()
                .Where(a => a.PersonId == personId).Where(a => a.PersonAttributeTypeEnum == (int)AttributeType.Stat).Sum(a => (int?)a.CurrentValue);
 
            int statPoints = statValue.GetValueOrDefault(0) - 18;

            if (statPoints > 0)
            {
                scores.Add(new ScoreBreakdown
                {
                    ScoreType = ScoreType.Stats,
                    DateTime = calcDate,
                    PersonId = personId,
                    PointsValue = statPoints,
                    CampaignId = campaignId
                });
            }

            int? descriptorValue = _repo.PersonAttributes()
                .Where(a => a.PersonId == personId).Count(a => a.PersonAttributeTypeEnum == (int)AttributeType.Descriptor);

            decimal descriptorPoints = (descriptorValue.GetValueOrDefault(0) - 3) / 2m;

            if (descriptorPoints > 0)
            {
                scores.Add(new ScoreBreakdown
                {
                    ScoreType = ScoreType.Descriptors,
                    DateTime = calcDate,
                    PersonId = personId,
                    PointsValue = descriptorPoints,
                    CampaignId = campaignId
                });
            }

            if (_repo.PageImages().Any(p => p.PageId == personId && p.IsPrimary))
            {
                scores.Add(new ScoreBreakdown
                {
                    ScoreType = ScoreType.Image,
                    DateTime = calcDate,
                    PersonId = personId,
                    PointsValue = 1,
                    CampaignId = campaignId
                });
            }

            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.Links,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = GetScoreForLinks<Page>(personId, includeWords: false) / 5,
                CampaignId = campaignId
            });

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

            int? otherLogCount = _repo.Pages()
                .OfType<SessionLog>()
                .Where(s => s.PersonId != personId)
                .Count(p => p.Pages.Any(r => r.Id == personId));

            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.OtherSessionLogs,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = (decimal)otherLogCount.GetValueOrDefault(0) / 4,
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
            int? logCount = _repo.Pages().OfType<SessionLog>()
                .Count(s => s.PersonId == personId);

            scores.Add(new ScoreBreakdown
            {
                ScoreType = ScoreType.Logs,
                DateTime = calcDate,
                PersonId = personId,
                PointsValue = (logWords.GetValueOrDefault(0) / 5000m) + (logCount.GetValueOrDefault(0)),
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

            int? awardValue = _repo.Awards()
                .Where(a => a.PersonId == personId)
                .Where(a => a.Trophy.TypeId != (int)TrophyType.Insignia)
                .Sum(a => (int?)a.Trophy.PointsValue);
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

        private decimal GetScoreForLinks<T>(int id, bool includeCount = true, bool includeWords = true, decimal factor = 2000m) where T : Page
        {
            var query = _repo.Pages().OfType<T>();

            query = query.Where(p => p.WordCount > 0);
            query = query.Where(p => p.Pages.Any());
            query = query.Where(p => p.Pages.Any(r => r.Id == id));

            int? words = null;
            int? count = null;

            if (includeWords)
            {
                words = query.Sum(s => (int?) s.WordCount);
            }

            if (includeCount)
            {
                count = query.Count();
            }

            return words.GetValueOrDefault(0) / factor + count.GetValueOrDefault(0);
        }

        public void UpdatePersonScore(int personId)
        {
            Person person = _repo.Pages().OfType<Person>().Include(s => s.ScoreBreakdowns).FirstOrDefault(p => p.Id == personId);
                
            if (person != null)
            {
                DateTime now = DateTime.Now;
                int campaignId = person.CampaignId;
                List<ScoreBreakdown> scores = CalculateScore(personId, campaignId).ToList();
                decimal current = scores.Sum(s => s.PointsValue);
                person.CurrentScore = current;
                person.LastScoreCalculation = now;

                foreach (var bd in scores)
                {
                    ScoreBreakdown updated = person.ScoreBreakdowns.FirstOrDefault(s => s.ScoreType == bd.ScoreType);
                    if (updated != null)
                    {
                        updated.DateTime = now;
                        updated.PointsValue = bd.PointsValue;
                    }
                    else
                    {
                        person.ScoreBreakdowns.Add(bd);
                    }
                }


                _repo.Save(person);
                List<ScoreBreakdown> removedScoreTypes =
                    person.ScoreBreakdowns.Where(s => scores.All(t => t.ScoreTypeId != s.ScoreTypeId)).ToList();

                foreach (var removed in removedScoreTypes)
                {
                    _repo.Delete(removed);
                }
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