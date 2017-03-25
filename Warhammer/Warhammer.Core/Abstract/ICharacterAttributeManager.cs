using Warhammer.Core.Models;

namespace Warhammer.Core.Abstract
{
    public interface ICharacterAttributeManager
    {
        CharacterAttributeModel GetCharacterAttributes(int personId);
        bool BuyAttributeAdvance(int personId, int attributeId);
        bool BuyNewAttribute(int personId, AttributeType attributeType, string name, string description);
    }
}