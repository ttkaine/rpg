using Warhammer.Core.Models;

namespace Warhammer.Core.Abstract
{
    public interface IRandomItemGenerator
    {
        RandomItemResult PersonAge();
        RandomItemResult PersonSex();
        RandomItemResult PersonOrientation();
    }
}