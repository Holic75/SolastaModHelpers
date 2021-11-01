using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IReactionPowerOnAttackAttempt
    {
        bool canBeUsedOnAttackAttempt(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode);
    }


    public interface IModifyFailedSavePower
    {
        bool canBeUsedOnFailedSave(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier save_modifier, RulesetEffect rulesetEffect);
        int getSavingThrowBonus(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier save_modifier, RulesetEffect rulesetEffect);
    }


    public interface IReactionPowerOnDamage
    {
        bool canBeUsedOnDamage(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, RulesetAttackMode attack_mode, bool is_magic);
    }

    public class FeatureDefinitionAddAbilityBonusOnFailedSavePower : NewFeatureDefinitions.LinkedPower, IModifyFailedSavePower, IHiddenAbility
    {
        public string ability;

        public bool canBeUsedOnFailedSave(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier save_modifier, RulesetEffect rulesetEffect)
        {
            var effect = this.EffectDescription;
            if (effect == null || caster == null || attacker == null)
            {
                return false;
            }

            int max_distance = this.EffectDescription.RangeParameter;

            if ((caster.LocationPosition - defender.LocationPosition).magnitude > max_distance)
            {
                return false;
            }

            bool works_on_caster = effect.TargetFilteringTag != (RuleDefinitions.TargetFilteringTag)ExtendedEnums.ExtraTargetFilteringTag.NonCaster;

            if (defender.Side != effect.TargetSide && effect.TargetSide != RuleDefinitions.Side.All)
            {
                return false;
            }


            if (!works_on_caster && defender == caster)
            {
                return false;
            }

            if (effect.targetType == RuleDefinitions.TargetType.Self && defender != caster)
            {
                return false;
            }

            return true;
        }

        public int getSavingThrowBonus(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier save_modifier, RulesetEffect rulesetEffect)
        {
            return Math.Max(1, AttributeDefinitions.ComputeAbilityScoreModifier(caster.RulesetCharacter.GetAttribute(ability).currentValue));
        }

        public bool isHidden()
        {
            return false;
        }
    }


    public class FeatureDefinitionAddRandomBonusOnFailedSavePower : NewFeatureDefinitions.LinkedPower, IModifyFailedSavePower, IHiddenAbility
    {
        public int diceNumber = 1;
        public RuleDefinitions.DieType dieType = RuleDefinitions.DieType.D1;


        public bool canBeUsedOnFailedSave(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier save_modifier, RulesetEffect rulesetEffect)
        {
            var effect = this.EffectDescription;
            if (effect == null || caster == null || attacker == null)
            {
                return false;
            }

            int max_distance = this.EffectDescription.RangeParameter;

            if ((caster.LocationPosition - defender.LocationPosition).magnitude > max_distance)
            {
                return false;
            }

            bool works_on_caster = effect.TargetFilteringTag != (RuleDefinitions.TargetFilteringTag)ExtendedEnums.ExtraTargetFilteringTag.NonCaster;

            if (defender.Side != effect.TargetSide && effect.TargetSide != RuleDefinitions.Side.All)
            {
                return false;
            }


            if (!works_on_caster && defender == caster)
            {
                return false;
            }

            if (effect.targetType == RuleDefinitions.TargetType.Self && defender != caster)
            {
                return false;
            }

            return true;
        }

        public int getSavingThrowBonus(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier save_modifier, RulesetEffect rulesetEffect)
        {
            int first_roll, second_roll;
            int bonus = 0;
            for (int i = 0; i < diceNumber; i++)
            {
                bonus += RuleDefinitions.RollDie(dieType, RuleDefinitions.AdvantageType.None, out first_roll, out second_roll, 0.0f);
            }
            return bonus;
        }

        public bool isHidden()
        {
            return true;
        }


    }


    public class FeatureDefinitionReactionPowerOnDamage: NewFeatureDefinitions.LinkedPower, IReactionPowerOnDamage, IHiddenAbility
    {
        public bool worksOnMelee;
        public bool worksOnRanged;
        public bool worksOnMagic;
        public List<ConditionDefinition> checkImmunityToCondtions = new List<ConditionDefinition>();

        public bool isHidden()
        {
            return true;
        }

        bool IReactionPowerOnDamage.canBeUsedOnDamage(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, RulesetAttackMode attack_mode, bool is_magic)
        {
            var effect = this.EffectDescription;
            if (effect == null || caster == null || attacker == null)
            {
                return false;
            }

            int max_distance = this.EffectDescription.RangeParameter;
            
            if ((caster.LocationPosition - attacker.LocationPosition).magnitude > max_distance)
            {
                return false;
            }

            bool works_on_caster = effect.TargetFilteringTag != (RuleDefinitions.TargetFilteringTag)ExtendedEnums.ExtraTargetFilteringTag.NonCaster;

            if (!is_magic && attack_mode == null)
            {
                return false;
            }
            else if (!is_magic)
            {
                if (attack_mode.Ranged && !worksOnRanged)
                {
                    return false;
                }

                if (!attack_mode.Ranged && !worksOnMelee)
                {
                    return false;
                }
            }

            if (is_magic && !worksOnMagic)
            {
                return false;
            }

            if (attacker.Side != effect.TargetSide && effect.TargetSide != RuleDefinitions.Side.All)
            {
                return false;
            }
            
            if (!works_on_caster && attacker == caster)
            {
                return false;
            }

            if (effect.targetType == RuleDefinitions.TargetType.Self && attacker != caster)
            {
                return false;
            }

            if (checkImmunityToCondtions.Any(c => attacker.rulesetActor.IsImmuneToCondition(c.name, 0)))
            {
                return false;
            }

            return true;
        }
    }




    public class FeatureDefinitionReactionPowerOnAttackAttempt : NewFeatureDefinitions.LinkedPower, IReactionPowerOnAttackAttempt, IHiddenAbility
    {
        public bool worksOnMelee;
        public bool worksOnRanged;
        public bool worksOnMagic;
        public bool onlyOnFailure;
        public bool onlyOnSuccess;

        public List<ConditionDefinition> checkImmunityToCondtions = new List<ConditionDefinition>();

        public bool isHidden()
        {
            return true;
        }

        bool IReactionPowerOnAttackAttempt.canBeUsedOnAttackAttempt(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode)
        {
            var prerolled_data = AttackRollsData.getPrerolledData(attacker);

            if (onlyOnSuccess && prerolled_data.outcome != RuleDefinitions.RollOutcome.Success)
            {
                return false;
            }

            if (onlyOnFailure && prerolled_data.outcome != RuleDefinitions.RollOutcome.Failure)
            {
                return false;
            }

            var effect = this.EffectDescription;
            if (effect == null)
            {
                return false;
            }

            int max_distance = this.EffectDescription.RangeParameter;

            if ((caster.LocationPosition - attacker.LocationPosition).magnitude > max_distance)
            {
                return false;
            }

            bool works_on_caster = effect.TargetFilteringTag != (RuleDefinitions.TargetFilteringTag)ExtendedEnums.ExtraTargetFilteringTag.NonCaster;

            if (attack_mode == null && !worksOnMagic)
            {
                return false;
            }

            if (attack_mode != null)
            {
                if (attack_mode.Ranged && !worksOnRanged)
                {
                    return false;
                }

                if (!attack_mode.Ranged && !worksOnMelee)
                {
                    return false;
                }
            }

            if (attacker.Side != effect.TargetSide && effect.TargetSide != RuleDefinitions.Side.All)
            {
                return false;
            }

            if (!works_on_caster && attacker == caster)
            {
                return false;
            }

            if (effect.targetType == RuleDefinitions.TargetType.Self && attacker != caster)
            {
                return false;
            }

            if (checkImmunityToCondtions.Any(c => attacker.rulesetActor.IsImmuneToCondition(c.name, 0)))
            {
                return false;
            }

            return true;
        }
    }
}
