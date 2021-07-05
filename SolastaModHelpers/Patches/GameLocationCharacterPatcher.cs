﻿using HarmonyLib;
using SolastaModHelpers.NewFeatureDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var hero_character = __instance.RulesetCharacter as RulesetCharacterHero;
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
                var hero_character = __instance.RulesetCharacter as RulesetCharacterHero;
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
                var hero_character = __instance.RulesetCharacter as RulesetCharacterHero;
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
    }
}