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

    public class WeaponDamageBonusWithSpecificStat: FeatureDefinition, IAttackModeModifier
    {
        public int value;
        public string attackStat;

        public void apply(RulesetCharacterHero character, RulesetAttackMode attack_mode, RulesetItem weapon)
        {
            if (weapon == null || attack_mode.AbilityScore != attackStat)
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
}
