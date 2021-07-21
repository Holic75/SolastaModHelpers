using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RulesetEffectPowerPatcher
    {
        [HarmonyPatch(typeof(RulesetEffectPower), "SourceAbility", MethodType.Getter)]
        class RulesetEffectPower_SourceAbility
        {
            static string Postfix(string __result, RulesetEffectPower __instance)
            {
                var custom_power = __instance.UsablePower?.PowerDefinition as NewFeatureDefinitions.ICustomPowerAbilityScore;
                if (custom_power == null)
                {
                    return __result;
                }

                return custom_power.getPowerAbilityScore(__instance.user);
            }
        }

        [HarmonyPatch(typeof(RulesetEffectPower), "ComputeSourceAbilityBonus")]
        class RulesetEffectPower_ComputeSourceAbilityBonus
        {
            static int Postfix(int __result, RulesetEffectPower __instance, RulesetCharacter source)
            {
                var custom_power = __instance.UsablePower?.PowerDefinition as NewFeatureDefinitions.ICustomPowerAbilityScore;
                if (custom_power == null)
                {
                    return __result;
                }

                return AttributeDefinitions.ComputeAbilityScoreModifier(source.GetAttribute(__instance.SourceAbility).CurrentValue);
            }
        }


        [HarmonyPatch(typeof(RulesetEffectPower), "GetFormAbilityBonus")]
        class RulesetEffectPower_GetFormAbilityBonus
        {
            static int Postfix(int __result, RulesetCharacter character, RulesetEffectPower __instance)
            {
                var custom_power = __instance.UsablePower?.PowerDefinition as NewFeatureDefinitions.ICustomPowerAbilityScore;
                if (custom_power == null)
                {
                    return __result;
                }

                return __instance.ComputeSourceAbilityBonus(character);
            }
        }


        [HarmonyPatch(typeof(RulesetEffectPower), "MagicAttackBonus", MethodType.Getter)]
        class RulesetEffectPower_MagicAttackBonus
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var get_attribute = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("GetAttribute"));
                codes[get_attribute] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<RulesetCharacter, RulesetEffectPower, RulesetAttribute>(getCustomAbilityScore).Method
                                                                 );
                codes.RemoveRange(get_attribute - 3, 3);
                return codes.AsEnumerable();
            }

            static RulesetAttribute getCustomAbilityScore(RulesetCharacter character, RulesetEffectPower effect)
            {
                return character.GetAttribute(effect.SourceAbility);
            }
        }


        [HarmonyPatch(typeof(RulesetEffectPower), "MagicAttackTrends", MethodType.Getter)]
        class RulesetEffectPower_MagicAttackTrends
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var get_attribute = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("GetAttribute"));
                codes[get_attribute - 1] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0); //load this == RulesetEffectPower
                codes[get_attribute] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<RulesetCharacter, RulesetEffectPower, RulesetAttribute>(getCustomAbilityScore).Method
                                                                 );
                codes.RemoveRange(get_attribute - 3, 3);
                var list_add = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("Add"));
                var get_ability_score = list_add - 3;
                codes[get_ability_score] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<RulesetEffectPower, string>(getCustomAbilityScoreString).Method
                                                                 );
                codes.RemoveAt(get_ability_score - 1);
                return codes.AsEnumerable();
            }

            static RulesetAttribute getCustomAbilityScore(RulesetCharacter character, RulesetEffectPower effect)
            {
                return character.GetAttribute(effect.SourceAbility);
            }

            static string getCustomAbilityScoreString(RulesetEffectPower effect)
            {
                return effect.SourceAbility;
            }
        }
    }
}
