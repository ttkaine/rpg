using Warhammer.Core.Models;

namespace Warhammer.Core.Abstract
{
    public interface IRandomItemGenerator
    {
        RandomItemResult PersonAge();
        RandomItemResult PersonSex();
        RandomItemResult PersonOrientation();

        RandomItemResult MonsterCreature();
        RandomItemResult MonsterFeature();
        RandomItemResult MonsterTrait();
        RandomItemResult MonsterAbility();
        RandomItemResult MonsterTactic();
        RandomItemResult MonsterPersonality();
        RandomItemResult MonsterWeakness();
    }
}