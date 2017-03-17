namespace Warhammer.Core.Abstract
{
    public interface IScoreCalculator
    {
       // void UpdateScoreHistories();
        void UpdatePersonScore(int personId);
        void UpdateScores();
    }
}