using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Abstract
{
    public interface IViewModelFactory
    {
        ActiveTextSessionViewModel MakeActiveTextSessionViewModel();
    }
}
