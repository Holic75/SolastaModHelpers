using SolastaModApi;
using SolastaModApi.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.AddressableAssets;
using static FeatureDefinitionAbilityCheckAffinity;
using static FeatureDefinitionSavingThrowAffinity;
using static SpellListDefinition;

namespace SolastaModHelpers.Helpers
{
    public static class DamageTypes
    {
        public static string Cold = "DamageCold";
        public static string Fire = "DamageFire";
        public static string Radiant = "DamageRadiant";
        public static string Force = "DamageForce";
        public static string Psychic = "DamagePsychic";
        public static string Necrotic = "DamageNecrotic";
        public static string Lightning = "DamageLightning";
        public static string Thundering = "DamageThunder";
        public static string Poison = "DamagePoison";
        public static string Acid = "DamageAcid";

        public static string Piercing = "DamagePiercing";
        public static string Bludgeoning = "DamageBludgeoning";
    }


    public static class SpellSchools
    {
        public static string Abjuration = "SchoolAbjuration";
        public static string Conjuration = "SchoolConjuration";
        public static string Evocation = "SchoolEvocation";
        public static string Enchantment = "SchoolEnchantment";
        public static string Divination = "SchoolDivination";
        public static string Transmutation = "SchoolTransmutation";
        public static string Necromancy = "SchoolNecromancy";
        public static string Illusion = "SchoolIllusion";

        public static string[] getAllSchools()
        {
            return typeof(SpellSchools).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).Cast<string>().ToArray();
        }
    }


    public static class Stats
    {
        public static string Strength = "Strength";
        public static string Dexterity = "Dexterity";
        public static string Constitution = "Constitution";
        public static string Wisdom = "Wisdom";
        public static string Intelligence = "Intelligence";
        public static string Charisma = "Charisma";

        public static string[] getAllStats()
        {
            return typeof(Stats).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).Cast<string>().ToArray();
        }

        public static HashSet<string> getAllStatsSet()
        {
            return getAllStats().ToHashSet();
        }

        public static void assertAllStats(IEnumerable<string> stats)
        {
            var all_stats = getAllStatsSet();
            foreach (var s in stats)
            {
                if (!all_stats.Contains(s))
                {
                    throw new System.Exception(s + "is not an Ability");
                }
            }
        }
    }


    public static class ArmorProficiencies
    {
        public static string LigthArmor = "LightArmorCategory";
        public static string MediumArmor = "MediumArmorCategory";
        public static string HeavyArmor = "HeavyArmorCategory";
        public static string Shield = "ShieldCategory";
        public static string HideArmor = "HideArmorType";
        public static string PaddedArmor = "PaddedLeatherType";
        public static string LeatherArmor = "LeatherType";

        public static string[] getAllArmorProficiencies()
        {
            return typeof(ArmorProficiencies).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).Cast<string>().ToArray();
        }

        public static HashSet<string> getAllArmorProficienciesSet()
        {
            return getAllArmorProficiencies().ToHashSet();
        }

        public static void assertAllArmorProficiencies(IEnumerable<string> profs)
        {
            var all_profs = getAllArmorProficienciesSet();
            foreach (var p in profs)
            {
                if (!all_profs.Contains(p))
                {
                    throw new System.Exception(p + "is not an Armor Proficiency");
                }
            }
        }
    }


    public static class WeaponProficiencies
    {
        public static string ShortSword = "ShortswordType";
        public static string Club = "ClubType";
        public static string Dagger = "DaggerType";
        public static string Dart = "DartType";
        public static string Handaxe = "HandaxeType";
        public static string Javelin = "JavelinType";
        public static string QuarterStaff = "QuarterstaffType";
        public static string Mace = "MaceType";
        public static string Spear = "SpearType";
        public static string LongSword = "LongswordType";
        public static string Unarmed = "UnarmedStrikeType";
        public static string BattleAxe = "BattleaxeType";
        public static string MorningStar = "MorningstarType";
        public static string Rapier = "RapierType";
        public static string Scimitar = "ScimitarType";
        public static string Warhammer = "WarhammerType";
        public static string GreatSword = "GreatswordType";
        public static string GreatAxe = "GreataxeType";
        public static string Maul = "MaulType";

        public static string Longbow = "LongbowType";
        public static string Shortbow = "ShortbowType";
        public static string LightCrossbow = "LightCrossbowType";
        public static string HeavyCrossbow = "HeavyCrossbowType";

        public static string Simple = "SimpleWeaponCategory";
        public static string Martial = "MartialWeaponCategory";

        public static string[] getAllWeaponProficiencies()
        {
            return typeof(WeaponProficiencies).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).Cast<string>().ToArray();
        }

        public static HashSet<string> getAllArmorProficienciesSet()
        {
            return getAllWeaponProficiencies().ToHashSet();
        }

        public static void assertAllWeaponProficiencies(IEnumerable<string> profs)
        {
            var all_profs = getAllWeaponProficiencies();
            foreach (var p in profs)
            {
                if (!all_profs.Contains(p))
                {
                    throw new System.Exception(p + " is not a Weapon Proficiency");
                }
            }
        }
    }


    public static class Conditions
    {
        public static string Charmed = "ConditionCharmed";
        public static string Frightened = "ConditionFrightened";


        public static string[] getAllConditions()
        {
            return typeof(Conditions).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).Cast<string>().ToArray();
        }

        public static HashSet<string> getAllConditionsSet()
        {
            return getAllConditions().ToHashSet();
        }

        public static void assertAllConditions(IEnumerable<string> conditions)
        {
            var all_conditions = getAllConditionsSet();
            foreach (var c in conditions)
            {
                if (!all_conditions.Contains(c))
                {
                    throw new System.Exception(c + " is not a Condition");
                }
            }
        }
    }

    public static class Skills
    {
        public static string Acrobatics = "Acrobatics";
        public static string Arcana = "Arcana";
        public static string AnimalHandling = "AnimalHandling";
        public static string Athletics = "Athletics";
        public static string Deception = "Deception";
        public static string History = "History";
        public static string Insight = "Insight";
        public static string Intimidation = "Intimidation";
        public static string Investigation = "Investigation";
        public static string Medicine = "Medecine";
        public static string Nature = "Nature";
        public static string Perception = "Perception";
        public static string Perfromance = "Performance";
        public static string Persuasion = "Persuasion";
        public static string Religion = "Religion";
        public static string SleightOfHand = "SleightOfHand";
        public static string Stealth = "Stealth";
        public static string Survival = "Survival";

        public static Dictionary<string, string> skill_stat_map = new Dictionary<string, string>
        {
            {Acrobatics, Stats.Dexterity },
            {Arcana, Stats.Intelligence },
            {AnimalHandling, Stats.Wisdom },
            {Athletics, Stats.Strength },
            {Deception, Stats.Charisma },
            {History, Stats.Intelligence },
            {Insight, Stats.Wisdom },
            {Intimidation, Stats.Charisma },
            {Investigation, Stats.Intelligence },
            {Medicine, Stats.Wisdom },
            {Nature, Stats.Intelligence },
            {Perception, Stats.Wisdom },
            {Perfromance, Stats.Charisma },
            {Persuasion, Stats.Charisma },
            {Religion, Stats.Wisdom },
            {SleightOfHand, Stats.Dexterity },
            {Stealth, Stats.Dexterity },
            {Survival, Stats.Wisdom }
        };

        public static string[] getAllSkills()
        {
            return typeof(Skills).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).Where(f => f is string).Cast<string>().ToArray();
        }

        public static HashSet<string> getAllSkillsSet()
        {
            return getAllSkills().ToHashSet();
        }

        public static void assertAllSkills(IEnumerable<string> skills)
        {
            var all_stats = getAllSkillsSet();
            foreach (var s in skills)
            {
                if (!all_stats.Contains(s))
                {
                    throw new System.Exception(s + " is not a Skill");
                }
            }
        }
    }

    public static class Tools
    {
        public static string ScrollKit = "ScrollKitType";
        public static string EnchantingTool = "EnchantingToolType";
        public static string SmithTool = "ArtisanToolSmithToolsType";
        public static string ThievesTool = "ThievesToolsType";
        public static string HerbalismKit = "HerbalismKitType";
        public static string PoisonerKit = "PoisonersKitType";
        public static string Lyre = "MusicalInstrumentLyreType";

        public static string[] getAllTools()
        {
            return typeof(Tools).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).Cast<string>().ToArray();
        }

        public static HashSet<string> getAllToolsSet()
        {
            return getAllTools().ToHashSet();
        }

        public static void assertAllTools(IEnumerable<string> tools)
        {
            var all_stats = getAllTools();
            foreach (var s in tools)
            {
                if (!all_stats.Contains(s))
                {
                    throw new System.Exception(s + "is not a Tool");
                }
            }
        }
    }



    public class ProficiencyBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionProficiency>
    {
        protected ProficiencyBuilder(string name, string guid, string title_string, string description_string, FeatureDefinitionProficiency base_feature, params string[] proficiencies)
                : base(base_feature, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }
            Definition.Proficiencies.Clear();
            Definition.Proficiencies.AddRange(proficiencies);
        }


        public static FeatureDefinitionProficiency CreateArmorProficiency(string name, string guid, string title_string, string description_string,
                                                                          params string[] proficiencies)
        {
            ArmorProficiencies.assertAllArmorProficiencies(proficiencies);
            return new ProficiencyBuilder(name, guid, title_string, description_string, DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyFighterArmor, proficiencies).AddToDB();
        }


        public static FeatureDefinitionProficiency CreateWeaponProficiency(string name, string guid, string title_string, string description_string,
                                                                  params string[] proficiencies)
        {
            WeaponProficiencies.assertAllWeaponProficiencies(proficiencies);
            return new ProficiencyBuilder(name, guid, title_string, description_string, DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyFighterWeapon, proficiencies).AddToDB();
        }


        public static FeatureDefinitionProficiency CreateSavingthrowProficiency(string name, string guid, params string[] stats)
        {
            Stats.assertAllStats(stats);
            return new ProficiencyBuilder(name, guid, "", "", DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyRogueSavingThrow, stats).AddToDB();
        }


        public static FeatureDefinitionProficiency CreateToolsProficiency(string name, string guid, string title_string, params string[] tools)
        {
            Tools.assertAllTools(tools);
            return new ProficiencyBuilder(name, guid, title_string, "", DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyRogueTools, tools).AddToDB();
        }


        public static FeatureDefinitionProficiency CreateSkillsProficiency(string name, string guid, string title_string, string description_string, params string[] skills)
        {
            Skills.assertAllSkills(skills);
            return new ProficiencyBuilder(name, guid, title_string, description_string, DatabaseHelper.FeatureDefinitionProficiencys.ProficiencySpySkills, skills).AddToDB();
        }


        public static FeatureDefinitionProficiency createCopy(string name, string guid, string new_title_string, string new_description_string, FeatureDefinitionProficiency base_feature)
        {
            return new ProficiencyBuilder(name, guid, new_title_string, new_description_string, base_feature, base_feature.Proficiencies.ToArray()).AddToDB();
        }
    }


    public class PoolBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionPointPool>
    {
        protected PoolBuilder(string name, string guid, string title_string, string description_string,
                                        FeatureDefinitionPointPool base_feature, HeroDefinitions.PointsPoolType pool_type,
                                        int num_choices, params string[] choices)
                : base(base_feature, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }
            Definition.SetPoolAmount(num_choices);
            Definition.SetPoolType(pool_type);
            Definition.RestrictedChoices.Clear();
            Definition.RestrictedChoices.AddRange(choices);
            Definition.RestrictedChoices.Sort();
        }


        public static FeatureDefinitionPointPool createSkillProficiency(string name, string guid, string new_title_string, string new_description_string, int num_skills, params string[] skills)
        {
            Skills.assertAllSkills(skills);
            return new PoolBuilder(name, guid, new_title_string, new_description_string, DatabaseHelper.FeatureDefinitionPointPools.PointPoolRogueSkillPoints,
                                      HeroDefinitions.PointsPoolType.Skill, num_skills, skills).AddToDB();
        }


        public static FeatureDefinitionPointPool createToolProficiency(string name, string guid, string new_title_string, string new_description_string, int num_tools, params string[] tools)
        {
            Tools.assertAllTools(tools);
            return new PoolBuilder(name, guid, new_title_string, new_description_string, DatabaseHelper.FeatureDefinitionPointPools.PointPoolRogueSkillPoints,
                                      HeroDefinitions.PointsPoolType.Tool, num_tools, tools).AddToDB();
        }
    }


    public class RitualSpellcastingBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionFeatureSet>
    {
        protected RitualSpellcastingBuilder(string name, string guid, string description_string,
                                            RuleDefinitions.RitualCasting ritual_casting_type) : base(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetWizardRitualCasting, name, guid)
        {
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }

            var action_affinity_feature = Definition.FeatureSet[1];

            Definition.FeatureSet.Clear();
            if (ritual_casting_type != RuleDefinitions.RitualCasting.None)
            {
                Definition.FeatureSet.Add(Common.ritual_spellcastings_map[ritual_casting_type]);
            }
            Definition.FeatureSet.Add(action_affinity_feature);
        }

        public static FeatureDefinitionFeatureSet createRitualSpellcasting(string name, string guid, string description_string, RuleDefinitions.RitualCasting ritual_casting_type)
        {
            return new RitualSpellcastingBuilder(name, guid, description_string, ritual_casting_type).AddToDB();
        }
    }


    public class SpelllistBuilder : BaseDefinitionBuilderWithGuidStorage<SpellListDefinition>
    {
        protected SpelllistBuilder(string name, string guid, string title_string, SpellListDefinition base_list, params List<SpellDefinition>[] spells_by_level) : base(DatabaseHelper.SpellListDefinitions.SpellListWizard, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }

            for (int i = 0; i < Definition.SpellsByLevel.Count; i++)
            {
                Definition.SpellsByLevel[i].Spells.Clear();
                if (spells_by_level.Length > i)
                {
                    Definition.SpellsByLevel[i].Spells.AddRange(spells_by_level[i].Where(s => s.Implemented));
                }
            }
        }

        public static SpellListDefinition create9LevelSpelllist(string name, string guid, string title_string, params List<SpellDefinition>[] spells_by_level)
        {
            return new SpelllistBuilder(name, guid, title_string, DatabaseHelper.SpellListDefinitions.SpellListWizard, spells_by_level).AddToDB();
        }


        public static SpellListDefinition createCombinedSpellList(string name, string guid, string title_string, params SpellListDefinition[] spell_lists)
        {
            List<SpellsByLevelDuplet> spells_by_level = new List<SpellsByLevelDuplet>();
            Dictionary<SpellDefinition, int> min_spell_levels = new Dictionary<SpellDefinition, int>();
            for (int i = 0; i < 10; i++)
            {
                spells_by_level.Add(new SpellsByLevelDuplet());
                spells_by_level[i].Level = i;
                spells_by_level[i].Spells = new List<SpellDefinition>();
            }

            foreach (var sl in spell_lists)
            {
                foreach (var sll in sl.SpellsByLevel)
                {
                    foreach (var s in sll.Spells)
                    {
                        if (!min_spell_levels.ContainsKey(s) || min_spell_levels[s] > sll.Level)
                        {
                            min_spell_levels[s] = sll.Level;
                        }
                    }
                }
            }

            foreach (var kv in min_spell_levels)
            {
                spells_by_level[kv.Value].Spells.Add(kv.Key);
            }

            spells_by_level.RemoveAll(e => e.Spells.Count == 0);

            foreach (var sl in spells_by_level)
            {
                sl.Spells.Sort((a, b) => a.name.CompareTo(b.name));
            }

            return create9LevelSpelllist(name, guid, title_string, spells_by_level.Select(s => s.Spells).ToArray());
        }


        public static SpellListDefinition createCombinedSpellList(string name, string guid, string title_string, Predicate<SpellDefinition> predicate, params SpellListDefinition[] spell_lists)
        {
            List<SpellsByLevelDuplet> spells_by_level = new List<SpellsByLevelDuplet>();
            Dictionary<SpellDefinition, int> min_spell_levels = new Dictionary<SpellDefinition, int>();
            for (int i = 0; i < 10; i++)
            {
                spells_by_level.Add(new SpellsByLevelDuplet());
                spells_by_level[i].Level = i;
                spells_by_level[i].Spells = new List<SpellDefinition>();
            }

            foreach (var sl in spell_lists)
            {
                foreach (var sll in sl.SpellsByLevel)
                {
                    foreach (var s in sll.Spells)
                    {
                        if (!predicate(s))
                        {
                            continue;
                        }
                        if (!min_spell_levels.ContainsKey(s) || min_spell_levels[s] > sll.Level)
                        {
                            min_spell_levels[s] = sll.Level;
                        }
                    }
                }
            }

            foreach (var kv in min_spell_levels)
            {
                spells_by_level[kv.Value].Spells.Add(kv.Key);
            }

            //spells_by_level.RemoveAll(e => e.Spells.Count == 0);

            foreach (var sl in spells_by_level)
            {
                sl.Spells.Sort((a, b) => a.name.CompareTo(b.name));
            }

            return create9LevelSpelllist(name, guid, title_string, spells_by_level.Select(s => s.Spells).ToArray());
        }


        public static SpellListDefinition createCombinedSpellListWithLevelRestriction(string name, string guid, string title_string, params (SpellListDefinition, int)[] spell_lists_with_max_lvl)
        {
            List<SpellsByLevelDuplet> spells_by_level = new List<SpellsByLevelDuplet>();
            Dictionary<SpellDefinition, int> min_spell_levels = new Dictionary<SpellDefinition, int>();
            for (int i = 0; i < 10; i++)
            {
                spells_by_level.Add(new SpellsByLevelDuplet());
                spells_by_level[i].Level = i;
                spells_by_level[i].Spells = new List<SpellDefinition>();
            }

            foreach (var sl in spell_lists_with_max_lvl)
            {
                foreach (var sll in sl.Item1.SpellsByLevel)
                {
                    if (sll.Level > sl.Item2)
                    {
                        continue;
                    }
                    foreach (var s in sll.Spells)
                    {
                        if (!min_spell_levels.ContainsKey(s) || min_spell_levels[s] > sll.Level)
                        {
                            min_spell_levels[s] = sll.Level;
                        }
                    }
                }
            }

            foreach (var kv in min_spell_levels)
            {
                spells_by_level[kv.Value].Spells.Add(kv.Key);
            }

            spells_by_level.RemoveAll(e => e.Spells.Count == 0);

            foreach (var sl in spells_by_level)
            {
                sl.Spells.Sort((a, b) => a.name.CompareTo(b.name));
            }

            return create9LevelSpelllist(name, guid, title_string, spells_by_level.Select(s => s.Spells).ToArray());
        }
    }





    public class SpellcastingBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionCastSpell>
    {

        protected SpellcastingBuilder(string name, string guid, string title_string, string description_string, SpellListDefinition spelllist,
                                      string spell_stat, RuleDefinitions.SpellKnowledge spell_knowledge, RuleDefinitions.SpellReadyness spell_readyness,
                                      List<int> scribed_spells, List<int> cantrips_per_level, List<int> known_spells,
                                      RuleDefinitions.SpellPreparationCount spell_preparation_count,
                                      List<FeatureDefinitionCastSpell.SlotsByLevelDuplet> slots_pre_level,
                                      FeatureDefinitionCastSpell base_feature) : base(base_feature, name, guid)
        {
            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;
            
            Definition.SetSpellcastingAbility(spell_stat);
            Definition.SetSpellKnowledge(spell_knowledge);
            Definition.SetSpellReadyness(spell_readyness);
            Definition.ScribedSpells.Clear();
            Definition.ScribedSpells.AddRange(scribed_spells);
            Definition.KnownSpells.Clear();
            Definition.KnownSpells.AddRange(known_spells);
            Definition.SetSpellListDefinition(spelllist);
            Definition.KnownCantrips.Clear();
            Definition.KnownCantrips.AddRange(cantrips_per_level);
            Definition.SlotsPerLevels.Clear();
            Definition.SlotsPerLevels.AddRange(slots_pre_level);
            Definition.SetSpellPreparationCount(spell_preparation_count);
        }

        public static FeatureDefinitionCastSpell createSpontaneousSpellcasting(string name, string guid, string title_string, string description_string,
                                                                                SpellListDefinition spelllist, string spell_stat,
                                                                                List<int> cantrips_per_level, List<int> known_spells,
                                                                                List<FeatureDefinitionCastSpell.SlotsByLevelDuplet> slots_pre_level)
        {
            Stats.assertAllStats(new string[] { spell_stat });
            return new SpellcastingBuilder(name, guid, title_string, description_string, spelllist, spell_stat,
                                           RuleDefinitions.SpellKnowledge.Selection, RuleDefinitions.SpellReadyness.AllKnown,
                                           Enumerable.Repeat(0, 20).ToList(),
                                           cantrips_per_level,
                                           known_spells,
                                           RuleDefinitions.SpellPreparationCount.AbilityBonusPlusLevel,
                                           slots_pre_level,
                                           DatabaseHelper.FeatureDefinitionCastSpells.CastSpellWizard).AddToDB();
        }


        public static FeatureDefinitionCastSpell createPreparedArcaneSpellcasting(string name, string guid, string title_string, string description_string,
                                                                        SpellListDefinition spelllist, string spell_stat,
                                                                        List<int> cantrips_per_level, List<int> scribed_spells,
                                                                        RuleDefinitions.SpellPreparationCount spell_preparation_count,
                                                                        List<FeatureDefinitionCastSpell.SlotsByLevelDuplet> slots_pre_level)
        {
            Stats.assertAllStats(new string[] { spell_stat });
            return new SpellcastingBuilder(name, guid, title_string, description_string, spelllist, spell_stat,
                                           RuleDefinitions.SpellKnowledge.Spellbook, RuleDefinitions.SpellReadyness.Prepared,
                                           scribed_spells,
                                           cantrips_per_level,
                                           DatabaseHelper.FeatureDefinitionCastSpells.CastSpellWizard.knownSpells,
                                           spell_preparation_count,
                                           slots_pre_level,
                                           DatabaseHelper.FeatureDefinitionCastSpells.CastSpellWizard).AddToDB();
        }


        public static FeatureDefinitionCastSpell createDivinePreparedSpellcasting(string name, string guid, string title_string, string description_string,
                                                                        SpellListDefinition spelllist, string spell_stat,
                                                                        List<int> cantrips_per_level,
                                                                        RuleDefinitions.SpellPreparationCount spell_preparation_count,
                                                                        List<FeatureDefinitionCastSpell.SlotsByLevelDuplet> slots_pre_level)
        {
            Stats.assertAllStats(new string[] { spell_stat });
            return new SpellcastingBuilder(name, guid, title_string, description_string, spelllist, spell_stat,
                                           RuleDefinitions.SpellKnowledge.WholeList, RuleDefinitions.SpellReadyness.Prepared,
                                           Enumerable.Repeat(0, 20).ToList(),
                                           cantrips_per_level,
                                           DatabaseHelper.FeatureDefinitionCastSpells.CastSpellCleric.KnownSpells,
                                           spell_preparation_count,
                                           slots_pre_level,
                                           DatabaseHelper.FeatureDefinitionCastSpells.CastSpellCleric).AddToDB();
        }
    }


    public class CustomSpellcastingBuilder<T> : BaseDefinitionBuilderWithGuidStorage<T> where T : FeatureDefinitionCastSpell
    {

        protected CustomSpellcastingBuilder(string name, string guid, string title_string, string description_string, SpellListDefinition spelllist,
                                      string spell_stat, RuleDefinitions.SpellKnowledge spell_knowledge, RuleDefinitions.SpellReadyness spell_readyness,
                                      List<int> scribed_spells, List<int> cantrips_per_level, List<int> known_spells,
                                      List<FeatureDefinitionCastSpell.SlotsByLevelDuplet> slots_pre_level,
                                      FeatureDefinitionCastSpell base_feature) : base(name, guid)
        {
            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;

            Definition.SetSpellcastingAbility(spell_stat);
            Definition.SetSpellKnowledge(spell_knowledge);
            Definition.SetSpellReadyness(spell_readyness);
            Definition.ScribedSpells.Clear();
            Definition.ScribedSpells.AddRange(scribed_spells);
            Definition.KnownSpells.Clear();
            Definition.KnownSpells.AddRange(known_spells);
            Definition.SetSpellListDefinition(spelllist);
            Definition.KnownCantrips.Clear();
            Definition.KnownCantrips.AddRange(cantrips_per_level);
            Definition.SlotsPerLevels.Clear();
            Definition.SlotsPerLevels.AddRange(slots_pre_level);
            Definition.SetSpellCastingLevel(base_feature.spellCastingLevel);
            Definition.SetStaticDCValue(base_feature.staticDCValue);
            Definition.SetStaticToHitValue(base_feature.staticToHitValue);
            Definition.SetSpellCastingLevel(base_feature.spellCastingLevel);
            Definition.SetSpellCastingOrigin(base_feature.spellCastingOrigin);
            Definition.SetSpellPreparationCount(base_feature.spellPreparationCount);
            Definition.SetSpellcastingParametersComputation(base_feature.spellcastingParametersComputation);
            Definition.restrictedSchools = base_feature.restrictedSchools.ToArray().ToList();
        }

        public static T createSpontaneousSpellcasting(string name, string guid, string title_string, string description_string,
                                                                                SpellListDefinition spelllist, string spell_stat,
                                                                                List<int> cantrips_per_level, List<int> known_spells,
                                                                                List<FeatureDefinitionCastSpell.SlotsByLevelDuplet> slots_pre_level)
        {
            Stats.assertAllStats(new string[] { spell_stat });
            return new CustomSpellcastingBuilder<T>(name, guid, title_string, description_string, spelllist, spell_stat,
                                           RuleDefinitions.SpellKnowledge.Selection, RuleDefinitions.SpellReadyness.AllKnown,
                                           Enumerable.Repeat(0, 20).ToList(),
                                           cantrips_per_level,
                                           known_spells,
                                           slots_pre_level,
                                           DatabaseHelper.FeatureDefinitionCastSpells.CastSpellWizard).AddToDB();
        }
    }


    public class SavingThrowAffinityBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionSavingThrowAffinity>
    {


        protected SavingThrowAffinityBuilder(string name, string guid,
                                             string title_string, string description_string,
                                             AssetReferenceSprite sprite,
                                             RuleDefinitions.CharacterSavingThrowAffinity affinity,
                                             int dice_number,
                                             RuleDefinitions.DieType die_type,
                                             List<string> restricted_schools,
                                             params string[] stats) : base(DatabaseHelper.FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityCreedOfSolasta, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }
            if (sprite != null)
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }

            Definition.AffinityGroups.Clear();
            foreach (var s in stats)
            {
                var group = new SavingThrowAffinityGroup();
                group.savingThrowModifierDieType = die_type;
                group.savingThrowModifierDiceNumber = dice_number;
                group.affinity = affinity;
                group.abilityScoreName = s;
                group.restrictedSchools = restricted_schools;
                Definition.AffinityGroups.Add(group);
            }
        }

        public static FeatureDefinitionSavingThrowAffinity createSavingthrowAffinity(string name, string guid,
                                                                                     string title_string, string description_string,
                                                                                     AssetReferenceSprite sprite,
                                                                                     RuleDefinitions.CharacterSavingThrowAffinity affinity,
                                                                                     int dice_number,
                                                                                     RuleDefinitions.DieType die_type,
                                                                                     params string[] stats)
        {
            Stats.assertAllStats(stats);
            return new SavingThrowAffinityBuilder(name, guid, title_string, description_string, sprite, affinity, dice_number, die_type, new List<string>(), stats).AddToDB();
        }


        public static FeatureDefinitionSavingThrowAffinity createSavingthrowAffinityAgainstSchools(string name, string guid,
                                                                             string title_string, string description_string,
                                                                             AssetReferenceSprite sprite,
                                                                             RuleDefinitions.CharacterSavingThrowAffinity affinity,
                                                                             int dice_number,
                                                                             RuleDefinitions.DieType die_type,
                                                                             List<string> schools,
                                                                             params string[] stats)
        {
            Stats.assertAllStats(stats);
            return new SavingThrowAffinityBuilder(name, guid, title_string, description_string, sprite, affinity, dice_number, die_type, schools, stats).AddToDB();
        }
    }


    public class ConditionAffinityBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionConditionAffinity>
    {


        protected ConditionAffinityBuilder(string name, string guid,
                                          string title_string, string description_string,
                                          AssetReferenceSprite sprite,
                                          string condition_type,
                                          RuleDefinitions.ConditionAffinityType affinity,
                                          RuleDefinitions.AdvantageType saving_throw_advantage_type,
                                          RuleDefinitions.AdvantageType reroll_advantage_type) :
            base(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityElfFeyAncestryCharm, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }
            if (sprite != null)
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }

            Definition.SetConditionType(condition_type);
            Definition.SetConditionAffinityType(affinity);
            Definition.SetRerollAdvantageType(reroll_advantage_type);
            Definition.SetSavingThrowAdvantageType(saving_throw_advantage_type);
        }

        public static FeatureDefinitionConditionAffinity createConditionAffinity(string name, string guid,
                                                                                  string title_string, string description_string,
                                                                                  AssetReferenceSprite sprite,
                                                                                  string condition_type,
                                                                                  RuleDefinitions.ConditionAffinityType affinity,
                                                                                  RuleDefinitions.AdvantageType saving_throw_advantage_type,
                                                                                  RuleDefinitions.AdvantageType reroll_advantage_type)
        {

            Conditions.assertAllConditions(new string[] { condition_type });
            return new ConditionAffinityBuilder(name, guid, title_string, description_string, sprite, condition_type, affinity, saving_throw_advantage_type, reroll_advantage_type).AddToDB();
        }
    }



    public class AbilityCheckAffinityBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionAbilityCheckAffinity>
    {
        protected AbilityCheckAffinityBuilder(string name, string guid,
                                             string title_string, string description_string,
                                             AssetReferenceSprite sprite,
                                             RuleDefinitions.CharacterAbilityCheckAffinity affinity,
                                             int dice_number,
                                             RuleDefinitions.DieType die_type,
                                             List<string> stats,
                                             List<string> proficiencies) : base(DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityGuided, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }
            if (sprite != null)
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }


            Definition.AffinityGroups.Clear();
            for (int i = 0; i < stats.Count; i++)
            {
                var group = new AbilityCheckAffinityGroup();
                group.abilityScoreName = stats[i];
                group.abilityCheckModifierDiceNumber = dice_number;
                group.proficiencyName = proficiencies[i];
                group.abilityCheckModifierDieType = die_type;
                group.affinity = affinity;
                Definition.AffinityGroups.Add(group);
            }
        }

        public static FeatureDefinitionAbilityCheckAffinity createAbilityCheckAffinity(string name, string guid,
                                                                                         string title_string, string description_string,
                                                                                         AssetReferenceSprite sprite,
                                                                                         RuleDefinitions.CharacterAbilityCheckAffinity affinity,
                                                                                         int dice_number,
                                                                                         RuleDefinitions.DieType die_type,
                                                                                         params string[] stats)
        {
            Stats.assertAllStats(stats);
            return new AbilityCheckAffinityBuilder(name, guid, title_string, description_string, sprite, affinity, dice_number, die_type,
                                                    stats.ToList(), Enumerable.Repeat("", stats.Length).ToList()).AddToDB();
        }


        public static FeatureDefinitionAbilityCheckAffinity createSkillCheckAffinity(string name, string guid,
                                                                             string title_string, string description_string,
                                                                             AssetReferenceSprite sprite,
                                                                             RuleDefinitions.CharacterAbilityCheckAffinity affinity,
                                                                             int dice_number,
                                                                             RuleDefinitions.DieType die_type,
                                                                             params string[] skills)
        {
            Skills.assertAllSkills(skills);
            return new AbilityCheckAffinityBuilder(name, guid, title_string, description_string, sprite, affinity, dice_number, die_type,
                                                    skills.Select(s => Skills.skill_stat_map[s]).ToList(), skills.ToList()).AddToDB();
        }
    }


    public class AttackBonusBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionCombatAffinity>
    {
        protected AttackBonusBuilder(string name, string guid,
                                             string title_string, string description_string,
                                             AssetReferenceSprite sprite,
                                             int dice_number,
                                             RuleDefinitions.DieType die_type,
                                             bool substract = false) : base(DatabaseHelper.FeatureDefinitionCombatAffinitys.CombatAffinityBlessed, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = title_string;
            }
            if (sprite != null)
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }
            Definition.SetMyAttackModifierDiceNumber(dice_number);
            Definition.SetMyAttackModifierDieType(die_type);
            if (substract)
            {
                Definition.SetMyAttackModifierSign(RuleDefinitions.AttackModifierSign.Substract);
            }

        }


        public static FeatureDefinitionCombatAffinity createAttackBonus(string name, string guid,
                                                                                 string title_string, string description_string,
                                                                                 AssetReferenceSprite sprite,
                                                                                 int dice_number,
                                                                                 RuleDefinitions.DieType die_type,
                                                                                 bool substract = false)
        {
            return new AttackBonusBuilder(name, guid, title_string, description_string, sprite, dice_number, die_type, substract).AddToDB();
        }
    }


    public class ConditionBuilder : BaseDefinitionBuilderWithGuidStorage<ConditionDefinition>
    {
        protected ConditionBuilder(string name, string guid,
                                   string title_string, string description_string,
                                   AssetReferenceSprite sprite,
                                   ConditionDefinition base_condititon,
                                   RuleDefinitions.ConditionInterruption[] interruptions,
                                   FeatureDefinition[] features) : base(base_condititon, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }
            if (sprite != null)
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }
            Definition.SpecialInterruptions.Clear();
            Definition.SpecialInterruptions.AddRange(interruptions);
            Definition.Features.Clear();
            Definition.Features.AddRange(features);
            Definition.RecurrentEffectForms?.Clear();
        }


        public static ConditionDefinition createCondition(string name, string guid,
                                                            string title_string, string description_string,
                                                            AssetReferenceSprite sprite,
                                                            ConditionDefinition base_condititon,
                                                            params FeatureDefinition[] features)
        {
            return new ConditionBuilder(name, guid, title_string, description_string, sprite, base_condititon, new RuleDefinitions.ConditionInterruption[0], features).AddToDB();
        }


        public static ConditionDefinition createConditionWithInterruptions(string name, string guid,
                                                                          string title_string, string description_string,
                                                                          AssetReferenceSprite sprite,
                                                                          ConditionDefinition base_condititon,
                                                                          RuleDefinitions.ConditionInterruption[] interruptions,
                                                                          params FeatureDefinition[] features)
        {
            return new ConditionBuilder(name, guid, title_string, description_string, sprite, base_condititon, interruptions, features).AddToDB();
        }
    }



    public class PowerBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionPower>
    {
        protected PowerBuilder(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite,
                               FeatureDefinitionPower base_power,
                               EffectDescription effect_description,
                               RuleDefinitions.ActivationTime activation_time,
                               int fixed_uses,
                               RuleDefinitions.UsesDetermination uses_determination,
                               RuleDefinitions.RechargeRate recharge_rate,
                               string uses_ability,
                               string ability,
                               int cost_per_use = 1,
                               bool show_casting = true) : base(base_power, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }
            if (sprite != null)
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }

            Definition.SetRechargeRate(recharge_rate);
            Definition.SetCostPerUse(cost_per_use);
            Definition.SetFixedUsesPerRecharge(fixed_uses);
            Definition.SetActivationTime(activation_time);
            Definition.SetUsesDetermination(uses_determination);
            Definition.SetShowCasting(show_casting);
            Definition.SetAbilityScore(ability);
            Definition.SetUsesAbilityScoreName(uses_ability);
            Definition.SetEffectDescription(effect_description);
        }

        public static FeatureDefinitionPower createPower(string name, string guid,
                                                         string title_string, string description_string, AssetReferenceSprite sprite,
                                                         FeatureDefinitionPower base_power,
                                                         EffectDescription effect_description,
                                                         RuleDefinitions.ActivationTime activation_time,
                                                         int fixed_uses,
                                                         RuleDefinitions.UsesDetermination uses_determination,
                                                         RuleDefinitions.RechargeRate recharge_rate,
                                                         string uses_ability = "Strength",
                                                         string ability = "Strength",
                                                         int cost_per_use = 1,
                                                         bool show_casting = true)
        {
            return new PowerBuilder(name, guid, title_string, description_string, sprite, base_power, effect_description,
                                    activation_time, fixed_uses, uses_determination, recharge_rate,
                                    uses_ability, ability, cost_per_use, show_casting).AddToDB();
        }
    }


    public class OnlyDescriptionFeatureBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinition>
    {
        protected OnlyDescriptionFeatureBuilder(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite)
                : base(name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }
            if (sprite == null)
            {
                Definition.GuiPresentation.SetSpriteReference(DatabaseHelper.FeatureDefinitionPointPools.PointPoolAbilityScoreImprovement.GuiPresentation.SpriteReference);
            }
            else
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }
        }


        public static FeatureDefinition createOnlyDescriptionFeature(string name, string guid, string new_title_string, string new_description_string)
        {
            return new OnlyDescriptionFeatureBuilder(name, guid, new_title_string, new_description_string, null).AddToDB();
        }


        public static FeatureDefinition createOnlyDescriptionFeature(string name, string guid, string new_title_string, string new_description_string, AssetReferenceSprite sprite)
        {
            return new OnlyDescriptionFeatureBuilder(name, guid, new_title_string, new_description_string, sprite).AddToDB();
        }
    }



    public class CopyFeatureBuilder<TDefinition> : BaseDefinitionBuilderWithGuidStorage<TDefinition> where TDefinition : BaseDefinition
    {
        protected CopyFeatureBuilder(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite, TDefinition base_feature)
                : base(base_feature, name, guid)
        {
            if (title_string != "")
            {
                Definition.GuiPresentation.Title = title_string;
            }
            if (description_string != "")
            {
                Definition.GuiPresentation.Description = description_string;
            }
            if (sprite != null)
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }
        }


        public static TDefinition createFeatureCopy(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite, TDefinition base_feature)
        {
            return new CopyFeatureBuilder<TDefinition>(name, guid, title_string, description_string, sprite, base_feature).AddToDB();
        }

        public static TDefinition createFeatureCopy(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite,
                                                        TDefinition base_feature, Action<TDefinition> action)
        {
            var res = new CopyFeatureBuilder<TDefinition>(name, guid, title_string, description_string, sprite, base_feature).AddToDB();
            action(res);
            return res;
        }
    }


    public class FeatureBuilder<TDefinition> : BaseDefinitionBuilderWithGuidStorage<TDefinition> where TDefinition : BaseDefinition
    {
        protected FeatureBuilder(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite)
                : base(name, guid)
        {

            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;

            if (sprite != null)
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }
            else
            {
                Definition.GuiPresentation.SetSpriteReference(DatabaseHelper.FeatureDefinitionPointPools.PointPoolAbilityScoreImprovement.GuiPresentation.SpriteReference);
            }
        }


        public static TDefinition createFeature(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite)
        {
            return new FeatureBuilder<TDefinition>(name, guid, title_string, description_string, sprite).AddToDB();
        }

        public static TDefinition createFeature(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite, Action<TDefinition> action)
        {
            var res = new FeatureBuilder<TDefinition>(name, guid, title_string, description_string, sprite).AddToDB();
            action(res);
            return res;
        }
    }


    public class AutoPrepareSpellBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionAutoPreparedSpells>
    {
        protected AutoPrepareSpellBuilder(string name, string guid, string title_string, string description_string,
                                          CharacterClassDefinition caster_class,
                                          params FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup[] spells_at_level)
                : base(DatabaseHelper.FeatureDefinitionAutoPreparedSpellss.AutoPreparedSpellsDomainBattle, name, guid)
        {

            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;

            Definition.SetSpellcastingClass(caster_class);
            Definition.AutoPreparedSpellsGroups.Clear();
            Definition.AutoPreparedSpellsGroups.AddRange(spells_at_level);

        }


        public static FeatureDefinitionAutoPreparedSpells createAutoPrepareSpell(string name, string guid, string title_string, string description_string,
                                                                                  CharacterClassDefinition caster_class,
                                                                                  params FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup[] spells_at_level)
        {
            return new AutoPrepareSpellBuilder(name, guid, title_string, description_string, caster_class, spells_at_level).AddToDB();
        }
    }


    public class BonusCantripsBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionBonusCantrips>
    {
        protected BonusCantripsBuilder(string name, string guid, string title_string, string description_string,
                                          params SpellDefinition[] cantrips)
                : base(DatabaseHelper.FeatureDefinitionBonusCantripss.BonusCantripsDomainSun, name, guid)
        {
            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;

            Definition.BonusCantrips.Clear();
            Definition.BonusCantrips.AddRange(cantrips);
        }


        public static FeatureDefinitionBonusCantrips createLearnBonusCantrip(string name, string guid, string title_string, string description_string,
                                                                            params SpellDefinition[] cantrips)
        {
            return new BonusCantripsBuilder(name, guid, title_string, description_string, cantrips).AddToDB();
        }
    }


    public class FeatureSetBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionFeatureSet>
    {
        protected FeatureSetBuilder(string name, string guid, string title_string, string description_string,
                                     bool enumerate_in_description, FeatureDefinitionFeatureSet.FeatureSetMode set_mode,
                                     bool unique_choices,
                                     params FeatureDefinition[] features)
            : base(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetHunterDefensiveTactics, name, guid)
        {
            Definition.GuiPresentation.Description = description_string;
            Definition.GuiPresentation.Title = title_string;

            Definition.FeatureSet.Clear();
            Definition.FeatureSet.AddRange(features);
            Definition.SetEnumerateInDescription(enumerate_in_description);
            Definition.SetMode(set_mode);
            Definition.SetUniqueChoices(unique_choices);
        }

        public static FeatureDefinitionFeatureSet createFeatureSet(string name, string guid, string title_string, string description_string,
                                                                             bool enumerate_in_description, FeatureDefinitionFeatureSet.FeatureSetMode set_mode,
                                                                             bool unique_choices,
                                                                             params FeatureDefinition[] features)
        {
            return new FeatureSetBuilder(name, guid, title_string, description_string, enumerate_in_description, set_mode, unique_choices, features).AddToDB();
        }
    }


    public class GenericPowerBuilder<T> : BaseDefinitionBuilderWithGuidStorage<T> where T : FeatureDefinitionPower
    {
        protected GenericPowerBuilder(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite,
                                       EffectDescription effect_description,
                                       RuleDefinitions.ActivationTime activation_time,
                                       int fixed_uses,
                                       RuleDefinitions.UsesDetermination uses_determination,
                                       RuleDefinitions.RechargeRate recharge_rate,
                                       string uses_ability,
                                       string ability,
                                       int cost_per_use = 1,
                                       bool show_casting = true) : base(name, guid)
        {
            Definition.SetGuiPresentation(new GuiPresentation());
            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;
            Definition.GuiPresentation.SetSpriteReference(sprite);

            Definition.SetRechargeRate(recharge_rate);
            Definition.SetCostPerUse(cost_per_use);
            Definition.SetFixedUsesPerRecharge(fixed_uses);
            Definition.SetActivationTime(activation_time);
            Definition.SetUsesDetermination(uses_determination);
            Definition.SetShowCasting(show_casting);
            Definition.SetAbilityScore(ability);
            Definition.SetUsesAbilityScoreName(uses_ability);
            Definition.SetEffectDescription(effect_description);
        }

        public static T createPower(string name, string guid,
                                    string title_string, string description_string, AssetReferenceSprite sprite,
                                    EffectDescription effect_description,
                                    RuleDefinitions.ActivationTime activation_time,
                                    int fixed_uses,
                                    RuleDefinitions.UsesDetermination uses_determination,
                                    RuleDefinitions.RechargeRate recharge_rate,
                                    string uses_ability = "Strength",
                                    string ability = "Strength",
                                    int cost_per_use = 1,
                                    bool show_casting = true)
        {
            return new GenericPowerBuilder<T>(name, guid, title_string, description_string, sprite, effect_description,
                                                activation_time, fixed_uses, uses_determination, recharge_rate,
                                                uses_ability, ability, cost_per_use, show_casting).AddToDB();
        }
    }



    public class GenericSpellBuilder<T> : BaseDefinitionBuilderWithGuidStorage<T> where T : SpellDefinition
    {
        protected GenericSpellBuilder(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite,
                                       EffectDescription effect_description,
                                       RuleDefinitions.ActivationTime casting_time,
                                       int spell_level,
                                       bool requires_concentration,
                                       bool verbose_component,
                                       bool somatic_component,
                                       string school,
                                       bool is_ritual = false,
                                       RuleDefinitions.ActivationTime ritual_time = RuleDefinitions.ActivationTime.Minute10) : base(name, guid)
        {
            Definition.SetGuiPresentation(new GuiPresentation());
            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;
            Definition.GuiPresentation.SetSpriteReference(sprite);

            Definition.SetCastingTime(casting_time);
            Definition.SetRitual(is_ritual);
            Definition.SetRitualCastingTime(ritual_time);
            Definition.SetSpellLevel(spell_level);
            Definition.SetRequiresConcentration(requires_concentration);
            Definition.SetVerboseComponent(verbose_component);
            Definition.SetSomaticComponent(somatic_component);
            Definition.SetSchoolOfMagic(school);
            Definition.SetEffectDescription(effect_description);
            Definition.SetImplemented(true);

            
        }

        public static T createSpell(string name, string guid, string title_string, string description_string, AssetReferenceSprite sprite,
                                       EffectDescription effect_description,
                                       RuleDefinitions.ActivationTime casting_time,
                                       int spell_level,
                                       bool requires_concentration,
                                       bool verbose_component,
                                       bool somatic_component,
                                       string school,
                                       bool is_ritual = false,
                                       RuleDefinitions.ActivationTime ritual_time = RuleDefinitions.ActivationTime.Minute10)
        {
            var spell = new GenericSpellBuilder<T>(name, guid, title_string, description_string, sprite,
                                               effect_description,
                                               casting_time,
                                               spell_level,
                                               requires_concentration,
                                               verbose_component,
                                               somatic_component,
                                               school,
                                               is_ritual,
                                               ritual_time).AddToDB();
            NewFeatureDefinitions.SpellData.registerSpell(spell);
            return spell;
        }
    }


    public class ExtraSpellSelectionBuilder : BaseDefinitionBuilderWithGuidStorage<NewFeatureDefinitions.FeatureDefinitionExtraSpellSelection>
    {
        protected ExtraSpellSelectionBuilder(string name, string guid, string title_string, string description_string,
                                          CharacterClassDefinition caster_class, int level, int num_spells, int num_cantrips,
                                          SpellListDefinition spell_list)
                : base(name, guid)
        {

            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;

            Definition.caster_class = caster_class;
            Definition.level = level;
            Definition.max_spells = num_spells;
            Definition.spell_list = spell_list;
            Definition.max_cantrips = num_cantrips;
            Definition.learnCantrips = num_cantrips > 0;
        }


        public static NewFeatureDefinitions.FeatureDefinitionExtraSpellSelection createExtraSpellSelection(string name, string guid, string title_string, string description_string,
                                                                                                          CharacterClassDefinition caster_class, int level, int num_spells,
                                                                                                          SpellListDefinition spell_list)
        {
            return new ExtraSpellSelectionBuilder(name, guid, title_string, description_string, caster_class, level, num_spells, 0, spell_list).AddToDB();
        }


        public static NewFeatureDefinitions.FeatureDefinitionExtraSpellSelection createExtraCantripSelection(string name, string guid, string title_string, string description_string,
                                                                                                  CharacterClassDefinition caster_class, int level, int num_cantrips,
                                                                                                  SpellListDefinition spell_list)
        {
            return new ExtraSpellSelectionBuilder(name, guid, title_string, description_string, caster_class, level, 0, num_cantrips, spell_list).AddToDB();
        }
    }


    public class ExtraSpellSelectionFromFeatBuilder : BaseDefinitionBuilderWithGuidStorage<NewFeatureDefinitions.FeatureDefinitionExtraSpellSelectionFromFeat>
    {
        protected ExtraSpellSelectionFromFeatBuilder(string name, string guid, string title_string, string description_string,
                                                      int num_spells, int num_cantrips,
                                                      SpellListDefinition spell_list)
                : base(name, guid)
        {

            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;
            Definition.max_spells = num_spells;
            Definition.spell_list = spell_list;
            Definition.max_cantrips = num_cantrips;
            Definition.learnCantrips = num_cantrips > 0;
        }


        public static NewFeatureDefinitions.FeatureDefinitionExtraSpellSelectionFromFeat createExtraSpellSelection(string name, string guid, string title_string, string description_string,
                                                                                                                      int num_spells,
                                                                                                                      SpellListDefinition spell_list)
        {
            return new ExtraSpellSelectionFromFeatBuilder(name, guid, title_string, description_string, num_spells, 0, spell_list).AddToDB();
        }


        public static NewFeatureDefinitions.FeatureDefinitionExtraSpellSelectionFromFeat createExtraCantripSelection(string name, string guid, string title_string, string description_string,
                                                                                                                      int num_cantrips,
                                                                                                                      SpellListDefinition spell_list)
        {
            return new ExtraSpellSelectionFromFeatBuilder(name, guid, title_string, description_string, 0, num_cantrips, spell_list).AddToDB();
        }
    }





    public static class Misc
    {
        public static string getFeatTagForFeature(CharacterBuildingManager manager, FeatureDefinition feature)
        {
            foreach (var tf in manager.trainedFeats)
            {
                foreach (var f in tf.Value)
                {
                    if (f.features.Contains(feature))
                    {
                        return tf.Key;
                    }
                }
            }
            return "";
        }

        public static (CharacterClassDefinition, int) getClassAndLevelFromTag(string tag)
        {
            if (!tag.Contains("03Class"))
            {
                return (null, 0);
            }

            string class_and_level = tag.Replace("03Class", "");
            var classes = DatabaseRepository.GetDatabase<CharacterClassDefinition>().GetAllElements();
            var cls = classes.FirstOrDefault(c => c.name == class_and_level.Substring(0, class_and_level.Length - 1));
            int lvl = 0;
            if (cls != null && int.TryParse(class_and_level.Substring(class_and_level.Length - 1), out lvl))
            {
                return (cls, lvl);
            }
            cls = classes.FirstOrDefault(c => c.name == class_and_level.Substring(0, class_and_level.Length - 2));
            if (cls != null && int.TryParse(class_and_level.Substring(class_and_level.Length - 2), out lvl))
            {
                return (cls, lvl);
            }
            return (null, 0);
        }

        public static bool canMakeAoo(GameLocationBattleManager battle_manager, GameLocationCharacter attacker, GameLocationCharacter defender,
                                      out RulesetAttackMode attackMode, out ActionModifier actionModifierBefore)
        {
            actionModifierBefore = new ActionModifier();
            attackMode = null;
            if (!battle_manager.IsValidAttackerForOpportunityAttackOnCharacter(attacker, defender))
            {
                return false;
            }
            attackMode = attacker.FindActionAttackMode(ActionDefinitions.Id.AttackOpportunity);
            if (attackMode == null)
            {
                return false;
            }
            BattleDefinitions.AttackEvaluationParams attackParams1 = new BattleDefinitions.AttackEvaluationParams();
            attackParams1.FillForPhysicalReachAttack(attacker, attacker.LocationPosition, attackMode, defender, defender.LocationPosition, actionModifierBefore);
            return battle_manager.CanAttack(attackParams1, false);
        }

        public static RulesetUsablePower[] getNonOverridenPowers(RulesetCharacter character, Predicate<RulesetUsablePower> filter)
        {
            if (character == null)
            {
                return new RulesetUsablePower[0];
            }

            var powers = character.UsablePowers.Where(u => filter(u)).ToArray();

            var overriden_powers = powers.Aggregate(new List<FeatureDefinitionPower>(), (old, next) =>
            {
                if (next.PowerDefinition?.overriddenPower != null)
                {
                    old.Add(next.PowerDefinition?.overriddenPower);
                }
                return old;
            }).ToHashSet();
            powers = powers.Where(pp => !overriden_powers.Contains(pp.powerDefinition)).ToArray();
            return powers;
        }


        static public bool isWearingArmorWithNonZeroProtection(RulesetCharacter character)
        {
            if (character.IsWearingArmor())
            {
                return true;
            }


            var hero = character as RulesetCharacterHero;
            RulesetItem equipedItem = hero.characterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeTorso].EquipedItem;
            if (equipedItem != null && equipedItem.ItemDefinition.IsArmor 
                && equipedItem.ItemDefinition.armorDefinition.isBaseArmorClass && equipedItem.ItemDefinition.armorDefinition.armorClassValue > 10)
            {
                return true;
            }
            return false;
        }


        static public bool itemHasFeature(RulesetItem item, FeatureDefinition feature)
        {
            List<FeatureDefinition> features = new List<FeatureDefinition>();
            item?.EnumerateFeaturesToBrowse<FeatureDefinition>(features);

            return features.Contains(feature);
        }

        static public FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup createAutopreparedSpellsGroup(int level, params SpellDefinition[] spells)
        {
            var group = new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup()
            {
                classLevel = level,
                spellsList = spells.ToList()
            };
            return group;
        }


        static public RulesetEffect findConditionParentEffect(RulesetCondition condition)
        {
            RulesetCharacter caster = (RulesetCharacter)null;
            if (RulesetEntity.TryGetEntity<RulesetCharacter>(condition.sourceGuid, out caster))
            {
                var eff = caster.EnumerateActiveEffectsActivatedByMe().Where(e => e.TrackedConditionGuids.Contains(condition.guid)).FirstOrDefault();
                if (eff != null)
                {
                    return eff;
                }
            }
            return null;
        }

        static public void addSpellToSpelllist(SpellListDefinition spelllist, SpellDefinition spell)
        {
            if (spell.spellLevel == 0 && !spelllist.hasCantrips)
            {
                throw new System.Exception($"Trying to add cantrip {spell.name} to spell list without cantrips {spelllist.name}");
            }

            if (spelllist.ContainsSpell(spell))
            {
                throw new System.Exception($"Spelllist {spelllist.name} already contains spell {spell}");
            }


            if (spelllist.hasCantrips)
            {
                spelllist.spellsByLevel[spell.spellLevel].spells.Add(spell);
            }
            else
            {
                spelllist.spellsByLevel[spell.spellLevel - 1].spells.Add(spell);
            }
        }

        static public List<SpellDefinition> filterCharacterSpells(RulesetCharacter character, Predicate<SpellDefinition> filter)
        {
            List<SpellDefinition> spells = new List<SpellDefinition>();
            character.EnumerateUsableSpells();
            if (character.UsableSpells.Count > 0)
            {
                foreach (SpellDefinition usableSpell in character.usableSpells)
                {
                    if (filter(usableSpell))
                    {
                        spells.Add(usableSpell);
                    }
                }
            }
            return spells;
        }


        static public bool hasDamageType(List<EffectForm> actualEffectForms, params string[] damage_types)
        {
            foreach (EffectForm actualEffectForm in actualEffectForms)
            {
                EffectForm effectForm = actualEffectForm;
                if (effectForm.FormType == EffectForm.EffectFormType.Damage && damage_types.Contains(effectForm.DamageForm.DamageType))
                {
                    return true;  
                }
            }
            return false;
        }
        public static bool characterHasFeature(RulesetActor actor, FeatureDefinition feature)
        {
            if (feature is FeatureDefinitionFeatureSet)
            {
                //TODO: make a proper check for FeatureSet
                var feature_set = feature as FeatureDefinitionFeatureSet;
                if (feature_set.mode == FeatureDefinitionFeatureSet.FeatureSetMode.Union)
                {
                    return feature_set.FeatureSet.Count > 0 ? characterHasFeature(actor, feature_set.FeatureSet[0]) : false;
                }
                else
                {
                    foreach (var f in feature_set.FeatureSet)
                    {
                        if (characterHasFeature(actor, f))
                        {
                            return true;
                        }
                       
                    }
                    return false;
                }
            }
            else
            {
                return Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinition>(actor).Any(f => f == feature);
            }

        }
        public static GameLocationCharacter findGameLocationCharacter(RulesetCharacter character)
        {
            if (character == null)
            {
                return null;
            }

            return GameLocationCharacter.GetFromActor(character);
        }


        public static SpellDefinition convertSpellToCantrip(SpellDefinition spell, string name,  string title_string, bool self_only = false)
        {
            var cantrip = Helpers.CopyFeatureBuilder<SpellDefinition>.createFeatureCopy(name,
                                                                                        "",
                                                                                        title_string,
                                                                                        "",
                                                                                        null,
                                                                                        spell);
            cantrip.spellLevel = 0;
            cantrip.EffectDescription.EffectAdvancement?.Clear();
            cantrip.materialComponentType = RuleDefinitions.MaterialComponentType.None;
            if (self_only)
            {
                cantrip.EffectDescription.SetTargetType(RuleDefinitions.TargetType.Self);
                cantrip.EffectDescription.SetRangeType(RuleDefinitions.RangeType.Self);
            }
            cantrip.ritual = false;
            return cantrip;
        }


        public static List<DiceByRank> createDiceRankTable(int max_level, params (int, int)[] entries)
        {
            List<DiceByRank> table = new List<DiceByRank>();
            for (int i = 1; i <= max_level; i++)
            {
                table.Add(new DiceByRank() { rank = i, diceNumber = 0 });
            }

            int k = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                while (k < table.Count && table[k].rank <= entries[i].Item1)
                {
                    table[k].diceNumber = entries[i].Item2;
                    k++;
                }
            }
            return table;
        }


        public static EffectDescription addEffectFormsToEffectDescription(EffectDescription base_effect, params EffectForm[] effect_forms)
        {
            var new_effect = new EffectDescription();
            new_effect.Copy(base_effect);
            foreach (var ef in effect_forms)
            {
                new_effect.effectForms.Add(ef);
            }

            return new_effect;
        }


        public static List<FeatureDefinitionCastSpell.SlotsByLevelDuplet> createSpellSlotsByLevel(params List<int>[] slots_num_per_level)
        {
            var res = new List<FeatureDefinitionCastSpell.SlotsByLevelDuplet>();

            for (int i = 0; i < slots_num_per_level.Length; i++)
            {
                res.Add(new FeatureDefinitionCastSpell.SlotsByLevelDuplet()
                        {
                            level = i + 1,
                            slots = slots_num_per_level[i]
                        });
            }

            return res;
        }

        public static string createImmuneIfHasNoConditionFamily(ConditionDefinition condition)
        {
            return "IMMUNE_IF_HAS_NO_CONDITION_" + condition.name;
        }

        public static ConditionDefinition extractImmuneIfHasNoCondition(string s)
        {
            if (!s.Contains("IMMUNE_IF_HAS_NO_CONDITION_"))
            {
                return null;
            }

            return DatabaseRepository.GetDatabase<ConditionDefinition>().GetElement(s.Replace("IMMUNE_IF_HAS_NO_CONDITION_", ""), true);
        }


        public static string createImmuneIfHasConditionFamily(ConditionDefinition condition)
        {
            return "IMMUNE_IF_HAS_CONDITION_" + condition.name;
        }

        public static ConditionDefinition extractImmuneCondition(string s)
        {
            if (!s.Contains("IMMUNE_IF_HAS_CONDITION_"))
            {
                return null;
            }

            return DatabaseRepository.GetDatabase<ConditionDefinition>().GetElement(s.Replace("IMMUNE_IF_HAS_CONDITION_", ""), true);
        }


        /*public static string createContextDeterminedAttribute(ConditionDefinition condition)
        {
            return "CONTEXT_DETERMINED_ATTRIBUTE_" + condition.name;
        }


        public static string extractContextDeterminedAttribute(RulesetCharacter character, string s)
        {
            if (!s.Contains("CONTEXT_DETERMINED_ATTRIBUTE_"))
            {
                return string.Empty;
            }
            
            var condition_name = s.Replace("CONTEXT_DETERMINED_ATTRIBUTE_", "");
            RulesetEffectSpell spell_effect = null;
            foreach (var cc in character.ConditionsByCategory)
            {
                foreach (var c in cc.Value)
                {
                    if (c.ConditionDefinition.name == condition_name)
                    {
                        spell_effect = Helpers.Misc.findConditionParentEffect(c) as RulesetEffectSpell;
                        break;
                    }
                }
            }

            if (spell_effect == null)
            {
                return string.Empty;
            }

            var stat = spell_effect.SpellRepertoire?.SpellCastingFeature?.SpellcastingAbility;
            if (stat != string.Empty)
            {
                return stat;
            }
            else
            {
                return string.Empty;
            }
        }*/
    }





    public class Accessors
    {
        static public T memberwiseClone<T>(T base_object)
        {
            MethodInfo memberwiseCloneMethod = typeof(T).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            var copy = (T)memberwiseCloneMethod.Invoke(base_object, null);
            return copy;
        }
       

        public class EnumeratorCombiner: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator enumerator1;
            private System.Collections.IEnumerator enumerator2;

            private bool moved_to_second;


            public EnumeratorCombiner(System.Collections.IEnumerator ienum1, System.Collections.IEnumerator ienum2)
            {
                enumerator1 = ienum1;
                enumerator2 = ienum2;
                moved_to_second = false;
            }

            public object Current => moved_to_second ? enumerator2.Current : enumerator1.Current;

            public System.Collections.IEnumerator GetEnumerator()
            {
                while (enumerator1.MoveNext())
                {
                    yield return enumerator1.Current;
                }
                while (enumerator2.MoveNext())
                {
                    yield return enumerator2.Current;
                }
            }

            public bool MoveNext()
            {
                if (moved_to_second)
                {
                    return enumerator2.MoveNext();
                }

                bool val = enumerator1.MoveNext();
                if (val)
                {
                    return val;
                }
                moved_to_second = true;
                return enumerator2.MoveNext();
                
            }

            public void Reset()
            {
                moved_to_second = false;
                enumerator1.Reset();
                enumerator2.Reset();
            }
        }


        public static void SetField(object obj, string name, object value)
        {
            HarmonyLib.AccessTools.Field(obj.GetType(), name).SetValue(obj, value);
        }


        public static object GetField(object obj, string name)
        {
            return HarmonyLib.AccessTools.Field(obj.GetType(), name).GetValue(obj);
        }


        static public System.Collections.IEnumerator convertToEnumerator(List<object> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
            }
        }

        static public List<T> extractFeaturesHierarchically<T>(RulesetActor actor) where T: class
        {
            var list = new List<FeatureDefinition>();
            actor.EnumerateFeaturesToBrowse<T>(list, null);
            return list.Select(s => s as T).ToList();
        }


        static public RulesetUsablePower[] extractPowers(RulesetCharacter character, Predicate<RulesetUsablePower> predicate)
        {
            var powers = character.UsablePowers.Where(u => 
                                                      character.GetRemainingUsesOfPower(u) > 0
                                                      && predicate(u)
                                                     ).ToArray();

            var overriden_powers = powers.Aggregate(new List<FeatureDefinitionPower>(), (old, next) =>
            {
                if (next.PowerDefinition?.overriddenPower != null)
                {
                    old.Add(next.PowerDefinition?.overriddenPower);
                }
                return old;
            });
            powers = powers.Where(pp => !overriden_powers.Contains(pp.powerDefinition)).ToArray();
            return powers;
        }
    }
}
