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
                if (context == RuleDefinitions.RollContext.DamageValueRoll)
                {
                    __instance.ProcessConditionsMatchingInterruption((RuleDefinitions.ConditionInterruption)ExtendedEnums.ExtraConditionInterruption.RollsForDamage, 0);
                }
            }
        }


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
        }

    }
}
