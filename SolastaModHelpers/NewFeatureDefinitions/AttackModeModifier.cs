using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IAttackModeModifier
    {
        void apply(RulesetCharacter character, RulesetAttackMode attack_mode, RulesetItem weapon);
    }


    public class AddAttackTagForSpecificWeaponType : FeatureDefinition, IAttackModeModifier
    {
        public List<string> weaponTypes = new List<string>();
        public string tag;

        public void apply(RulesetCharacter character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
            var weapon2 = weapon?.itemDefinition ?? (attack_mode.sourceDefinition as ItemDefinition);
            if (weapon2 == null || !weapon2.isWeapon)
            {
                return;
            }

            var description = weapon2.WeaponDescription;
            if (description == null)
            {
                return;
            }


            if (!weaponTypes.Empty() && !weaponTypes.Contains(description.WeaponType))
            {
                return;
            }

            attack_mode.AddAttackTagAsNeeded(tag);
        }
    }


    public class WeaponDamageBonusWithSpecificStat: FeatureDefinition, IAttackModeModifier
    {
        public int value;
        public string attackStat;

        public void apply(RulesetCharacter character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
            var weapon2 = weapon?.itemDefinition ?? (attack_mode.sourceDefinition as ItemDefinition);
            if (weapon2 == null || !weapon2.isWeapon)
            {
                return;
            }

            if (attack_mode.AbilityScore != attackStat)
            {
                return;
            }

            DamageForm first_Damage_form = attack_mode.EffectDescription.FindFirstDamageForm();
            if (first_Damage_form == null)
            {
                return;
            }

            first_Damage_form.BonusDamage += value;
            first_Damage_form.DamageBonusTrends.Add(new RuleDefinitions.TrendInfo(value, RuleDefinitions.FeatureSourceType.CharacterFeature,
                                                                                  this.Name,
                                                                                  null));

        }
    }


    public class OverwriteDamageOnSpecificWeaponTypesBasedOnClassLevel : FeatureDefinition, IAttackModeModifier
    {
        public CharacterClassDefinition characterClass;
        public List<(int, int, RuleDefinitions.DieType)> levelDamageList;
        public List<string> weaponTypes = new List<string>();
        public List<IRestriction> restrictions = new List<IRestriction>();

        public void apply(RulesetCharacter character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
            var hero = character as RulesetCharacterHero;
            if (hero == null)
            {
                return;
            }
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return;
                }
            }

            var weapon2 = weapon?.itemDefinition ?? (attack_mode.sourceDefinition as ItemDefinition);
            if (weapon2 == null || !weapon2.isWeapon)
            {
                return;
            }

            var description = weapon2.WeaponDescription;
            if (description == null)
            {
                return;
            }


            if (!weaponTypes.Empty() && !weaponTypes.Contains(description.WeaponType))
            {
                return;
            }


            if (!hero.ClassesAndLevels.ContainsKey(characterClass))
            {
                return;
            }

            var lvl = hero.ClassesAndLevels[characterClass];

            var damage = attack_mode?.EffectDescription?.FindFirstDamageForm();
            if (damage == null)
            {
                return;
            }

            foreach (var d in levelDamageList)
            {
                if (d.Item1 >= lvl)
                {
                    int old_damage = RuleDefinitions.DieAverage(damage.dieType) * damage.diceNumber;
                    int old_damage_versatile = RuleDefinitions.DieAverage(damage.versatileDieType) * damage.diceNumber;
                    int new_damage = RuleDefinitions.DieAverage(d.Item3) * d.Item2;
                    if (new_damage > old_damage)
                    {
                        damage.DieType = d.Item3;
                        damage.DiceNumber = d.Item2;
                    }
                    if (new_damage > old_damage_versatile)
                    {
                        damage.VersatileDieType = d.Item3;
                    }
                    break;
                }
            }

        }
    }


    public class AttackModeExtraMainAttackWithSpecificWeaponType : FeatureDefinition, IAttackModeModifier
    {
        public List<string> weaponTypes = new List<string>();
        public List<IRestriction> restrictions = new List<IRestriction>();

        public void apply(RulesetCharacter character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
            if (attack_mode.ActionType != ActionDefinitions.ActionType.Main)
            {
                return;
            }
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return;
                }
            }

            var weapon2 = weapon?.itemDefinition ?? (attack_mode.sourceDefinition as ItemDefinition);
            if (weapon2 == null || !weapon2.isWeapon)
            {
                return;
            }

            var description = weapon2.WeaponDescription;
            if (description == null)
            {
                return;
            }


            if (!weaponTypes.Empty() && !weaponTypes.Contains(description.WeaponType))
            {
                return;
            }

            attack_mode.AttacksNumber ++;
        }
    }



    public class OverwriteDamageOnWeaponWithFeature : FeatureDefinition, IAttackModeModifier
    {
        public FeatureDefinition weaponFeature;
        public int numDice;
        public RuleDefinitions.DieType dieType;

        public void apply(RulesetCharacter character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
            var weapon2 = weapon?.itemDefinition ?? (attack_mode.sourceDefinition as ItemDefinition);
            if (weapon2 == null || !weapon2.isWeapon)
            {
                return;
            }

            if (weapon == null || !weapon.dynamicItemProperties.Any(d => d.featureDefinition == weaponFeature))
            {
                return;
            }

            var damage = attack_mode?.EffectDescription?.FindFirstDamageForm();
            if (damage == null)
            {
                return;
            }
            int old_damage = RuleDefinitions.DieAverage(damage.dieType) * damage.diceNumber;
            int old_damage_versatile = RuleDefinitions.DieAverage(damage.versatileDieType) * damage.diceNumber;
            int new_damage = RuleDefinitions.DieAverage(dieType) * numDice;

            if (new_damage > old_damage)
            {
                damage.DieType = dieType;
                damage.DiceNumber = numDice;
            }
            if (new_damage > old_damage_versatile)
            {
                damage.VersatileDieType = dieType;
            }
        }
    }


    public class AddAttackTagonWeaponWithFeature : FeatureDefinition, IAttackModeModifier
    {
        public FeatureDefinition weaponFeature;
        public string tag;

        public void apply(RulesetCharacter character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
            var weapon2 = weapon?.itemDefinition ?? (attack_mode.sourceDefinition as ItemDefinition);
            if (weapon2 == null || !weapon2.isWeapon)
            {
                return;
            }

            if (weapon == null || !weapon.dynamicItemProperties.Any(d => d.featureDefinition == weaponFeature))
            {
                return;
            }

            attack_mode.AddAttackTagAsNeeded(tag);
        }
    }


    public class AddAttackTagIfHasFeature : FeatureDefinition, IAttackModeModifier
    {
        public FeatureDefinition requiredFeature;
        public string tag;

        public void apply(RulesetCharacter character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
            if (requiredFeature == null || Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinition>(character).Contains(requiredFeature))
            {
                attack_mode.AddAttackTagAsNeeded(tag);
            }
        }
    }


    public class AttackDamageBonusBasedOnCasterStat : FeatureDefinition, IAttackModeModifier
    {
        public string abilityScore;
        
        public void apply(RulesetCharacter character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
            var condition = character.FindFirstConditionHoldingFeature(this);
            if (condition == null)
            {
                return;
            }

            var caster = RulesetEntity.GetEntity<RulesetCharacter>(condition.sourceGuid);
            if (caster == null)
            {
                return;
            }

            var value =  AttributeDefinitions.ComputeAbilityScoreModifier(caster.GetAttribute(abilityScore).CurrentValue);

            attack_mode.ToHitBonus += value;
            attack_mode.ToHitBonusTrends.Add(new RuleDefinitions.TrendInfo(value, RuleDefinitions.FeatureSourceType.MonsterFeature, this.Name, this));

            DamageForm first_Damage_form = attack_mode.EffectDescription.FindFirstDamageForm();
            if (first_Damage_form != null)
            {
                first_Damage_form.BonusDamage += value;
                first_Damage_form.DamageBonusTrends.Add(new RuleDefinitions.TrendInfo(value, RuleDefinitions.FeatureSourceType.CharacterFeature,
                                                                                      this.Name,
                                                                                      this));
            }
        }
    }
}
