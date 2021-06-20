using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace SolastaModHelpers.Patches
{
    class SpellRepertoirePatches
    {
        class SpellsByLevelGroupBindLearningPatcher
        {
            [HarmonyPatch(typeof(SpellsByLevelGroup), "BindLearning")]
            internal static class SpellsByLevelGroup_BindLearning_Patch
            {
                internal static bool Prefix(ICharacterBuildingService characterBuildingService,
                                            ref SpellListDefinition spellListDefinition,
                                            List<string> restrictedSchools,
                                            int spellLevel,
                                            SpellBox.SpellBoxChangedHandler spellBoxChanged,
                                            List<SpellDefinition> knownSpells,
                                            string spellTag,
                                            bool canAcquireSpells)
                {
                    var hero = characterBuildingService.HeroCharacter;
                    if (hero == null)
                    {
                        return true;
                    }

                    var extra_spell_list = hero.ActiveFeatures.Values.Aggregate(new List<NewFeatureDefinitions.IReplaceSpellList>(),
                                                                               (old, next) =>
                                                                               {
                                                                                   old.AddRange(Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IReplaceSpellList>(next));
                                                                                   return old;
                                                                               }).Select(rs => rs.getSpelllist(characterBuildingService)).FirstOrDefault(s => s != null);

                    if (extra_spell_list == null)
                    {
                        return true;
                    }

                    spellListDefinition = extra_spell_list;
                    return true;
                }
            }
        }


        class CharacterBuildingManagerSetPointPoolPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "SetPointPool")]
            internal static class CharacterBuildingManager_SetPointPool_Patch
            {
                internal static bool Prefix(CharacterBuildingManager __instance, HeroDefinitions.PointsPoolType pointPoolType, string tag, ref int maxNumber)
                {
                    if (pointPoolType != HeroDefinitions.PointsPoolType.Spell)
                    {
                        return true;
                    }
                    var hero = __instance.HeroCharacter;
                    if (hero == null)
                    {
                        return true;
                    }
                    int bonus_known_spells = __instance.HeroCharacter.ActiveFeatures.Values.Aggregate(new List<NewFeatureDefinitions.IKnownSpellNumberIncrease>(),
                                                                          (old, next) =>
                                                                          {
                                                                              old.AddRange(Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IKnownSpellNumberIncrease>(next));
                                                                              return old;
                                                                          }
                                                                          ).Aggregate(0, (old, next) => old += next.getKnownSpellsBonus(hero));

                    maxNumber = maxNumber + bonus_known_spells;
                    return true;
                }
            }
        }


        class CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "BrowseGrantedFeaturesHierarchically")]
            internal static class CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch
            {
                internal static bool Prefix(CharacterBuildingManager __instance, List<FeatureDefinition> grantedFeatures, string tag)
                {
                    var features = grantedFeatures.OfType<NewFeatureDefinitions.GrantSpells>().ToList();
                    features = __instance.HeroCharacter.ActiveFeatures.Values.Aggregate(features,
                                                                                              (old, next) =>
                                                                                              {
                                                                                                  old.AddRange(Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.GrantSpells>(next));
                                                                                                  return old;
                                                                                              }
                                                                                              );

                    CharacterClassDefinition current_class;
                    int current_level;
                    __instance.GetLastAssignedClassAndLevel(out current_class, out current_level);

                    HashSet<SpellDefinition> spells = new HashSet<SpellDefinition>();
                    foreach (var f in features)
                    {
                        if (f.spellcastingClass != current_class)
                        {
                            continue;
                        }

                        foreach (var sg in f.spellGroups)
                        {
                            if (sg.ClassLevel != current_level)
                            {
                                continue;
                            }

                            foreach (var s in sg.SpellsList)
                            {
                                spells.Add(s);
                            }
                        }
                    }

                    var repertoire = __instance.HeroCharacter.SpellRepertoires.FirstOrDefault(r => r.spellCastingClass == current_class);
                    if (repertoire == null)
                    {
                        return true;
                    }
                    foreach (var s in repertoire.KnownSpells)
                    {
                        if (spells.Contains(s))
                        {
                            spells.Remove(s);
                        }
                    }
                    repertoire.KnownSpells.AddRange(spells);

                    return true;
                }
            }
        }
    }
}
