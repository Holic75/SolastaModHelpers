﻿namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IReactionPowerOnAttackAttempt
    {
        bool canBeUsed(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, RulesetAttackMode attack_mode);
    }


    public interface IReactionPowerOnDamage
    {
        bool canBeUsed(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, RulesetAttackMode attack_mode, bool is_magic);
    }


    public class FeatureDefinitionReactionPowerOnDamage : LinkedPower, IReactionPowerOnDamage
    {
        public bool worksOnMelee;
        public bool worksOnRanged;
        public bool worksOnMagic;

        bool IReactionPowerOnDamage.canBeUsed(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, RulesetAttackMode attack_mode, bool is_magic)
        {
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

            if (!is_magic)
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

            return true;
        }
    }


    public class FeatureDefinitionReactionPowerOnAttackAttempt : LinkedPower, IReactionPowerOnAttackAttempt
    {
        public bool worksOnMelee;
        public bool worksOnRanged;

        bool IReactionPowerOnAttackAttempt.canBeUsed(GameLocationCharacter caster, GameLocationCharacter attacker, GameLocationCharacter defender, RulesetAttackMode attack_mode)
        {
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

            if (attack_mode.Ranged && !worksOnRanged)
            {
                return false;
            }

            if (!attack_mode.Ranged && !worksOnMelee)
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

            return true;
        }
    }
}
