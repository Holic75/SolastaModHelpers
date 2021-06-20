using HarmonyLib;
using SolastaModApi.Infrastructure;

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

                var effect = __instance.GetField<EffectDescription>("effectDescription");

                var tag = (ExtendedEnums.ExtraTargetFilteringTag)effect.TargetFilteringTag;
                if (effect == null || (tag & ExtendedEnums.ExtraTargetFilteringTag.NonCaster) == ExtendedEnums.ExtraTargetFilteringTag.No)
                {
                    return;
                }

                __result = target != __instance.ActionParams.ActingCharacter;
                if (!__result)
                {
                    var action_modifier = __instance.GetField<ActionModifier>("actionModifier");
                    action_modifier.FailureFlags.Add("Failure/&FailureFlagTargetIncorrectCreatureFamily");
                }
                return;
            }
        }
    }
}
