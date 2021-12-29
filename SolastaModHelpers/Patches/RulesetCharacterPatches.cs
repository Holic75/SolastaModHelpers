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
        //support for IDefenseAffinty
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

        
        [HarmonyPatch(typeof(RulesetCharacter), "GrantPowers")]
        class RulesetCharacter_GrantPowers
        {
            internal static void Postfix(RulesetCharacter __instance)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IPowerNumberOfUsesIncrease>(__instance);
                var original = (__instance as RulesetCharacterMonster)?.originalFormCharacter;

                var usable_powers = __instance.usablePowers;
                foreach (var p in usable_powers)
                {
                    if (p?.powerDefinition == null)
                    {
                        continue;
                    }

                    p.maxUses = p.PowerDefinition.fixedUsesPerRecharge;
                    foreach (var f in features)
                    {
                        f.apply(__instance, p);
                    }

                    if (original != null)
                    {
                        var original_power = original.usablePowers.FirstOrDefault(u => u.powerDefinition == p.powerDefinition);
                        if (original_power != null)
                        {
                            p.remainingUses = original_power.remainingUses;
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "IsValidReadyCantrip")]
        class RulesetCharacter_IsValidReadyCantripr
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        SpellDefinition cantrip,
                                        ref bool __result)
            {
                if (cantrip.name == "SunlightBladeSpell")
                {
                    __result =  __instance.AreSpellComponentsValid(cantrip);
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
                    if (usablePower.PowerDefinition.rechargeRate == RuleDefinitions.RechargeRate.SpellSlot && usablePower.PowerDefinition.CostPerUse > 1)
                    {
                        __result = Helpers.Accessors.getNumberOfSpellsFromRepertoire(usablePower.PowerDefinition.CostPerUse, __instance.FindSpellRepertoireOfPower(usablePower)).total;
                    }
                    return;
                }
                __result = Math.Min(__instance.GetMaxUsesOfPower(base_power) * base_power.PowerDefinition.costPerUse / usablePower.powerDefinition.costPerUse, __result);
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "RepayPowerUse")]
        class RulesetCharacter_RepayPowerUse
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        RulesetUsablePower usablePower)
            {
                var base_power = (usablePower.PowerDefinition as NewFeatureDefinitions.LinkedPower)?.getBasePower(__instance);
                if (base_power == null)
                {
                    Helpers.Misc.synchronizePowers(__instance, usablePower);
                    return;
                }
                int uses_to_repay = usablePower.PowerDefinition.costPerUse / base_power.PowerDefinition.costPerUse;
                for (int i = 0; i < uses_to_repay; i++)
                {
                    base_power.RepayUse();
                }

                Helpers.Misc.synchronizePowers(__instance, base_power);
                Helpers.Misc.regularizePowerUses(__instance, base_power);
                __instance.RefreshAll();
            }
        }

        //allow to recover spell repertoires from original characters for wilshapes 
        [HarmonyPatch(typeof(RulesetCharacter), "FindSpellRepertoireOfPower")]
        class RulesetCharacter_FindSpellRepertoireOfPower
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        RulesetUsablePower usablePower, ref RulesetSpellRepertoire __result)
            {
               var original_character = (__instance as RulesetCharacterMonster)?.originalFormCharacter;
               if (__result == null && original_character != null)
               {
                    __result = original_character.SpellRepertoires.FirstOrDefault(sr => sr.spellCastingFeature == usablePower.PowerDefinition.spellcastingFeature);
                    return;
               }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "GetRemainingUsesOfPower")]
        class RulesetCharacter_GetRemainingUsesOfPower
        {
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

                if (usablePower?.PowerDefinition is NewFeatureDefinitions.HiddenPower)
                {
                    __result = 0;
                    return;
                }

                var base_power = (usablePower?.PowerDefinition as NewFeatureDefinitions.LinkedPower)?.getBasePower(__instance);
                if (base_power == null)
                {
                    Helpers.Misc.synchronizePowers(__instance, usablePower);
                    if (usablePower.PowerDefinition.rechargeRate == RuleDefinitions.RechargeRate.SpellSlot && usablePower.PowerDefinition.CostPerUse > 1)
                    {
                        __result = Helpers.Accessors.getNumberOfSpellsFromRepertoire(usablePower.PowerDefinition.CostPerUse, __instance.FindSpellRepertoireOfPower(usablePower)).remains;
                    }

                    return;
                }
                Helpers.Misc.synchronizePowers(__instance, base_power);
                __result = Math.Min(__instance.GetRemainingUsesOfPower(base_power) * base_power.PowerDefinition.CostPerUse / usablePower.PowerDefinition.costPerUse, __result);
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "UsePower")]
        class RulesetCharacter_UsePower
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var get_lowest_available_spell_level = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("GetLowestAvailableSlotLevel"));

                codes[get_lowest_available_spell_level] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1); //put usablePower on stack
                codes.Insert(get_lowest_available_spell_level + 1,
                              new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                             new Func<RulesetSpellRepertoire, RulesetUsablePower, int>(getLowestAvailbleSpellSlotOfMinLevel).Method
                                                             )
                            );
                return codes.AsEnumerable();
            }

            static int getLowestAvailbleSpellSlotOfMinLevel(RulesetSpellRepertoire spellRepertoire, RulesetUsablePower usable_power)
            {
                if (usable_power.PowerDefinition.CostPerUse <= 1)
                {
                    return spellRepertoire.GetLowestAvailableSlotLevel();
                }
                else
                {
                    return Helpers.Accessors.getLowestAvailableSlotLevelFromRepertoire(usable_power.PowerDefinition.CostPerUse, spellRepertoire);
                }
            }

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
                    Helpers.Misc.synchronizePowers(__instance, usablePower);
                    return;
                    
                }
                for (int i = 0; i < usablePower.PowerDefinition.costPerUse / base_power.PowerDefinition.costPerUse; i++)
                {
                    base_power.Consume();
                }
                Helpers.Misc.synchronizePowers(__instance, base_power);
                Helpers.Misc.regularizePowerUses(__instance, base_power);
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "ApplyRest")]
        class RulesetCharacter_ApplyRest
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        RuleDefinitions.RestType restType,
                                        bool simulate,
                                        TimeInfo restStartTime)
            {

                if (__instance?.recoveredFeatures != null)
                {
                    foreach (var f in __instance.recoveredFeatures.ToArray())
                    {
                        if ((f as NewFeatureDefinitions.LinkedPower)?.linkedPower != null)
                        {
                            __instance.recoveredFeatures.Remove(f);
                        }
                    }
                }
                //remove temporary item upon long rest
                if (restType == RuleDefinitions.RestType.LongRest && !simulate && __instance.CharacterInventory != null)
                {
                    foreach (var slot in __instance.CharacterInventory.EnumerateAllSlots().ToArray())
                    {
                        var item = slot?.EquipedItem;
                        if (item?.ItemDefinition != null 
                            && NewFeatureDefinitions.ItemsData.items_to_remove_on_long_rest.Contains(item.itemDefinition))
                        {
                            __instance.LoseItem(item);
                        }
                    }
                }
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var restore_all_spell_slots = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("RestoreAllSpellSlots"));

                codes[restore_all_spell_slots] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1); //put rest type on stack
                codes.Insert(restore_all_spell_slots + 1,
                              new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                             new Action<RulesetSpellRepertoire, RuleDefinitions.RestType>(applyRestToSpellSlots).Method
                                                             )
                            );
                return codes.AsEnumerable();
            }

            static void applyRestToSpellSlots(RulesetSpellRepertoire spellRepertoire, RuleDefinitions.RestType restType)
            {
                //account for lower level warlock spell slots that should recover on short rest and higher level spell slots that should recover on long rest
                var warlock_spellcasting = spellRepertoire?.spellCastingFeature as NewFeatureDefinitions.WarlockCastSpell;
                if (warlock_spellcasting == null)
                {
                    if (spellRepertoire.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.ShortRest
                        && (restType == RuleDefinitions.RestType.ShortRest || restType == RuleDefinitions.RestType.LongRest)
                        || spellRepertoire.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest && restType == RuleDefinitions.RestType.LongRest)
                    {
                        spellRepertoire.RestoreAllSpellSlots();
                    }
                }
                else if (restType == RuleDefinitions.RestType.LongRest)
                {
                    spellRepertoire.RestoreAllSpellSlots();
                }
                else
                {
                    for (int i = 0; i < warlock_spellcasting.mystic_arcanum_level_start; i++)
                    {
                        spellRepertoire.usedSpellsSlots.Remove(i);
                    }
                    spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
                }
            }
        }

        [HarmonyPatch(typeof(RulesetCharacter), "RollAttackMode")]
        class RulesetCharacter_RollAttackMode
        {
            internal static bool Prefix(RulesetCharacter __instance,
                                        RulesetAttackMode attackMode,
                                        RulesetActor target,
                                        BaseDefinition attackMethod,
                                        List<RuleDefinitions.TrendInfo> toHitTrends,
                                        bool ignoreAdvantage,
                                        List<RuleDefinitions.TrendInfo> advantageTrends,
                                        bool opportunity,
                                        int rollModifier,
                                        ref RuleDefinitions.RollOutcome outcome,
                                        ref int successDelta,
                                        ref int predefinedRoll,
                                        bool testMode)
            {
                var game_location_character = Helpers.Misc.findGameLocationCharacter(__instance);
                if (predefinedRoll != -1 || game_location_character == null)
                {
                    if (game_location_character != null)
                    {
                        NewFeatureDefinitions.AttackRollsData.removePrerolledData(game_location_character);
                    }
                    return true;
                }

                predefinedRoll = NewFeatureDefinitions.AttackRollsData.getPrerolledData(game_location_character).roll_value;
                return true;

            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "IsComponentMaterialValid")]
        class RulesetCharacter_IsComponentMaterialValid
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        SpellDefinition spellDefinition,
                                        ref bool __result)
            {
                if (__result)
                {
                    return;
                }

                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IMaterialComponentIgnore>(__instance);
                foreach (var f in features)
                {
                    if (f.canIgnoreMaterialComponent(__instance, spellDefinition))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "IsComponentSomaticValid")]
        class RulesetCharacter_IsComponentSomaticValid
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        SpellDefinition spellDefinition,
                                        ref bool __result)
            {
                if (__result)
                {
                    return;
                }

                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.ISomaticComponentIgnore>(__instance);
                foreach (var f in features)
                {
                    if (f.canIgnoreSomaticComponent(__instance, spellDefinition))
                    {
                        __result = true;
                        return;
                    }
                }
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

                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.ITargetApplyEffectOnDamageTaken>(__instance);
                foreach (var f in features)
                {
                    f.processDamageTargetTaken(__instance, totalDamageRaw, damageType);
                }

                if (__instance.currentHitPoints > 0)
                {
                    return;
                }

                var features2 = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IInitiatorApplyEffectOnCharacterDeath>(__instance);
                foreach (var f in features2)
                {
                    RulesetCharacter entity = (RulesetCharacter)null;
                    RulesetEntity.TryGetEntity<RulesetCharacter>(sourceGuid, out entity);
                    f.processDeath(entity, __instance);
                }
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


        [HarmonyPatch(typeof(RulesetCharacter), "IsSubjectToAttackOfOpportunity")]
        class RulesetCharacter_IsSubjectToAttackOfOpportunity
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        RulesetCharacter attacker,
                                        ref bool __result)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IIgnoreAooImmunity>(attacker);
                foreach (var f in features)
                {
                    if (f.canIgnore(attacker, __instance))
                    {
                        __result = true;
                        break;
                    }
                }
            }
        }
    }
}
