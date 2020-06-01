using Warhammer.Core.Models;
using Warhammer.Core.Models.Crow;

namespace Warhammer.Core.Abstract
{
    public interface ICrowCharacterManager
    {
        CrowCharacterAttributesModel GetCharacterAttributes(int id);
    }
}