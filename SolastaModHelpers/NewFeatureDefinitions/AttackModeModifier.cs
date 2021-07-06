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
        void apply(RulesetCharacterHero character, RulesetAttackMode attack_mode, RulesetItem weapon);
    }


    public class AddAttackTagForSpecificWeaponType : FeatureDefinition, IAttackModeModifier
    {
        public List<string> weaponTypes = new List<string>();
        public string tag;

        public void apply(RulesetCharacterHero character, RulesetAttackMode attack_mode, RulesetItem weapon)
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

        public void apply(RulesetCharacterHero character, RulesetAttackMode attack_mode, RulesetItem weapon)
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

        public void apply(RulesetCharacterHero character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
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


            if (!character.ClassesAndLevels.ContainsKey(characterClass))
            {
                return;
            }

            var lvl = character.ClassesAndLevels[characterClass];

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
}
