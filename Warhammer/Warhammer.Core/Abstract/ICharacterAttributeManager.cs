using Warhammer.Core.Models;

namespace Warhammer.Core.Abstract
{
    public interface ICharacterAttributeManager
    {
        CharacterAttributeModel GetCharacterAttributes(int personId);
        bool BuyAttributeAdvance(int personId, int attributeId);
        bool BuyNewAttribute(int personId, AttributeType attributeType, string name, string description);
        bool MoveAttributePoint(int personId, int sourceAttributeId, int targetAttributeId);
        CharacterInitialStatsModel GetDefaultStats(int id);
        bool InitializeStats(CharacterInitialStatsModel model);
        void ResetAttributes(int id);
    }
}