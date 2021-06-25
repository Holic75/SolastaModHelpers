using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RulesetCharacterPatches
    {
        [HarmonyPatch(typeof(RulesetCharacter), "ComputeAttackModifier")]
        class RulesetCharacter_ComputeAttackModifier
        {
            internal static void Postfix(RulesetCharacter __instance,
                                            RulesetCharacter defender,
                                            RulesetAttackMode attackMode,
                                            ActionModifier attackModifier,
                                            bool isWithin5Feet,
                                            bool isAllyWithin5Feet,
                                            int defenderSustainedAttacks,
                                            bool defenderAlreadyAttackedByAttackerThisTurn)
            {
                var defender_features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IDefenseAffinity>(defender);

                foreach (var feature in defender_features)
                {
                    feature.computeDefenseModifier(defender, __instance, defenderSustainedAttacks, defenderAlreadyAttackedByAttackerThisTurn, attackModifier, attackMode, defender.FeaturesOrigin[feature as FeatureDefinition]);
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "GetMaxUsesOfPower")]
        class RulesetCharacter_GetMaxUsesOfPower
        {
            internal static bool Prefix(RulesetCharacter __instance,
                                        RulesetUsablePower usablePower,
                                        ref int __result)
            {
                var base_power = (usablePower.PowerDefinition as NewFeatureDefinitions.LinkedPower)?.getBasePower(__instance);
                if (base_power == null)
                {
                    return true;
                }
                __result = __instance.GetMaxUsesOfPower(base_power);
                return false;
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "GetRemainingUsesOfPower")]
        class RulesetCharacter_GetRemainingUsesOfPower
        {
            internal static bool Prefix(RulesetCharacter __instance,
                                        RulesetUsablePower usablePower,
                                        ref int __result)
            {
                if (((usablePower.PowerDefinition as NewFeatureDefinitions.IPowerRestriction)?.isForbidden(__instance)).GetValueOrDefault())
                {
                    __result = 0;
                    return false;
                }
                var base_power = (usablePower.PowerDefinition as NewFeatureDefinitions.LinkedPower)?.getBasePower(__instance);
                if (base_power == null)
                {
                    return true;
                }
                __result = __instance.GetRemainingUsesOfPower(base_power);
                return false;
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "UsePower")]
        class RulesetCharacter_UsePower
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        RulesetUsablePower usablePower)
            {
                var base_power = (usablePower.PowerDefinition as NewFeatureDefinitions.LinkedPower)?.getBasePower(__instance);
                if (base_power == null)
                {
                    return;
                    
                }
                base_power.Consume();
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "CanUseAttackOutcomeAlterationPower")]
        class RulesetCharacter_CanUseAttackOutcomeAlterationPower
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var check_remaining_uses = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("RemainingUses"));

                codes[check_remaining_uses] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0);
                codes.Insert(check_remaining_uses + 1,
                              new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                             new Func<RulesetUsablePower, RulesetCharacter, int>(getNumberOfRemainingUses).Method
                                                             )
                            );
                return codes.AsEnumerable();
            }

            static int getNumberOfRemainingUses(RulesetUsablePower power, RulesetCharacter character)
            {
                return character.GetRemainingUsesOfPower(power);
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "FillAvailableMagicEffectList")]
        class RulesetCharacter_FillAvailableMagicEffectList
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var check_remaining_uses = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("RemainingUses"));

                codes[check_remaining_uses] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0);
                codes.Insert(check_remaining_uses + 1,
                              new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                             new Func<RulesetUsablePower, RulesetCharacter, int>(getNumberOfRemainingUses).Method
                                                             )
                            );
                return codes.AsEnumerable();
            }

            static int getNumberOfRemainingUses(RulesetUsablePower power, RulesetCharacter character)
            {
                return character.GetRemainingUsesOfPower(power);
            }
        }
    }
}
