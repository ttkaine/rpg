namespace Warhammer.Core.Abstract
{
    public interface IScoreCalculator
    {
        void UpdateScoreHistories();
        void UpdateUserScores(int id);
        void ForceUpdateScoresForToday(int id);
    }
}