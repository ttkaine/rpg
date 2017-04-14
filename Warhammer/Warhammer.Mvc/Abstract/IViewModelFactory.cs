using System.Collections.Generic;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Abstract
{
    public interface IViewModelFactory
    {
        ActiveTextSessionViewModel MakeActiveTextSessionViewModel();
        PersonStatViewModel MakeStatModel(Person person);
        MenuViewModel MakeMenu();
        UserSettingsViewModel MakeUserSettings();
        UserSettingsSectionViewModel Make(List<Setting> settingSection);
        PersonAssetsViewModel MakePersonAssetsViewModel(Person person);
        PlayerSessionControlsViewModel MakePlayerSessionControlsViewModel(Session session, Player player, bool playerIsGm);
        SessionGmViewModel MakeSessionGmViewModel(int id, List<Player> players, int gmId);
    }
}
