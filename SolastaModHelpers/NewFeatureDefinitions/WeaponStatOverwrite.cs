using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface ICanUSeDexterityWithWeapon
    {
        bool worksOn(RulesetCharacter character, WeaponDescription weapon_description);
    }

    public interface IAttackAbilityScoreModeModifier
    {
        void applyAbilityScoreModification(RulesetCharacterHero character, RulesetAttackMode attack_mode, RulesetItem weapon);
    }

    public class canUseDexterityWithSpecifiedWeaponTypes : FeatureDefinition, ICanUSeDexterityWithWeapon
    {
        public List<string> weaponTypes;
        public List<IRestriction> restrictions = new List<IRestriction>();

        public bool worksOn(RulesetCharacter character, WeaponDescription weapon_description)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return false;
                }
            }

            return weaponTypes.Contains(weapon_description.WeaponType);
        }
    }


    public class ReplaceWeaponAbilityScoreForRangedOrFinessableWeapons : FeatureDefinition, IAttackAbilityScoreModeModifier
    {
        public List<string> abilityScores = new List<string>();

        public void applyAbilityScoreModification(RulesetCharacterHero character, RulesetAttackMode attack_mode, RulesetItem weapon)
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

            if (!description.WeaponTags.Contains("Finesse") 
                && DatabaseRepository.GetDatabase<WeaponTypeDefinition>().GetElement(description.WeaponType).WeaponProximity == RuleDefinitions.AttackProximity.Melee)
            {
                return;
            }

            var current_value = character.GetAttribute(attack_mode.AbilityScore).CurrentValue;
            var current_stat = attack_mode.AbilityScore;

            foreach (var a in abilityScores)
            {
                var new_val = character.GetAttribute(a).CurrentValue;
                if (new_val > current_value)
                {
                    current_value = new_val;
                    current_stat = a;
                }
            }

            if (current_stat != attack_mode.AbilityScore)
            {
                attack_mode.AbilityScore = current_stat;
            }

        }
    }


    public class ReplaceWeaponAbilityScoreWithHighestStatIfWeaponHasFeature : FeatureDefinition, IAttackAbilityScoreModeModifier
    {
        public FeatureDefinition weaponFeature;
        public List<string> abilityScores = new List<string>();

        public void applyAbilityScoreModification(RulesetCharacterHero character, RulesetAttackMode attack_mode, RulesetItem weapon)
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
            if (weapon == null || !weapon.dynamicItemProperties.Any(d => d.featureDefinition == weaponFeature))
            {
                return;
            }

            var current_value = character.GetAttribute(attack_mode.AbilityScore).CurrentValue;
            var current_stat = attack_mode.AbilityScore;

            foreach (var a in abilityScores)
            {
                var new_val = character.GetAttribute(a).CurrentValue;
                if (new_val > current_value)
                {
                    current_value = new_val;
                    current_stat = a;
                }
            }
            
            if (current_stat != attack_mode.AbilityScore)
            {             
                attack_mode.AbilityScore = current_stat;         
            }

        }
    }
}
