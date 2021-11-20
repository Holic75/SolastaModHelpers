using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    //power that will additionaly consume resource from another power linked to it,
    //thus number of times it can be used = min(number_of_remaining_power_uses, number_of_remaining_linked_power_uses)
    public class LinkedPower : FeatureDefinitionPower
    {
        public FeatureDefinition linkedPower;

        public RulesetUsablePower getBasePower(RulesetCharacter character)
        {
            if (linkedPower == null)
            {
                return null;
            }
            return character?.usablePowers?.FirstOrDefault(p => p.PowerDefinition == linkedPower);
        }
    }

    public interface IPowerRestriction
    {
        bool isForbidden(RulesetActor character);
        bool isReactionForbidden(RulesetActor character);
    }


    public interface ICustomPowerAbilityScore
    {
        string getPowerAbilityScore(RulesetCharacter character);
    }

    public interface ICustomPowerEffectBasedOnCaster
    {
        EffectDescription getCustomEffect(RulesetEffectPower power_effect);
    }


    public class HighestAbilityScorePower : FeatureDefinitionPower, ICustomPowerAbilityScore
    {
        public List<string> validAbilityScores = new List<string>();

        public string getPowerAbilityScore(RulesetCharacter character)
        {
            int max_value = 0;
            string max_stat = "";

            foreach (var a in validAbilityScores)
            {
                var val = character.GetAttribute(a).currentValue;
                if (val > max_value)
                {
                    max_value = val;
                    max_stat = a;
                }
            }

            return max_stat;
        }
    }


    public class PowerWithContextFromCondition : PowerWithRestrictions, ICustomPowerAbilityScore, ICustomPowerEffectBasedOnCaster
    {
        public ConditionDefinition condition;
        public List<(int, EffectDescription)> levelEffectList = new List<(int, EffectDescription)>();
        public int minCustomEffectLevel = 100;

        public EffectDescription getCustomEffect(RulesetEffectPower power_effect)
        {
            RulesetEffectSpell spell_effect = getParentSpellEffect(power_effect.User);

            if (spell_effect == null)
            {
                return this.effectDescription;
            }

            if (spell_effect.slotLevel < minCustomEffectLevel)
            {
                return this.effectDescription;
            }

            foreach (var e in levelEffectList)
            {
                if (spell_effect.slotLevel <= e.Item1)
                {
                    return e.Item2;
                }
            }

            return this.effectDescription;
        }


        RulesetEffectSpell getParentSpellEffect(RulesetCharacter character)
        {
            RulesetEffectSpell spell_effect = null;
            foreach (var cc in character.ConditionsByCategory)
            {
                foreach (var c in cc.Value)
                {
                    if (c.ConditionDefinition == condition)
                    {
                        spell_effect = Helpers.Misc.findConditionParentEffect(c) as RulesetEffectSpell;
                        break;
                    }
                }
            }

            return spell_effect;
        }

        public string getPowerAbilityScore(RulesetCharacter character)
        {
            RulesetEffectSpell spell_effect = getParentSpellEffect(character);

            if (spell_effect == null)
            {
                return this.abilityScore;
            }

            var stat = spell_effect.SpellRepertoire?.SpellCastingFeature?.SpellcastingAbility;
            if (stat != string.Empty)
            {
                return stat;
            }
            else
            {
                return this.abilityScore;
            }
        }
    }


    public class PowerWithRestrictions : LinkedPower, IPowerRestriction
    {
        public List<IRestriction> restrictions = new List<IRestriction>();
        public bool checkReaction = false;

        public bool isForbidden(RulesetActor character)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return true;
                }
            }
            return false;
        }

        public bool isReactionForbidden(RulesetActor character)
        {
            return checkReaction ? isForbidden(character) : false;
        }
    }


    public class PowerWithRestrictionsAndCasterLevelDependentEffect : PowerWithRestrictions, ICustomPowerEffectBasedOnCaster
    {
        public List<(int, EffectDescription)> levelEffectList = new List<(int, EffectDescription)>();
        public int minCustomEffectLevel = 100;

        public EffectDescription getCustomEffect(RulesetEffectPower power_effect)
        {
            int caster_level = power_effect.GetClassLevel(power_effect.User);
            if (caster_level < minCustomEffectLevel)
            {
                return this.effectDescription;
            }

            foreach (var e in levelEffectList)
            {
                if (caster_level <= e.Item1)
                {
                    return e.Item2;
                }
            }

            return this.effectDescription;
        }
    }


    public interface IHiddenAbility
    {
        bool isHidden();
    }


    public class HiddenPower : PowerWithRestrictions, IHiddenAbility
    {
        public bool isHidden()
        {
            return true;
        }
    }

    public class RerollFailedSavePower : PowerWithRestrictions, IHiddenAbility
    {
        public bool isHidden()
        {
            return true;
        }
    }



    public interface IPowerBundle
    {
        List<RulesetUsablePower> getAvailablePowers(RulesetCharacter character);
        List<FeatureDefinitionPower> getAllPowers();
        void addSubPower(FeatureDefinitionPower power);
        bool containsSubpower(FeatureDefinitionPower power);
    }



    static public class PowerBundleData
    {
        static private Dictionary<SpellDefinition, FeatureDefinitionPower> spell_power_map = new Dictionary<SpellDefinition, FeatureDefinitionPower>();
        static private Dictionary<FeatureDefinitionPower, SpellDefinition> power_spell_map = new Dictionary<FeatureDefinitionPower, SpellDefinition>();

        static public void registerPower(FeatureDefinitionPower custom_power)
        {
            if (power_spell_map.ContainsKey(custom_power))
            {
                throw new System.Exception(custom_power.name + " already registered");
            }
            var custom_spell = Helpers.CopyFeatureBuilder<SpellDefinition>
                                    .createFeatureCopy("Spell" + custom_power.name,
                                                 GuidStorage.mergeGuids(custom_power.guid, "33b5a52d-b646-4df8-b686-8b2783e73ef3"),
                                                 Common.common_no_title,
                                                 Common.common_no_title,
                                                 Common.common_no_icon,
                                                 DatabaseHelper.SpellDefinitions.AcidArrow);
            spell_power_map[custom_spell] = custom_power;
            power_spell_map[custom_power] = custom_spell;
        }

        static public FeatureDefinitionPower findPowerFromSpell(SpellDefinition spell)
        {
            if (spell_power_map.ContainsKey(spell))
            {
                return spell_power_map[spell];
            }
            return null;
        }


        static public SpellDefinition findSpellFromPower(FeatureDefinitionPower power)
        {
            if (power_spell_map.ContainsKey(power))
            {
                return power_spell_map[power];
            }
            return null;
        }


        static public bool registeredPower(FeatureDefinitionPower power)
        {
            return power_spell_map.ContainsKey(power);
        }
    }


    public class PowerBundle : PowerWithRestrictions, IPowerBundle
    {
        private HashSet<FeatureDefinitionPower> availablePowers = new HashSet<FeatureDefinitionPower>();

        public List<RulesetUsablePower> getAvailablePowers(RulesetCharacter character)
        {
            var powers = Helpers.Misc.getNonOverridenPowers(character,
                                                            u =>
                                                            {
                                                                if (!availablePowers.Contains(u.PowerDefinition))
                                                                {
                                                                    return false;
                                                                }

                                                                /*if (((u.powerDefinition as IHiddenAbility)?.isHidden()).GetValueOrDefault())
                                                                {
                                                                    return false;
                                                                }*/
                                                                if (((u.powerDefinition as PowerWithRestrictions)?.isForbidden(character)).GetValueOrDefault())
                                                                {
                                                                    return false;
                                                                }
                                                                return true;
                                                            });
            return powers.ToList();
        }

        public void addSubPower(FeatureDefinitionPower power)
        {
            if (!PowerBundleData.registeredPower(power))
            {
                PowerBundleData.registerPower(power);
                availablePowers.Add(power);
            }

            if (!PowerBundleData.registeredPower(this))
            {
                PowerBundleData.registerPower(this);
            }
        }

        public bool containsSubpower(FeatureDefinitionPower power)
        {
            return availablePowers.Contains(power);
        }

        public List<FeatureDefinitionPower> getAllPowers()
        {
            return availablePowers.ToList();
        }
    }
}
