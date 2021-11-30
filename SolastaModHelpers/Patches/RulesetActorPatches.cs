using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RuleActorPatcher
    {
        [HarmonyPatch(typeof(RulesetActor), "RollDamage")]
        internal class RulesetActor_RollDamage
        {
            internal static DamageForm currentDamageForm = null;
            internal static void Prefix(DamageForm damageForm)
            {
                currentDamageForm = damageForm;
            }


            internal static void Postfix()
            {
                currentDamageForm = null;
            }
        }


        [HarmonyPatch(typeof(RulesetActor), "RerollDieAsNeeded")]
        class RulesetActor_RerollDieAsNeeded
        {
            internal static bool Prefix(FeatureDefinitionDieRollModifier dieRollModifier,
                                        RuleDefinitions.DieType dieType,
                                        int rollScore,
                                        ref int __result)
            {
                NewFeatureDefinitions.ModifyDamageRollTypeDependent roll_modifier = dieRollModifier as NewFeatureDefinitions.ModifyDamageRollTypeDependent;
                if (roll_modifier == null 
                    || RulesetActor_RollDamage.currentDamageForm == null
                    || roll_modifier.damageTypes.Contains(RulesetActor_RollDamage.currentDamageForm.damageType))
                {
                    return true;
                }

                __result = rollScore;
                return false;
            }
        }



        [HarmonyPatch(typeof(RulesetActor), "RollDiceAndSum")]
        class RulesetActor_RollDiceAndSum
        {
            internal static void Postfix(RulesetActor __instance,
                                        RuleDefinitions.DieType diceType,
                                        RuleDefinitions.RollContext context,
                                        int diceNumber,
                                        List<int> rolledValues,
                                        bool canRerollDice,
                                        ref int __result)
            {
                List<FeatureDefinition> features = new List<FeatureDefinition>();
                __instance.EnumerateFeaturesToBrowse<NewFeatureDefinitions.IModifyDiceRollValue>(features, (Dictionary<FeatureDefinition, RuleDefinitions.FeatureOrigin>)null);
                foreach (NewFeatureDefinitions.IModifyDiceRollValue f in features)
                {
                    __result = f.processDiceRoll(context, __result, __instance);
                }
                if (context == RuleDefinitions.RollContext.AttackDamageValueRoll || context == RuleDefinitions.RollContext.MagicDamageValueRoll)
                {
                    __instance.ProcessConditionsMatchingInterruption((RuleDefinitions.ConditionInterruption)ExtendedEnums.ExtraConditionInterruption.RollsForDamage, 0);
                }
            }
        }

        //add IConditionImmunity support for custom condition immunity effects
        [HarmonyPatch(typeof(RulesetActor), "IsImmuneToCondition")]
        class RulesetActor_IsImmuneToCondition
        {
            internal static void Postfix(RulesetActor __instance,
                                         string conditionDefinitionName,
                                         ref bool __result)
            {
                if (__result)
                {
                    return;
                }
                var condition = DatabaseRepository.GetDatabase<ConditionDefinition>().GetElement(conditionDefinitionName, false);
                if (condition == null)
                {
                    return;
                }

                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IConditionImmunity>(__instance);
                foreach (var f in features)
                {
                    if (f.isImmune(__instance, condition))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }

        //support for IApplyEffectOnConditionRemoval that should trigeer effects on condition removal
        [HarmonyPatch(typeof(RulesetActor), "RemoveCondition")]
        class RulesetCharacter_RemoveCondition
        {
            internal static bool Prefix(RulesetActor __instance,
                                        RulesetCondition rulesetCondition, 
                                        ref bool refresh,
                                        bool showGraphics)
            {

                if (NewFeatureDefinitions.ConditionsData.no_refresh_conditions.Contains(rulesetCondition.ConditionDefinition))
                {
                    refresh = false;
                }
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IApplyEffectOnConditionRemoval>(__instance);

                foreach (var f in features)
                {
                    f.processConditionRemoval(__instance, rulesetCondition.ConditionDefinition);
                }
                return true;
            }
        }

        //Prevent refresh of character when applying certain conditions, to avoid losing count of its number of attacks (unclear if it is still necessary in 1.2.x)
        [HarmonyPatch(typeof(RulesetActor), "AddConditionOfCategory")]
        class AddConditionOfCategory_AddConditionOfCategory
        {
            internal static bool Prefix(RulesetActor __instance,
                                        string category,
                                        RulesetCondition newCondition,
                                        ref bool refresh)
            {
                if (NewFeatureDefinitions.ConditionsData.no_refresh_conditions.Contains(newCondition.ConditionDefinition))
                {
                    refresh = false;
                }
                return true;
            }


            internal static void Postfix(RulesetActor __instance,
                                        string category,
                                        RulesetCondition newCondition,
                                        bool refresh)
            {
                var caster = RulesetEntity.GetEntity<RulesetCharacter>(newCondition.sourceGuid);
                if (caster != null)
                {
                    var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.ICasterApplyEffectOnConditionApplication>(caster);
                    foreach (var f in features)
                    {
                        f.processCasterConditionApplication(caster, __instance, newCondition);
                    }

                    var features2 = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.ITargetApplyEffectOnConditionApplication>(__instance);
                    foreach (var f in features2)
                    {
                        f.processTargetConditionApplication(caster, __instance, newCondition);
                    }
                }
            }
        }



        //refresh attack modes after spell cast in case some features give extra attacks after using spell/power
        [HarmonyPatch(typeof(RulesetActor), "ProcessConditionsMatchingInterruption")]
        class AddConditionOfCategory_ProcessConditionsMatchingInterruption
        {
            internal static void Postfix(RulesetActor __instance,
                                        RuleDefinitions.ConditionInterruption interruption, int amount)
            {

                if (interruption == RuleDefinitions.ConditionInterruption.UsePowerExecuted
                    || interruption == RuleDefinitions.ConditionInterruption.CastSpellExecuted)
                {
                    (__instance as RulesetCharacter)?.RefreshAttackModes();
                }
            }
        }


        [HarmonyPatch(typeof(RulesetActor), "ModulateSustainedDamage")]
        class RulesetActor_ModulateSustainedDamage
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var modulate_sustained_damage = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("ModulateSustainedDamage"));

                codes[modulate_sustained_damage] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                                  new Func<IDamageAffinityProvider, string, float, List<string>, string, ulong, float>(maybeModulateSustainedDamage).Method
                                                                                  );
                codes.Insert(modulate_sustained_damage, new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_3));
                return codes.AsEnumerable();
            }

            static float maybeModulateSustainedDamage(IDamageAffinityProvider affinityProvider,
                                                      string damageType,
                                                      float multiplier,
                                                      List<string> sourceTags,
                                                      string ancestryDamageType,
                                                      ulong sourceGuid)
            {
                RulesetEntity rulesetEntity = (RulesetEntity)null;
                ServiceRepository.GetService<IRulesetEntityService>().TryGetEntityByGuid(sourceGuid, out rulesetEntity);
                RulesetCharacter caster = rulesetEntity as RulesetCharacter;

                if (caster != null)
                {
                    var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IIgnoreDamageAffinity>(caster);
                    foreach (var f in features)
                    {
                        if (f.canIgnoreDamageAffinity(affinityProvider, damageType))
                        {
                            return multiplier;
                        }
                    }
                }

                return affinityProvider.ModulateSustainedDamage(damageType, multiplier, sourceTags, ancestryDamageType);
            }
        }

    }
}
