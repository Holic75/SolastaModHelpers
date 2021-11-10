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
                 
                    return true;
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    var codes = instructions.ToList();
                    var common_bind = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("CommonBind"));
                    int all_spells_loaded_idx = common_bind - 6;

                    codes.Insert(all_spells_loaded_idx + 1, new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1)); //put characterBuildingService on stack
                    codes.Insert(all_spells_loaded_idx + 2, new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_S, 10)); //put unlearn on stack
                    codes.Insert(all_spells_loaded_idx + 3, new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_S, 4)); //put spell level on stack
                    codes.Insert(all_spells_loaded_idx + 4,
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<List<SpellDefinition>, ICharacterBuildingService, bool, int, List<SpellDefinition>>(overwriteSpellList).Method
                                                                 )
                                );
                    return codes.AsEnumerable();
                }

                static List<SpellDefinition> overwriteSpellList(List<SpellDefinition> original, ICharacterBuildingService characterBuildingService, bool unlearn, int spellLevel)
                {
                    if (unlearn)
                    {
                        return original;
                    }

                    var hero = characterBuildingService.HeroCharacter;
                    if (hero == null)
                    {
                        return original;
                    }

                    var extra_spell_list = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IReplaceSpellList>(hero)
                                                .Select(rs => rs.getSpelllist(characterBuildingService, spellLevel == 0)).FirstOrDefault(s => s != null);
                    if (extra_spell_list == null)
                    {
                        return original;
                    }

                    List<SpellDefinition> new_list = new List<SpellDefinition>();

                    foreach (SpellDefinition andAcquiredSpell in characterBuildingService.EnumerateKnownAndAcquiredSpells(string.Empty))
                    {
                        if (andAcquiredSpell.SpellLevel == spellLevel && !new_list.Contains(andAcquiredSpell))
                            new_list.Add(andAcquiredSpell);
                    }

                    if (extra_spell_list.spellsByLevel.Count > spellLevel)
                    {
                        foreach (var s in extra_spell_list.spellsByLevel[spellLevel].spells)
                        {
                            if (!new_list.Contains(s))
                            new_list.Add(s);
                        }
                    }
                    return new_list;                  
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
                    if (warlock_spellcasting == null || slotLevel == 0 || slotLevel >= warlock_spellcasting.mystic_arcanum_level_start
                        || EnableCombinedSpellCasting)
                    {
                        return true;
                    }

                    var max_slot_level = Math.Min(__instance.MaxSpellLevelOfSpellCastingLevel, warlock_spellcasting.mystic_arcanum_level_start - 1);

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

            [HarmonyPatch(typeof(RulesetSpellRepertoire), "CanUpcastSpell")]
            internal static class RulesetSpellRepertoire_CanUpcastSpell_Patch
            {
                internal static void Postfix(RulesetSpellRepertoire __instance,
                                            SpellDefinition spellDefinition, List<int> availableSlotLevels,
                                            ref bool __result)
                {
                    var warlock_spellcasting = (__instance?.spellCastingFeature as NewFeatureDefinitions.WarlockCastSpell);
                    if (warlock_spellcasting == null || !__result
                        || EnableCombinedSpellCasting)
                    {
                        return;
                    }

                    availableSlotLevels?.RemoveAll(s => s >= warlock_spellcasting.mystic_arcanum_level_start);
                    if (spellDefinition.spellLevel + 1 < warlock_spellcasting.mystic_arcanum_level_start)
                    {
                        int remaining = 0;
                        int max = 0;
                        __instance.GetSlotsNumber(1, out remaining,out max);
                        __result = remaining > 0;
                    }
                    else
                    {
                        __result = false;
                    }
                }
            }



            [HarmonyPatch(typeof(RulesetSpellRepertoire), "GetMaxSlotsNumberOfAllLevels")]
            internal static class RulesetSpellRepertoire_GetMaxSlotsNumberOfAllLevels_Patch
            {
                internal static bool Prefix(RulesetSpellRepertoire __instance, ref int __result)
                {
                    //NOTE: No need to account for mystic arcanum since this method is only used to extract spell slots
                    //for spending them on powers, mystic arcanum slots should not be used this way
                    var warlock_spellcasting = (__instance?.spellCastingFeature as NewFeatureDefinitions.WarlockCastSpell);
                    if (warlock_spellcasting == null || EnableCombinedSpellCasting)
                    {
                        return true;
                    }

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
                    //NOTE: No need to account for mystic arcanum since this method is only used to extract spell slots
                    //for spending them on powers, mystic arcanum slots should not be used this way
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
                    if (warlock_spellcasting == null || spellLevel == 0 || spellLevel >= warlock_spellcasting.mystic_arcanum_level_start
                        || EnableCombinedSpellCasting)
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


        //do not propose to use mystic arcanum slots for reaction powers
        class BuildSlotSubOptionsPatcher
        {
            [HarmonyPatch(typeof(ReactionRequestCastSpell), "BuildSlotSubOptions")]
            internal static class ReactionRequestCastSpell_BuildSlotSubOptions_Patch
            {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    var codes = instructions.ToList();
                    var get_max_spellcasting_level = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("MaxSpellLevelOfSpellCastingLevel"));
                    codes[get_max_spellcasting_level] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldloc_1); //spell level
                    codes.Insert(get_max_spellcasting_level + 1,
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<RulesetSpellRepertoire, int, int>(getMaxSpellLevel).Method
                                                                 )
                                );
                    return codes.AsEnumerable();
                }

                static int getMaxSpellLevel(RulesetSpellRepertoire spell_repertoire, int spell_level)
                {
                    var warlock_spellcasting = spell_repertoire.spellCastingFeature as NewFeatureDefinitions.WarlockCastSpell;
                    if (warlock_spellcasting == null)
                    {
                        return spell_repertoire.MaxSpellLevelOfSpellCastingLevel;
                    }
                    else
                    {
                        if (spell_level >= warlock_spellcasting.mystic_arcanum_level_start)
                        {
                            return Math.Min(spell_level, spell_repertoire.MaxSpellLevelOfSpellCastingLevel);
                        }
                        else
                        {
                            return Math.Min(warlock_spellcasting.mystic_arcanum_level_start - 1, spell_repertoire.MaxSpellLevelOfSpellCastingLevel);
                        }
                    }
                }
            }
        }
    }
}
