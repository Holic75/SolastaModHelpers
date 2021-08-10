using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    public class SpellRepertoirePatches
    {
        public static bool EnableCombinedSpellCasting = false;

        class SpellsByLevelGroupBindLearningPatcher
        {
            [HarmonyPatch(typeof(SpellsByLevelGroup), "BindLearning")]
            internal static class SpellsByLevelGroup_BindLearning_Patch
            {
                internal static bool Prefix(ICharacterBuildingService characterBuildingService,
                                            ref SpellListDefinition spellListDefinition,
                                            ref List<string> restrictedSchools,
                                            int spellLevel,
                                            SpellBox.SpellBoxChangedHandler spellBoxChanged,
                                            ref List<SpellDefinition> knownSpells,
                                            List<SpellDefinition> unlearnedSpells,
                                            string spellTag,
                                            bool canAcquireSpells,
                                            bool unlearn)
                {
                    var hero = characterBuildingService.HeroCharacter;
                    if (hero == null)
                    {
                        return true;
                    }

                    if (unlearn)
                    {
                        var restricted_schools = restrictedSchools;

                        var spell_lists = Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinitionMagicAffinity>(hero).Where(f => f.extendedSpellList != null)
                                                                                            .Select(f => f.extendedSpellList).ToList();
                        spell_lists.Add(spellListDefinition);

                        var spell_set = spell_lists.Aggregate(new HashSet<SpellDefinition>(), (old, next) => 
                        {
                            foreach (var sl in next.spellsByLevel)
                            {
                                foreach (var ss in sl.spells)
                                {
                                    if (restricted_schools.Count == 0 || restricted_schools.Contains(ss.SchoolOfMagic))
                                    {
                                        old.Add(ss);
                                    }
                                }
                            }
                           return old;
                        }
                        );
                        knownSpells = knownSpells.Where(s => spell_set.Contains(s)).ToList();
                        return true;
                    }

                    
                    var extra_spell_list = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IReplaceSpellList>(hero)
                                                                    .Select(rs => rs.getSpelllist(characterBuildingService, spellLevel == 0)).FirstOrDefault(s => s != null);

                    if (extra_spell_list == null)
                    {
                        return true;
                    }

                    spellListDefinition = extra_spell_list;
                    restrictedSchools = new List<string>();
                    return true;
                }
            }
        }

        class RulesetSpellRepertoirePatcher
        {
            [HarmonyPatch(typeof(RulesetSpellRepertoire), "SpendSpellSlot")]
            internal static class RulesetSpellRepertoire_SpendSpellSlot_Patch
            {
                internal static bool Prefix(RulesetSpellRepertoire __instance,
                                           int slotLevel)
                {
                    var warlock_spellcasting = (__instance?.spellCastingFeature as NewFeatureDefinitions.WarlockCastSpell);
                    if (warlock_spellcasting == null || slotLevel == 0 || EnableCombinedSpellCasting)
                    {
                        return true;
                    }

                    var max_slot_level = __instance.MaxSpellLevelOfSpellCastingLevel;

                    for (int i = 1; i <= max_slot_level; i++)
                    {
                        if (!__instance.usedSpellsSlots.ContainsKey(i))
                            __instance.usedSpellsSlots.Add(i, 1);
                        else
                            __instance.usedSpellsSlots[i]++;
                    }
                    __instance.RepertoireRefreshed?.Invoke(__instance);
                    return false;
                }
            }


            [HarmonyPatch(typeof(RulesetSpellRepertoire), "GetMaxSlotsNumberOfAllLevels")]
            internal static class RulesetSpellRepertoire_GetMaxSlotsNumberOfAllLevels_Patch
            {
                internal static bool Prefix(RulesetSpellRepertoire __instance, ref int __result)
                {
                    var warlock_spellcasting = (__instance?.spellCastingFeature as NewFeatureDefinitions.WarlockCastSpell);
                    if (warlock_spellcasting == null || EnableCombinedSpellCasting)
                    {
                        return true;
                    }

                    var max_slot_level = __instance.MaxSpellLevelOfSpellCastingLevel;
                    __result = 0;
                    __instance.spellsSlotCapacities.TryGetValue(1, out __result);
                    return false;
                }
            }


            [HarmonyPatch(typeof(RulesetSpellRepertoire), "GetRemainingSlotsNumberOfAllLevels")]
            internal static class RulesetSpellRepertoire_GetRemainingSlotsNumberOfAllLevels_Patch
            {
                internal static bool Prefix(RulesetSpellRepertoire __instance, ref int __result)
                {
                    var warlock_spellcasting = (__instance?.spellCastingFeature as NewFeatureDefinitions.WarlockCastSpell);
                    if (warlock_spellcasting == null || EnableCombinedSpellCasting)
                    {
                        return true;
                    }

                    __result = 0;
                    int max = 0;
                    int used = 0;
                    __instance.spellsSlotCapacities.TryGetValue(1, out max);
                    __instance.usedSpellsSlots.TryGetValue(1, out used);
                    __result = max - used;

                    return false;
                }
            }


            [HarmonyPatch(typeof(RulesetSpellRepertoire), "GetSlotsNumber")]
            internal static class RulesetSpellRepertoire_GetSlotsNumber_Patch
            {
                internal static bool Prefix(RulesetSpellRepertoire __instance,
                                           int spellLevel, ref int remaining, ref int max)
                {
                    var warlock_spellcasting = (__instance?.spellCastingFeature as NewFeatureDefinitions.WarlockCastSpell);
                    if (warlock_spellcasting == null || spellLevel == 0 || EnableCombinedSpellCasting)
                    {
                        return true;
                    }

                    var max_slot_level = __instance.MaxSpellLevelOfSpellCastingLevel;
                    remaining = 0;
                    max = 0;
                    if (spellLevel > max_slot_level)
                    {
                        return false;
                    }
                    int used = 0;
                    __instance.usedSpellsSlots.TryGetValue(1, out used);
                    __instance.spellsSlotCapacities.TryGetValue(1, out max);
                    remaining = max - used;

                    return false;
                }
            }
        }
    }
}
