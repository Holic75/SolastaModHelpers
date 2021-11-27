using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class CharacterBuildingManagerPatcher
    {
        //Support for custom bonus cantrips known and spells known number increase features
        //fix vanilla bonus cantrip features not to acount against total number of cantrips character knows
        class CharacterBuildingManagerApplyFeatureCastSpellPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "ApplyFeatureCastSpell")]
            internal static class CharacterBuildingManager_ApplyFeatureCastSpell_Patch
            {
                internal static void Postfix(CharacterBuildingManager __instance, FeatureDefinition feature)
                {
                    var hero = __instance.HeroCharacter;
                    var feature_cast_spell = feature as FeatureDefinitionCastSpell;
                    if (hero == null || feature_cast_spell == null)
                    {
                        return;
                    }

                    CharacterClassDefinition class_origin;
                    CharacterRaceDefinition race_origin;
                    FeatDefinition feat_origin;
                    hero.LookForFeatureOrigin(feature_cast_spell, out race_origin, out class_origin, out feat_origin);
                    CharacterClassDefinition current_class;
                    int current_level;
                    __instance.GetLastAssignedClassAndLevel(out current_class, out current_level);


                    int bonus_known_spells = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IKnownSpellNumberIncrease>(__instance.HeroCharacter)
                                                                         .Aggregate(0, (old, next) => old += next.getKnownSpellsBonus(__instance, hero, feature_cast_spell));
                    int bonus_known_cantrips = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IKnownSpellNumberIncrease>(__instance.HeroCharacter)
                                                     .Aggregate(0, (old, next) => old += next.getKnownCantripsBonus(__instance, hero, feature_cast_spell));

                    if (current_class == class_origin)
                    {
                        //fix vanilla bonus cantrip features not to acount against total number of cantrips character knows
                        bonus_known_cantrips += Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinitionBonusCantrips>(__instance.HeroCharacter)
                                                         .Aggregate(0, (old, next) => old += next.bonusCantrips.Count()) - __instance.bonusCantrips.Count;
                    }

                    __instance.tempAcquiredSpellsNumber = __instance.tempAcquiredSpellsNumber + bonus_known_spells;
                    __instance.tempAcquiredCantripsNumber = __instance.tempAcquiredCantripsNumber + bonus_known_cantrips;
                    return;
                }
            }
        }

        //Reset extra bonus spells known, when user unselects a subclass
        class CharacterBuildingManagerGrantFeaturesPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignLastSubclass")]
            internal static class CharacterBuildingManager_UnassignLastSubclass_Patch
            {
                internal static bool Prefix(CharacterBuildingManager __instance)
                {
                    CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
                        .CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch.
                            correctNumberOfSpellsKnown(__instance, 
                                                        Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IKnownSpellNumberIncrease>(__instance.heroCharacter).OfType<FeatureDefinition>().ToList(),
                                                        false,
                                                        -1);
                    return true;
                }
            }
        }


        class CharacterBuildingManagerTrainFeatPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "TrainFeat")]
            internal static class CharacterBuildingManager_TrainFeat_Patch
            {
                internal static void Postfix(CharacterBuildingManager __instance, FeatDefinition feat)
                {
                    CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
                        .CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch.
                            correctNumberOfSpellsKnown(__instance,
                                    feat.features.OfType<NewFeatureDefinitions.IKnownSpellNumberIncrease>().OfType<FeatureDefinition>().ToList(),
                                    false,
                                    1);
                }
            }
        }


        class CharacterBuildingManagerUnTrainFeatPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "UntrainFeat")]
            internal static class CharacterBuildingManager_UntrainFeat_Patch
            {
                internal static void Postfix(CharacterBuildingManager __instance, FeatDefinition feat)
                {
                    CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
                        .CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch.
                            correctNumberOfSpellsKnown(__instance,
                                    feat.features.OfType<NewFeatureDefinitions.IKnownSpellNumberIncrease>().OfType<FeatureDefinition>().ToList(),
                                    false,
                                    -1);
                }
            }
        }


        class CharacterBuildingManagerUnTrainFeatsPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "UntrainFeats")]
            internal static class CharacterBuildingManager_UntrainFeats_Patch
            {
                internal static void Prefix(CharacterBuildingManager __instance, string tag)
                {
                    if (!__instance.trainedFeats.ContainsKey(tag))
                    {
                        return;
                    }
                    var feats = __instance.trainedFeats[tag];
                    foreach (var f in feats)
                    {
                        CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
                            .CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch.
                                correctNumberOfSpellsKnown(__instance,
                                        f.features.OfType<NewFeatureDefinitions.IKnownSpellNumberIncrease>().OfType<FeatureDefinition>().ToList(),
                                        false,
                                        -1);
                    }
                }
            }
        }

        //Support for features granting extra known spells
        //correct bonus number of spells  known for the features granted by subclass at level 1 
        class CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "BrowseGrantedFeaturesHierarchically")]
            internal static class CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch
            {
                internal static bool Prefix(CharacterBuildingManager __instance, List<FeatureDefinition> grantedFeatures, string tag)
                {
                    grantSpells(__instance, grantedFeatures);
                    if (tag.Contains("06Subclass"))
                    {
                        correctNumberOfSpellsKnown(__instance, grantedFeatures, true);
                    }
                    return true;
                }

                //correct bonus number of spells  known for the features granted by subclass at level 1 
                //Since at this time spell repertoires are not yet created, so they will not be applied until next lvl up
                internal static void correctNumberOfSpellsKnown(CharacterBuildingManager __instance, List<FeatureDefinition> grantedFeatures, bool only_missing_features, int multiplier = 1)
                {                 
                    var bonus_known_spells_features = grantedFeatures.OfType<NewFeatureDefinitions.IKnownSpellNumberIncrease>();

                    foreach (var f in bonus_known_spells_features)
                    {
                        if (__instance.pointPoolStacks.ContainsKey(HeroDefinitions.PointsPoolType.Cantrip))
                        {
                            var cantrip_pools = __instance.pointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip];
                            foreach (var cp in cantrip_pools.activePools)
                            {
                                var sp_features = __instance.heroCharacter.activeFeatures[cp.Key].OfType<FeatureDefinitionCastSpell>();
                                if (only_missing_features)
                                    sp_features = sp_features.Where(sp => !__instance.heroCharacter.spellRepertoires.Any(sr => sr.SpellCastingFeature == sp));
                                foreach (var sp_f in sp_features)
                                {
                                    var bonus = f.getKnownCantripsBonus(__instance, __instance.heroCharacter, sp_f) * multiplier;
                                    cp.Value.RemainingPoints += bonus;
                                    cp.Value.MaxPoints += bonus;
                                }
                            }
                        }

                        if (__instance.pointPoolStacks.ContainsKey(HeroDefinitions.PointsPoolType.Spell))
                        {
                            var spell_pools = __instance.pointPoolStacks[HeroDefinitions.PointsPoolType.Spell];
                            foreach (var cp in spell_pools.activePools)
                            {
                                var sp_features = __instance.heroCharacter.activeFeatures[cp.Key].OfType<FeatureDefinitionCastSpell>();
                                if (only_missing_features)
                                    sp_features = sp_features.Where(sp => !__instance.heroCharacter.spellRepertoires.Any(sr => sr.SpellCastingFeature == sp));
                                foreach (var sp_f in sp_features)
                                {
                                    var bonus = f.getKnownSpellsBonus(__instance, __instance.heroCharacter, sp_f) * multiplier;
                                    cp.Value.RemainingPoints += bonus;
                                    cp.Value.MaxPoints += bonus;
                                }
                            }
                        }
                    }
                }

                static void grantSpells(CharacterBuildingManager __instance, List<FeatureDefinition> grantedFeatures)
                {
                    var features = grantedFeatures.OfType<NewFeatureDefinitions.IGrantKnownSpellsOnLevelUp>().ToList();
                    features.AddRange(Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IGrantKnownSpellsOnLevelUp>(__instance.HeroCharacter));

                    CharacterClassDefinition current_class;
                    int current_level;
                    __instance.GetLastAssignedClassAndLevel(out current_class, out current_level);

                    foreach (var f in features)
                    {
                        f.maybeGrantSpellsOnLevelUp(__instance);
                    }

                    return;
                }
            }
        }
    }
}
