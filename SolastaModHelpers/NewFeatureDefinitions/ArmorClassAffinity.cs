using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IDefenseAffinity
    {
        void computeDefenseModifier(
              RulesetCharacter myself,
              RulesetCharacter attacker,
              int sustained_attacks,
              bool defender_already_attacked_by_attacker_this_turn,
              ActionModifier attack_modifier,
              RulesetAttackMode attack_mode);
    }


    public class ArmorBonusAgainstAttackType : FeatureDefinition, IDefenseAffinity
    {
        public int value;
        public bool applyToMelee;
        public bool applyToRanged;
        public List<ConditionDefinition> requiredConditions = new List<ConditionDefinition>();

        public void computeDefenseModifier(RulesetCharacter myself, RulesetCharacter attacker, int sustained_attacks, bool defender_already_attacked_by_attacker_this_turn, ActionModifier attack_modifier, RulesetAttackMode attack_mode)
        {
            if (attack_mode?.SourceDefinition == null)
            {
                return;
            }

            if (attack_mode.Ranged && !applyToRanged)
            {
                return;
            }

            if (!attack_mode.Ranged && !applyToMelee)
            {
                return;
            }

            bool condition_ok = requiredConditions.Empty();
            foreach (var c in requiredConditions)
            {
                if (myself.HasConditionOfType(c))
                {
                    condition_ok = true;
                    break;
                }
            }

            if (!condition_ok)
            {
                return;
            }

            attack_modifier.AttackRollModifier -= value;
            attack_modifier.AttacktoHitTrends.Add(new RuleDefinitions.TrendInfo(-value, RuleDefinitions.FeatureSourceType.CharacterFeature, this.Name, (object)null));
        }
    }
}
