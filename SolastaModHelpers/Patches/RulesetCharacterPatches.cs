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
                    feature.computeDefenseModifier(defender, __instance, defenderSustainedAttacks, defenderAlreadyAttackedByAttackerThisTurn, attackModifier, attackMode);
                }
            }
        }

        //remove overriden powers (in case there are too many of them like for druid wildshapes)
        [HarmonyPatch(typeof(RulesetCharacter), "GrantPowers")]
        class RulesetCharacter_GrantPowers
        {
            internal static void Postfix(RulesetCharacter __instance)
            {
                var overriden_powers = __instance.UsablePowers.Select(p => p.powerDefinition.overriddenPower).Where(p => p != null).ToHashSet();

                var powers_array = __instance.usablePowers.ToArray();

                foreach (var p in powers_array)
                {
                    if (overriden_powers.Contains(p?.powerDefinition))
                    {
                        __instance.usablePowers.Remove(p);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "GetMaxUsesOfPower")]
        class RulesetCharacter_GetMaxUsesOfPower
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        RulesetUsablePower usablePower,
                                        ref int __result)
            {
                var base_power = (usablePower.PowerDefinition as NewFeatureDefinitions.LinkedPower)?.getBasePower(__instance);
                if (base_power == null)
                {
                    return;
                }
                __result = Math.Min(__instance.GetMaxUsesOfPower(base_power), __result);
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "GetRemainingUsesOfPower")]
        class RulesetCharacter_GetRemainingUsesOfPower
        {
            /*internal static bool Prefix(RulesetCharacter __instance,
                                        RulesetUsablePower usablePower,
                                        ref int __result)
            {
                if (((usablePower.PowerDefinition as NewFeatureDefinitions.IPowerRestriction)?.isForbidden(__instance)).GetValueOrDefault())
                {
                    __result = 0;
                    return false;
                }
                return true;
            }*/


            internal static void Postfix(RulesetCharacter __instance,
                            RulesetUsablePower usablePower,
                            ref int __result)
            {
                if (__instance == null)
                {
                    return;
                }
                if (((usablePower?.PowerDefinition as NewFeatureDefinitions.IPowerRestriction)?.isReactionForbidden(__instance)).GetValueOrDefault())
                {
                    __result = 0;
                    return;
                }

                var base_power = (usablePower?.PowerDefinition as NewFeatureDefinitions.LinkedPower)?.getBasePower(__instance);
                if (base_power == null)
                {
                    return;
                }
                __result = Math.Min(__instance.GetRemainingUsesOfPower(base_power) * base_power.PowerDefinition.CostPerUse / usablePower.PowerDefinition.costPerUse, __result);
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "UsePower")]
        class RulesetCharacter_UsePower
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        RulesetUsablePower usablePower)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IApplyEffectOnPowerUse>(__instance);
                foreach (var f in features)
                {
                    f.processPowerUse(__instance, usablePower);
                }

                var base_power = (usablePower.PowerDefinition as NewFeatureDefinitions.LinkedPower)?.getBasePower(__instance);
                if (base_power == null)
                {
                    return;
                    
                }
                for (int i = 0; i < usablePower.PowerDefinition.costPerUse / base_power.PowerDefinition.costPerUse; i++)
                {
                    base_power.Consume();
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "ApplyRest")]
        class RulesetCharacter_ApplyRest
        {
            internal static bool Prefix(RulesetCharacter __instance,
                                        RuleDefinitions.RestType restType,
                                        bool simulate,
                                        TimeInfo restStartTime)
            {
                RulesetCharacterHeroPatcher.RulesetCharacterHero_PostLoad.refreshMaxPowerUses(__instance as RulesetCharacterHero);
                return true;
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


        [HarmonyPatch(typeof(RulesetCharacter), "AreSpellComponentsValid")]
        class RulesetCharacter_AreSpellComponentsValid
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        SpellDefinition spellDefinition,
                                        ref bool __result)
            {
                if (!__result)
                {
                    return;
                }


                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IForbidSpellcasting>(__instance);
                foreach (var f in features)
                {
                    if (f.isSpellcastingForbidden(__instance, spellDefinition))
                    {
                        __result = false;
                        return;
                    }
                }

                if (((spellDefinition as NewFeatureDefinitions.ISpellRestriction)?.isForbidden(__instance)).GetValueOrDefault())
                {
                    __result = false;
                    return;
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "GetMovementModifiers")]
        class RulesetCharacter_GetMovementModifiers
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        ref List<FeatureDefinition> __result)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IConditionalMovementModifier>(__instance);
                foreach (var f in features)
                {
                    f.tryAddConditionalMovementModfiers(__instance, __result);
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "SustainDamage")]
        class RulesetCharacter_SustainDamage
        {
            static void Postfix(RulesetCharacter __instance,
                                int totalDamageRaw,
                                string damageType,
                                bool criticalSuccess,
                                ulong sourceGuid,
                                RollInfo rollInfo,
                                bool forceKillOnZeroHp,
                                ConditionDefinition specialDeathCondition)
            {
                if (__instance.currentHitPoints > 0)
                {
                    return;
                }

                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IInitiatorApplyEffectOnCharacterDeath>(__instance);
                foreach (var f in features)
                {
                    RulesetCharacter entity = (RulesetCharacter)null;
                    RulesetEntity.TryGetEntity<RulesetCharacter>(sourceGuid, out entity);
                    f.processDeath(entity, __instance);
                }

                //NewFeatureDefinitions.Polymorph.maybeProcessPolymorphedDeath(__instance);
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var account_slain_enemy = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("AccountSlainEnemy"));

                codes[account_slain_enemy] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Action<RulesetCharacter, RulesetCharacter>(accountSlainEnemy).Method
                                                                 );
                codes.Insert(account_slain_enemy,
                             new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0)//load this == RulesetCharacter (target)
                            );
                return codes.AsEnumerable();
            }


            static void accountSlainEnemy(RulesetCharacter attacker, RulesetCharacter target)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IInitiatorApplyEffectOnTargetKill>(attacker);
                foreach (var f in features)
                {
                    f.processTargetKill(attacker, target);
                }
            }
        }
    }
}
