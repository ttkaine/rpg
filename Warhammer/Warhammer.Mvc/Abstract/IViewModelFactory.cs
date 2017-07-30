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
        PageControlsViewModel MakePageControlsViewModel(Page page, bool isAdmin, bool playerIsGm, int currentPlayerId, List<Player> players);
        ManageAwardsViewModel MakeManageAwardsViewModel(List<Trophy> trophies, Person person);
        EditTrophyViewModel Make(Trophy trophy, bool playerIsGm, bool playerIsAdmin);
        TrophyCabinetViewModel Make(List<Trophy> trophies, bool currentPlayerIsGm, bool playerIsAdmin);
        NpcSheetViewModel MakeNpcSheetViewModel(Person npc);
        TrophyNominationViewModel MakeTrophyNominationViewModel(Person person, List<Trophy> trophies, List<AwardNomination> nominations);
    }
}
