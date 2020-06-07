using System.Collections.Generic;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Core.Models.Crow;

namespace Warhammer.Core.Abstract
{
    public interface ICrowCharacterManager
    {
        CrowCharacterAttributesModel GetCharacterAttributes(int id);
        List<DefaultPersonAttribute> GetDefaultPersonAttributes();
        CrowCharacterGenerationModel GetCharacterGenerationModel(int id);
        void InitializeCharacter(int id);
        void AddTerm(AddedTermModel addedTerm);
    }
}