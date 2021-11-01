using HarmonyLib;
using System.Collections.Generic;
using UnityModManagerNet;

namespace SolastaModHelpers.Patches
{
    class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GamingPlatformManager), "UpdateAvailableDlc")]
        internal static class GamingPlatformManager_UpdateAvailableDlc_Patch
        {
            internal static void Postfix(HashSet<GamingPlatformDefinitions.ContentPack> ___unlockedContentPacks)
            {

                ___unlockedContentPacks.Add(GamingPlatformDefinitions.ContentPack.BackerItems);
                ___unlockedContentPacks.Add(GamingPlatformDefinitions.ContentPack.DigitalBackerContent);
                ___unlockedContentPacks.Add(GamingPlatformDefinitions.ContentPack.LoadedDice);
                ___unlockedContentPacks.Add(GamingPlatformDefinitions.ContentPack.PrimalCalling);
            }
        }

        [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
        internal static class GameManager_BindPostDatabase_Patch
        {
            internal static void Postfix()
            {
                Main.ModEntryPoint();
            }
        }
    }
}
