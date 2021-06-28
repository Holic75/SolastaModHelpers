using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace SolastaModHelpers.Patches
{
    class GuiPatcher
    {
        [HarmonyPatch]
        class Gui_LoadAssetSync
        {
            static System.Reflection.MethodBase TargetMethod()
            {
                // refer to C# reflection documentation:
                return typeof(Gui).GetMethod("LoadAssetSync", new Type[] { typeof(AssetReference) }).MakeGenericMethod(typeof(UnityEngine.Sprite));
            }

            static bool Prefix(AssetReference asset,
                               ref UnityEngine.Sprite __result)
            {
                var asset_reference_sprite = asset as AssetReferenceSprite;
                if (asset_reference_sprite == null)
                {
                    return true;
                }

                var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(asset.AssetGUID);
                if (custom_sprite == null)
                {
                    return true;
                }

                __result = custom_sprite;
                return false;
            } 
        }
    }
}
