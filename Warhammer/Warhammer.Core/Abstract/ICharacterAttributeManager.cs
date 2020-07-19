using Warhammer.Core.Models;

namespace Warhammer.Core.Abstract
{
    public interface ICharacterAttributeManager
    {
        CharacterAttributeModel GetCharacterAttributes(int personId);
        bool BuyAttributeAdvance(int personId, int attributeId);
        bool BuyNewAttribute(int personId, AttributeType attributeType, string name, string description, int initialValue);
        bool MoveAttributePoint(int personId, int sourceAttributeId, int targetAttributeId);
        bool RenameAttribute(int personId, int attributeId, string name, string description);
        CharacterInitialStatsModel GetDefaultStats(int id);
        bool InitializeStats(CharacterInitialStatsModel model);
        void ResetAttributes(int id);
        bool RefreshWear(int personId);
        bool ApplyWear(int personId, int attributeId);
        bool ApplyHarm(int personId, int attributeId, string harmMessage);
        bool RefreshHarm(int personId, int attributeId);
        bool SetDefaultWearAndHarm(int personId);
        bool SetAttributeVisibility(int personId, int attributeId, bool isVisible);
        bool AlterWishingWell(int personId, int amount);
        bool SetCurrentResolve(int personId, int updatedResolve);
        bool AddXp(int personId, decimal amount);
        bool SellAttributePoint(int personId, int targetAttributeId);

        bool InitRandomAttributes(RandomInitialStatsModel model);
    }
}