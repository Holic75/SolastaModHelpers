using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace SolastaModHelpers.Patches
{
    //set of patches to support custom icons usage (Currently works for Class, Race, Spell, Power and Item sprites)
    class GuiPatcher
    {
        static UnityEngine.Sprite loadIcon(AssetReferenceSprite sprite_reference)
        {
            var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(sprite_reference.AssetGUID);
            return custom_sprite ?? Gui.LoadAssetSync<UnityEngine.Sprite>(sprite_reference);
        }

        static IEnumerable<CodeInstruction> replaceLoadAssetSyncCalls(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            bool should_search = true;
            int idx_start = 0;
            while (should_search)
            {
                var load_asset_sync = codes.FindIndex(idx_start, x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("LoadAssetSync"));
                if (load_asset_sync > 0)
                {
                    codes[load_asset_sync] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                     new Func<AssetReferenceSprite, UnityEngine.Sprite>(loadIcon).Method
                                                                     );
                    idx_start = load_asset_sync + 1;
                }
                else
                {
                    should_search = false;
                }
            }
            return codes.AsEnumerable();
        }


        [HarmonyPatch(typeof(GuiItemProperty), "SetupSprite")]
        class GuiItemProperty_SetupSprite
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return replaceLoadAssetSyncCalls(instructions);
            }
        }

        [HarmonyPatch(typeof(BaseSelectionSlot), "Bind")]
        class BaseSelectionSlot_Bind
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return replaceLoadAssetSyncCalls(instructions);
            }

        }


        [HarmonyPatch(typeof(RaceSelectionSlot), "Refresh")]
        class RaceSelectionSlot_Refresh
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return replaceLoadAssetSyncCalls(instructions);
            }
        }


        [HarmonyPatch(typeof(GuiBaseDefinitionWrapper), "SetupSprite")]
        class GuiBaseDefinitionWrapper_SetupSprite
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return replaceLoadAssetSyncCalls(instructions);
            }
        }


        [HarmonyPatch(typeof(GuiCharacter), "AssignClassImage")]
        class GuiCharacter_AssignClassImage
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return replaceLoadAssetSyncCalls(instructions);
            }
        }


        [HarmonyPatch(typeof(GuiCharacter), "AssignRaceImage")]
        class GuiCharacter_AssignRaceImage
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return replaceLoadAssetSyncCalls(instructions);
            }
        }


        [HarmonyPatch(typeof(InventorySlotBox), "RefreshState")]
        class InventorySlotBox_RefreshState
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return replaceLoadAssetSyncCalls(instructions);
            }
        }


        [HarmonyPatch(typeof(GuiCharacterAction), "SetupSprite")]
        class GuiCharacterAction_SetupSprite
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return replaceLoadAssetSyncCalls(instructions);
            }
        }


        [HarmonyPatch(typeof(InventoryPanel), "StartDrag")]
        class InventoryPanel_StartDrag
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return replaceLoadAssetSyncCalls(instructions);
            }
        }



        [HarmonyPatch(typeof(Gui), "ReleaseAddressableAsset")]
        internal static class Gui_ReleaseAddressableAsset_Patch
        {
            internal static bool Prefix(UnityEngine.Object asset)
            {
                var sprite = asset as UnityEngine.Sprite;
                if (sprite != null && CustomIcons.Tools.isCustomIcon(sprite))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
