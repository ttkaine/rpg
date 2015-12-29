using System.Collections.Generic;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Abstract
{
    public interface IViewModelFactory
    {
        ActiveTextSessionViewModel MakeActiveTextSessionViewModel();
        PersonStatViewModel MakeStatModel(Person person);
        string Combine(List<string> list);
    }
}
