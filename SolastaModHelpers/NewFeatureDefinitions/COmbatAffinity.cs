using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class RecklessAttack : FeatureDefinition, ICombatAffinityProvider
    {
        public string attackStat;

        public RuleDefinitions.SituationalContext SituationalContext => RuleDefinitions.SituationalContext.None;

        public bool AutoCritical => false;

        public bool CriticalHitImmunity => false;

        public ConditionDefinition RequiredTargetCondition => null;

        public bool IgnoreCover => false;

        public void ComputeAttackModifier(RulesetCharacter myself, RulesetCharacter defender, RulesetAttackMode attackMode, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
            if (attackMode.AbilityScore != attackStat)
            {
                return;
            }
            attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
        }

        public void ComputeDefenseModifier(RulesetCharacter myself, RulesetCharacter attacker, int sustainedAttacks, bool defenderAlreadyAttackedByAttackerThisTurn, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
            attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
        }

        public RuleDefinitions.AdvantageType GetAdvantageOnOpportunityAttackOnMe(RulesetCharacter myself, RulesetCharacter attacker)
        {
            return RuleDefinitions.AdvantageType.None;
        }

        public bool IsImmuneToOpportunityAttack(RulesetCharacter myself, RulesetCharacter attacker)
        {
            return false;
        }
    }
}
