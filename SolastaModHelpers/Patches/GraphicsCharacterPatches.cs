using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolastaModHelpers.Patches
{
    class GraphicsCharacterPatches
    {
        //patch allowing to store prerolled saving throw data, that can further be used to determine whether saving throw failed and propose a player to use an ability to reroll/modify it
        [HarmonyPatch(typeof(GraphicsCharacter), "SaveRolled")]
        class GraphicsCharacter_SaveRolled
        {
            internal static bool Prefix(RulesetActor character,
                                        string abilityScoreName,
                                        BaseDefinition sourceDefinition,
                                        RuleDefinitions.RollOutcome outcome,
                                        int saveDC,
                                        int totalRoll,
                                        int saveRoll,
                                        int firstRoll,
                                        int secondRoll,
                                        int rollModifier,
                                        List<RuleDefinitions.TrendInfo> modifierTrends,
                                        List<RuleDefinitions.TrendInfo> advantageTrends,
                                        bool hasHitVisual)
            {
                var game_location_character = Helpers.Misc.findGameLocationCharacter(character as RulesetCharacter);
                if (game_location_character != null)
                {
                    NewFeatureDefinitions.SavingthrowRollsData.storePrerolledData(game_location_character, new NewFeatureDefinitions.SavingthrowRollInfo(totalRoll, saveDC, outcome));
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(GraphicsCharacter), "ResetScale")]
        class GraphicsCharacter_ResetScale
        {
            internal static void Postfix(GraphicsCharacter __instance, ref float __result)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.ModelSizeScaleDefinition>(__instance.rulesetCharacter);

                var race = (__instance.rulesetCharacter as RulesetCharacterHero)?.raceDefinition;

                if (race != null && NewFeatureDefinitions.RaceData.raceScaleMap.ContainsKey(race))
                {
                    __result *= NewFeatureDefinitions.RaceData.raceScaleMap[race];
                }

                foreach (var f in features)
                {
                    __result *= f.scaleFactor;
                }
                __instance.transform.localScale = new Vector3(__result, __result, __result);

            }
        }
    }
}
