using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace SolastaModHelpers.Patches
{
    class AssetReferencePatcher
    {
        [HarmonyPatch(typeof(AssetReference), "RuntimeKeyIsValid")]
        internal static class AssetReference_RuntimeKeyIsValid
        {
            internal static bool Prefix(AssetReference __instance, ref bool __result)
            {
                var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(__instance.AssetGUID);
                if (custom_sprite != null)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(AssetReference), "ReleaseAsset")]
        internal static class AssetReference_ReleaseAsset
        {
            internal static bool Prefix(AssetReference __instance)
            {
                var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(__instance.AssetGUID);
                if (custom_sprite != null)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
