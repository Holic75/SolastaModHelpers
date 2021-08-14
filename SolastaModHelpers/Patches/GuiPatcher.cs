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
    class GuiPatcher
    {
        [HarmonyPatch(typeof(GuiItemProperty), "SetupSprite")]
        class GuiItemProperty_SetupSprite
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var load_asset_sync = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("LoadAssetSync"));

                codes[load_asset_sync] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<AssetReferenceSprite, UnityEngine.Sprite>(loadIcon).Method
                                                                 );
                return codes.AsEnumerable();
            }

            static UnityEngine.Sprite loadIcon(AssetReferenceSprite sprite_reference)
            {
                var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(sprite_reference.AssetGUID);
                return custom_sprite ?? Gui.LoadAssetSync<UnityEngine.Sprite>(sprite_reference);
            }
        }

        [HarmonyPatch(typeof(BaseSelectionSlot), "Bind")]
        class BaseSelectionSlot_Bind
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var load_asset_sync = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("LoadAssetSync"));

                codes[load_asset_sync] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<AssetReferenceSprite, UnityEngine.Sprite>(loadIcon).Method
                                                                 );
                return codes.AsEnumerable();
            }

            static UnityEngine.Sprite loadIcon(AssetReferenceSprite sprite_reference)
            {
                var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(sprite_reference.AssetGUID);
                return custom_sprite ?? Gui.LoadAssetSync<UnityEngine.Sprite>(sprite_reference);
            }
        }


        [HarmonyPatch(typeof(RaceSelectionSlot), "Refresh")]
        class RaceSelectionSlot_Refresh
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var load_asset_sync = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("LoadAssetSync"));

                codes[load_asset_sync] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<AssetReferenceSprite, UnityEngine.Sprite>(loadIcon).Method
                                                                 );

                load_asset_sync = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("LoadAssetSync"));

                codes[load_asset_sync] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<AssetReferenceSprite, UnityEngine.Sprite>(loadIcon).Method
                                                                 );

                return codes.AsEnumerable();
            }

            static UnityEngine.Sprite loadIcon(AssetReferenceSprite sprite_reference)
            {
                var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(sprite_reference.AssetGUID);
                return custom_sprite ?? Gui.LoadAssetSync<UnityEngine.Sprite>(sprite_reference);
            }
        }


        [HarmonyPatch(typeof(GuiBaseDefinitionWrapper), "SetupSprite")]
        class GuiBaseDefinitionWrapper_SetupSprite
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var load_asset_sync = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("LoadAssetSync"));

                codes[load_asset_sync] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<AssetReferenceSprite, UnityEngine.Sprite>(loadIcon).Method
                                                                 );
                return codes.AsEnumerable();
            }

            static UnityEngine.Sprite loadIcon(AssetReferenceSprite sprite_reference)
            {
                var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(sprite_reference.AssetGUID);
                return custom_sprite ?? Gui.LoadAssetSync<UnityEngine.Sprite>(sprite_reference);
            }
        }


        [HarmonyPatch(typeof(GuiCharacter), "AssignClassImage")]
        class GuiCharacter_AssignClassImage
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var load_asset_sync = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("LoadAssetSync"));

                codes[load_asset_sync] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<AssetReferenceSprite, UnityEngine.Sprite>(loadClassIcon).Method
                                                                 );
                return codes.AsEnumerable();
            }

            static UnityEngine.Sprite loadClassIcon(AssetReferenceSprite sprite_reference)
            {
                var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(sprite_reference.AssetGUID);
                return custom_sprite ?? Gui.LoadAssetSync<UnityEngine.Sprite>(sprite_reference);
            }
        }


        [HarmonyPatch(typeof(GuiCharacter), "AssignRaceImage")]
        class GuiCharacter_AssignRaceImage
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var load_asset_sync = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("LoadAssetSync"));

                codes[load_asset_sync] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<AssetReferenceSprite, UnityEngine.Sprite>(loadClassIcon).Method
                                                                 );
                return codes.AsEnumerable();
            }

            static UnityEngine.Sprite loadClassIcon(AssetReferenceSprite sprite_reference)
            {
                var custom_sprite = CustomIcons.Tools.loadStoredCustomIcon(sprite_reference.AssetGUID);
                return custom_sprite ?? Gui.LoadAssetSync<UnityEngine.Sprite>(sprite_reference);
            }
        }



        /*[HarmonyPatch(typeof(GraphicsResourceManager), "LateUpdate")]
        class GraphicsResourceManager_LateUpdate
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var release = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("Release"));

                codes[release] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Action<UnityEngine.Object>(releaseObject).Method
                                                                 );
                return codes.AsEnumerable();
            }

            static void releaseObject(UnityEngine.Object asset)
            {
                if (CustomIcons.Tools.isCustomIcon(asset as UnityEngine.Sprite))
                {
                    return;
                }
                Addressables.Release<UnityEngine.Object>(asset);
            }
        }*/


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
