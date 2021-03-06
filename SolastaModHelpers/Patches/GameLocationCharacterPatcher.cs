using HarmonyLib;
using SolastaModHelpers.NewFeatureDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA;

namespace SolastaModHelpers.Patches
{
    class GameLocationCharacterPatcher
    {
        [HarmonyPatch(typeof(GameLocationCharacter), "StartBattleTurn")]
        internal static class GameLocationCharacter_StartBattleTurn_Patch
        {
            internal static void Postfix(GameLocationCharacter __instance)
            {
                if (!__instance.Valid)
                {
                    return;
                }
                var hero_character = __instance.RulesetCharacter;
                if (hero_character != null)
                {
                    var features = Helpers.Accessors.extractFeaturesHierarchically<IApplyEffectOnTurnStart>(hero_character);
                    foreach (var f in features)
                    {
                        f.processTurnStart(__instance);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(GameLocationCharacter), "EndBattleTurn")]
        internal static class GameLocationCharacter_EndBattleTurn_Patch
        {
            internal static void Postfix(GameLocationCharacter __instance)
            {
                if (!__instance.Valid)
                {
                    return;
                }
                var hero_character = __instance.RulesetCharacter as RulesetCharacter;
                if (hero_character != null)
                {
                    var features = Helpers.Accessors.extractFeaturesHierarchically<IApplyEffectOnTurnEnd>(hero_character);
                    foreach (var f in features)
                    {
                        f.processTurnEnd(__instance);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(GameLocationCharacter), "EndBattle")]
        internal static class GameLocationCharacter_EndBattle_Patch
        {
            internal static void Postfix(GameLocationCharacter __instance)
            {
                if (!__instance.Valid)
                {
                    return;
                }
                var hero_character = __instance.RulesetCharacter as RulesetCharacter;
                if (hero_character != null)
                {
                    var features = Helpers.Accessors.extractFeaturesHierarchically<IApplyEffectOnBattleEnd>(hero_character);
                    foreach (var f in features)
                    {
                        f.processBattleEnd(__instance);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(GameLocationCharacter), "ComputeAbilityCheckActionModifier")]
        internal static class GameLocationCharacter_ComputeAbilityCheckActionModifier_Patch
        {
            internal static bool Prefix(GameLocationCharacter __instance,
                                        string abilityScoreName,
                                        string proficiencyName,
                                        ActionModifier actionModifier,
                                        RuleDefinitions.AbilityCheckContext context)
            {
                __instance.RulesetCharacter.EnumerateFeaturesToBrowse<NewFeatureDefinitions.AbilityCheckAffinityUnderRestriction>(__instance.RulesetCharacter.FeaturesToBrowse, __instance.RulesetCharacter.FeaturesOrigin);
                foreach (NewFeatureDefinitions.AbilityCheckAffinityUnderRestriction key in __instance.RulesetCharacter.FeaturesToBrowse)
                {
                    if (key.canBeUsed(__instance.RulesetCharacter))
                    {
                        key.feature.ComputeAbilityCheckModifier(abilityScoreName, proficiencyName, actionModifier, __instance.RulesetCharacter.FeaturesOrigin[key], context, __instance.lightingState);
                    }
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(GameLocationCharacter), "FinishMoveTo")]
        class GameLocationCharacter_FinishMoveTo
        {
            internal static bool Prefix(GameLocationCharacter __instance, int3 destination)
            {
                if (__instance.locationPosition == destination)
                {
                    return true;
                }
                var hero_character = __instance.RulesetCharacter as RulesetCharacter;
                if (hero_character != null)
                {
                    var features = Helpers.Accessors.extractFeaturesHierarchically<IApplyEffectOnTargetMoved>(hero_character);
                    foreach (var f in features)
                    {
                        f.processTargetMoved(hero_character);
                    }
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(GameLocationCharacter), "GetActionStatus")]
        class GameLocationCharacter_GetActionStatus
        {
            internal static bool Prefix(GameLocationCharacter __instance, ActionDefinitions.Id actionId, ref ActionDefinitions.ActionStatus __result)
            {
                ActionDefinition actionDefinition = ServiceRepository.GetService<IGameLocationActionService>().AllActionDefinitions[actionId];

                var restrictions = ActionData.getActionPrerequisites(actionDefinition);

                foreach (var r in restrictions)
                {
                    if (r.isForbidden(__instance.RulesetCharacter))
                    {
                        __result = ActionDefinitions.ActionStatus.Unavailable;
                        return false;
                    }
                }
                return true;
            }
        }

    }
}
