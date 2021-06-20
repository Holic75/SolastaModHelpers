using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class DoNotApplySpellOnSelfPatcher
    {
        [HarmonyPatch(typeof(CursorLocationSelectTarget), "IsFilteringValid")]
        internal static class CursorLocationSelectTarget_IsFilteringValid_Patch
        {
            internal static void Postfix(CursorLocationSelectTarget __instance, GameLocationCharacter target, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                var tr = HarmonyLib.Traverse.Create(__instance);
                var effect = tr.Field("effectDescription").GetValue<EffectDescription>();
                var tag = (ExtendedEnums.ExtraTargetFilteringTag)effect.TargetFilteringTag;
                if (effect == null || (tag & ExtendedEnums.ExtraTargetFilteringTag.NonCaster) == ExtendedEnums.ExtraTargetFilteringTag.No)
                {
                    return;
                }

                __result =  target != __instance.ActionParams.ActingCharacter;
                if (!__result)
                {
                    var action_modifier = tr.Field("actionModifier").GetValue<ActionModifier>();
                    action_modifier.FailureFlags.Add("Failure/&FailureFlagTargetIncorrectCreatureFamily");
                }
                return;
            }
        }
    }
}
