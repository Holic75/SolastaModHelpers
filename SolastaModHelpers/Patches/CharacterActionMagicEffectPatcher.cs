using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class CharacterActionMagicEffectPatcher
    {
        [HarmonyPatch(typeof(CharacterActionMagicEffect), "ExecuteImpl")]
        internal static class CharacterActionMagicEffect_ExecuteImpl_Patch
        {
            internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result, CharacterActionMagicEffect __instance)
            {
                while (__result.MoveNext())
                {
                    yield return __result.Current;
                }

                (__instance.GetBaseDefinition() as NewFeatureDefinitions.IPerformAttackAfterMagicEffectUse)?.performAttackAfterUse(__instance);            
            }
        }
    }
}
