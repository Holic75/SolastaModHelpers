using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class GameLocationSelectionManagerPatcher
    {
        [HarmonyPatch(typeof(GameLocationSelectionManager), "GuestRemoved")]
        internal static class AssetReference_ReleaseAsset
        {
            internal static bool Prefix(GameLocationSelectionManager __instance, GameLocationCharacter guest)
            {
                if (__instance?.SelectedCharacters == null || guest == null
                    || ServiceRepository.GetService<IGameLocationBattleService>() == null
                    || ServiceRepository.GetService<IPlayerControllerService>() == null)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
