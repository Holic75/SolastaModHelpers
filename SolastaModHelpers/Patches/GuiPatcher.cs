﻿using HarmonyLib;
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
        //We can not reliably patch Gui.LoadAssetSync since it is generic,
        //so will have to do it case by case :(
        //Class selection images
        [HarmonyPatch(typeof(BaseSelectionSlot), "Bind")]
        class BaseSelectionSlot_Bind
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



        [HarmonyPatch(typeof(GraphicsResourceManager), "LateUpdate")]
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
        }
    }
}