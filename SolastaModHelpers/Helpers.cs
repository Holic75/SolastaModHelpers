﻿using SolastaModApi;
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
                    Definition.SpellsByLevel[i].Spells.AddRange(spells_by_level[i].Where(s => s.ContentPack == GamingPlatformDefinitions.ContentPack.BaseGame && s.Implemented));
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
    }


    public class SpellcastingBuilder : BaseDefinitionBuilderWithGuidStorage<FeatureDefinitionCastSpell>
    {

        protected SpellcastingBuilder(string name, string guid, string title_string, string description_string, SpellListDefinition spelllist,
                                      string spell_stat, RuleDefinitions.SpellKnowledge spell_knowledge, RuleDefinitions.SpellReadyness spell_readyness,
                                      List<int> scribed_spells, List<int> cantrips_per_level, List<int> known_spells,
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
            return new SavingThrowAffinityBuilder(name, guid, title_string, description_string, sprite, affinity, dice_number, die_type, stats).AddToDB();
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
                Definition.GuiPresentation.Description = title_string;
            }
            if (sprite != null)
            {
                Definition.GuiPresentation.SetSpriteReference(sprite);
            }
            Definition.SpecialInterruptions.Clear();
            Definition.SpecialInterruptions.AddRange(interruptions);
            Definition.Features.Clear();
            Definition.Features.AddRange(features);
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
        protected OnlyDescriptionFeatureBuilder(string name, string guid, string title_string, string description_string)
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

            Definition.GuiPresentation.SetSpriteReference(DatabaseHelper.FeatureDefinitionPointPools.PointPoolAbilityScoreImprovement.GuiPresentation.SpriteReference);
        }


        public static FeatureDefinition createOnlyDescriptionFeature(string name, string guid, string new_title_string, string new_description_string)
        {
            return new OnlyDescriptionFeatureBuilder(name, guid, new_title_string, new_description_string).AddToDB();
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



    public class ExtraSpellSelectionBuilder : BaseDefinitionBuilderWithGuidStorage<NewFeatureDefinitions.FeatureDefinitionExtraSpellSelection>
    {
        protected ExtraSpellSelectionBuilder(string name, string guid, string title_string, string description_string,
                                          CharacterClassDefinition caster_class, int level, int num_spells,
                                          SpellListDefinition spell_list)
                : base(name, guid)
        {

            Definition.GuiPresentation.Title = title_string;
            Definition.GuiPresentation.Description = description_string;

            Definition.caster_class = caster_class;
            Definition.level = level;
            Definition.max_spells = num_spells;
            Definition.spell_list = spell_list;
        }


        public static NewFeatureDefinitions.FeatureDefinitionExtraSpellSelection createExtraSpellSelection(string name, string guid, string title_string, string description_string,
                                                                                                          CharacterClassDefinition caster_class, int level, int num_spells,
                                                                                                          SpellListDefinition spell_list)
        {
            return new ExtraSpellSelectionBuilder(name, guid, title_string, description_string, caster_class, level, num_spells, spell_list).AddToDB();
        }
    }


    public static class Misc
    {
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
    }


    public class Accessors
    {

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