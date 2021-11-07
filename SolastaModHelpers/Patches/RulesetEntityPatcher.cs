using HarmonyLib;
using SolastaModApi;
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
            //no longer needed since 1.2.x introdced Surrogate spell field of power definition to extract original spell context (although it will work correctly only if spell belongs to a single spell repertoire)
            /*internal static bool Prefix(RulesetEntity __instance, ref string attributeName, ref int __result)
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
            }*/

            //allow to get extract RageDamage attribute value from original caster if the rage was shared (like in case of War shaman)
            internal static void Postfix(RulesetEntity __instance, string attributeName, ref int __result)
            {
                var character = __instance as RulesetCharacter;
                if (character == null)
                {
                    return;
                }

                //extract RageDamage attribute from parent caster (for war shaman share rage)
                if (attributeName == "RageDamage")
                {
                    var condition = character.FindFirstConditionHoldingFeature(DatabaseHelper.FeatureDefinitionAdditionalDamages.AdditionalDamageConditionRaging);
                    if (condition == null)
                    {
                        return;
                    }
                    var caster = RulesetEntity.GetEntity<RulesetCharacter>(condition.SourceGuid) as RulesetCharacter;
                    if (caster == null || caster == character)
                    {
                        return;
                    }
                    var new_val = caster.TryGetAttributeValue(attributeName);
                    if (new_val > __result)
                    {
                        __result = new_val;
                    }
                    return;
                }

                return;
            }
        }
    }
}
