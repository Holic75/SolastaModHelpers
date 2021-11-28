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
        class CharacterBuildingManagerUnassignLastSubclasssPatcher
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


        class CharacterBuildingManagerEnumerateKnownAndAcquiredSpellsPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "EnumerateKnownAndAcquiredSpells")]
            internal static class CharacterBuildingManager_EnumerateKnownAndAcquiredSpells_Patch
            {
                internal static void Postfix(CharacterBuildingManager __instance, ref List<SpellDefinition> __result)
                {
                    var spells = new HashSet<SpellDefinition>(__result);

                    foreach (RulesetSpellRepertoire spellRepertoire in __instance.HeroCharacter.SpellRepertoires)
                    {
                        if (spellRepertoire.SpellCastingFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.WholeList)
                        {
                            foreach (var sl in spellRepertoire.SpellCastingFeature.spellListDefinition.spellsByLevel)
                            {
                                if (sl.level == 0)
                                {
                                    continue;
                                }
                                foreach (var s in sl.spells)
                                {
                                    if (!spells.Contains(s))
                                    {
                                        spells.Add(s);
                                    }
                                }
                            }
                        }
                    }
                    __result.AddRange(spells);
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
                    CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
                        .CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch.
                            correctNumberOfSpellsKnown(__instance,
                                    feat.features.OfType<NewFeatureDefinitions.IKnownSpellNumberIncrease>().OfType<FeatureDefinition>().ToList(),
                                    true,
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

                    CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
                        .CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch.
                            correctNumberOfSpellsKnown(__instance,
                                    feat.features.OfType<NewFeatureDefinitions.IKnownSpellNumberIncrease>().OfType<FeatureDefinition>().ToList(),
                                    true,
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
                        CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
                            .CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch.
                                correctNumberOfSpellsKnown(__instance,
                                        f.features.OfType<NewFeatureDefinitions.IKnownSpellNumberIncrease>().OfType<FeatureDefinition>().ToList(),
                                        true,
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
                    addEmptySpellpointPools(__instance);
                    var bonus_known_spells_features = grantedFeatures.OfType<NewFeatureDefinitions.IKnownSpellNumberIncrease>();

                    foreach (var f in bonus_known_spells_features)
                    {
                        if (__instance.pointPoolStacks.ContainsKey(HeroDefinitions.PointsPoolType.Cantrip))
                        {
                            var cantrip_pools = __instance.pointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip];
                            foreach (var cp in cantrip_pools.activePools)
                            {
                                List<FeatureDefinitionCastSpell> features_cast_spell = new List<FeatureDefinitionCastSpell>(); ;
                                if (only_missing_features)
                                {
                                    if (!__instance.heroCharacter.activeFeatures.ContainsKey(cp.Key))
                                    {
                                        continue;
                                    }
                                    var sp_features = __instance.heroCharacter.activeFeatures[cp.Key].OfType<FeatureDefinitionCastSpell>();
                                    sp_features = sp_features.Where(sp => !__instance.heroCharacter.spellRepertoires.Any(sr => sr.SpellCastingFeature == sp));
                                    features_cast_spell = sp_features.ToList();
                                }
                                else
                                {
                                    features_cast_spell = __instance.heroCharacter.spellRepertoires.Select(sr => sr.SpellCastingFeature).ToList();
                                }
                                foreach (var sp_f in features_cast_spell)
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
                                List<FeatureDefinitionCastSpell> features_cast_spell = new List<FeatureDefinitionCastSpell>(); ;
                                if (only_missing_features)
                                {
                                    if (!__instance.heroCharacter.activeFeatures.ContainsKey(cp.Key))
                                    {
                                        continue;
                                    }
                                    var sp_features = __instance.heroCharacter.activeFeatures[cp.Key].OfType<FeatureDefinitionCastSpell>();
                                    sp_features = sp_features.Where(sp => !__instance.heroCharacter.spellRepertoires.Any(sr => sr.SpellCastingFeature == sp));
                                    features_cast_spell = sp_features.ToList();
                                }
                                else
                                {
                                    features_cast_spell = __instance.heroCharacter.spellRepertoires.Select(sr => sr.SpellCastingFeature).ToList();
                                }
                                foreach (var sp_f in features_cast_spell)
                                {
                                    var bonus = f.getKnownSpellsBonus(__instance, __instance.heroCharacter, sp_f) * multiplier;
                                    cp.Value.RemainingPoints += bonus;
                                    cp.Value.MaxPoints += bonus;
                                }
                            }
                        }
                    }

                    removeEmptySpellpointPools(__instance);
                }


                static void addEmptySpellpointPools(CharacterBuildingManager __instance)
                {
                    var features = __instance.HeroCharacter.SpellRepertoires.Select(sr => sr.SpellCastingFeature).ToList();
                    CharacterClassDefinition lastClassDefinition = (CharacterClassDefinition)null;
                    int level = 0;
                    __instance.GetLastAssignedClassAndLevel(out lastClassDefinition, out level);

                    var features_to_get_from_class = lastClassDefinition.featureUnlocks.Where(fu => fu.level == level).Select(f => f.featureDefinition).OfType<FeatureDefinitionCastSpell>();
                    features.AddRange(features_to_get_from_class);

                    foreach (var f in features)
                    {
                        string str = string.Empty;
                        if (f.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Class)
                        {
                            __instance.GetLastAssignedClassAndLevel(out lastClassDefinition, out level);
                            str = AttributeDefinitions.GetClassTag(lastClassDefinition, level);
                        }
                        else if (f.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Subclass)
                        {
                            CharacterSubclassDefinition classesAndSubclass = __instance.heroCharacter.ClassesAndSubclasses[lastClassDefinition];
                            str = AttributeDefinitions.GetSubclassTag(lastClassDefinition, level, classesAndSubclass);
                        }
                        else if (f.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Race)
                            continue;

                        var pools = new List<HeroDefinitions.PointsPoolType>() { HeroDefinitions.PointsPoolType.Cantrip, HeroDefinitions.PointsPoolType.Spell };
                        foreach (var p in pools)
                        {
                            if (__instance.pointPoolStacks.ContainsKey(p)
                                && !__instance.pointPoolStacks[p].activePools.ContainsKey(str))
                            {
                                __instance.SetPointPool(p, str, 1);
                                __instance.pointPoolStacks[p].activePools[str].remainingPoints = 0;
                                __instance.pointPoolStacks[p].activePools[str].maxPoints = 0;
                            }
                        }
                    }
                }



                static void removeEmptySpellpointPools(CharacterBuildingManager __instance)
                {
                    var pools = new List<HeroDefinitions.PointsPoolType>() { HeroDefinitions.PointsPoolType.Cantrip, HeroDefinitions.PointsPoolType.Spell };

                    foreach (var p in pools)
                    {
                        if (__instance.pointPoolStacks.ContainsKey(p))
                        {
                            foreach (var k in __instance.pointPoolStacks[p].activePools.Keys.ToArray())
                            {
                                if (__instance.pointPoolStacks[p].activePools[k].remainingPoints <= 0)
                                {
                                    __instance.pointPoolStacks[p].activePools.Remove(k);
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
