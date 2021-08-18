using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RulesetUsablePowerPatches
    {
        [HarmonyPatch(typeof(RulesetUsablePower), "MaxUses", MethodType.Getter)]
        internal static class RulesetUsablePower_MaxUses
        {
            internal static void Postfix(RulesetUsablePower __instance, ref int __result)
            {
                if (__instance.powerDefinition.usesDetermination == RuleDefinitions.UsesDetermination.AbilityBonusPlusFixed 
                    || __instance.powerDefinition.usesDetermination == RuleDefinitions.UsesDetermination.ProficiencyBonus)
                {
                    __result += (__instance.maxUses - __instance.powerDefinition.FixedUsesPerRecharge);
                }
            }
        }
    }
}
