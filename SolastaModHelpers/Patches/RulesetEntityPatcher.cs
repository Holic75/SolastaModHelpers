using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RulesetEntityPatcher
    {
        [HarmonyPatch(typeof(RulesetEntity), "TryGetAttributeValue")]
        internal static class RulesetEntity_TryGetAttributeValue
        {
            internal static bool Prefix(RulesetEntity __instance, ref string attributeName, ref int __result)
            {
                var character = __instance as RulesetCharacter;
                if (character == null)
                {
                    return true;
                }

                var ability_name = Helpers.Misc.extractContextDeterminedAttribute(character, attributeName);
                if (ability_name != string.Empty)
                {
                    attributeName = ability_name;
                }
                return true;
            }
        }
    }
}
