using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class DisadvantageOnWeaponAttack : FeatureDefinition, ICombatAffinityProvider
    {
        public RuleDefinitions.SituationalContext SituationalContext => RuleDefinitions.SituationalContext.None;
        public bool AutoCritical => false;
        public bool CriticalHitImmunity => false;
        public ConditionDefinition RequiredTargetCondition => null;
        public bool IgnoreCover => false;
        public bool CanRageToOvercomeSurprise => false;
        public bool onlyMelee = false;

        public void ComputeAttackModifier(RulesetCharacter myself, RulesetCharacter defender, RulesetAttackMode attackMode, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
            if ((attackMode?.sourceDefinition as MonsterAttackDefinition)?.itemDefinitionMainHand != null
                || (attackMode?.sourceDefinition as MonsterAttackDefinition)?.ItemDefinitionOffHand != null
                || ((attackMode?.sourceDefinition as ItemDefinition)?.IsWeapon).GetValueOrDefault())
            {
                if (onlyMelee && attackMode.ranged)
                {
                    return;
                }
                attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(-1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
            }
        }

        public void ComputeDefenseModifier(RulesetCharacter myself, RulesetCharacter attacker, int sustainedAttacks, bool defenderAlreadyAttackedByAttackerThisTurn, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
           
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



    public class ObscuredByDarkness : FeatureDefinition, ICombatAffinityProvider
    {
        public RuleDefinitions.SituationalContext SituationalContext => RuleDefinitions.SituationalContext.None;
        public bool AutoCritical => false;
        public bool CriticalHitImmunity => false;
        public ConditionDefinition RequiredTargetCondition => null;
        public bool IgnoreCover => false;
        public bool CanRageToOvercomeSurprise => false;
        public List<FeatureDefinition> ignore_features;

        public void ComputeAttackModifier(RulesetCharacter myself, RulesetCharacter defender, RulesetAttackMode attackMode, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
            bool defender_ignore = ignore_features.Any(f => Helpers.Misc.characterHasFeature(defender, f));
            bool attacker_ignore = ignore_features.Any(f => Helpers.Misc.characterHasFeature(myself, f));

            if (defender_ignore && attacker_ignore)
            {
                return;
            }

            if (attacker_ignore)
            {
                attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
            }
            else
            {
                attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(-1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
            }        
        }

        public void ComputeDefenseModifier(RulesetCharacter myself, RulesetCharacter attacker, int sustainedAttacks, bool defenderAlreadyAttackedByAttackerThisTurn, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
            bool defender_ignore = ignore_features.Any(f => Helpers.Misc.characterHasFeature(myself, f));
            bool attacker_ignore = ignore_features.Any(f => Helpers.Misc.characterHasFeature(attacker, f));

            if (defender_ignore && attacker_ignore)
            {
                return;
            }

            if (attacker_ignore)
            {
                attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
            }
            else
            {
                attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(-1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
            }
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

    public class RecklessAttack : FeatureDefinition, ICombatAffinityProvider
    {
        public string attackStat;
        public RuleDefinitions.SituationalContext SituationalContext => RuleDefinitions.SituationalContext.None;
        public bool AutoCritical => false;
        public bool CriticalHitImmunity => false;
        public ConditionDefinition RequiredTargetCondition => null;
        public bool IgnoreCover => false;
        public bool CanRageToOvercomeSurprise => false;

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



    public class AttackDisadvantageAgainstNonCaster : FeatureDefinition, ICombatAffinityProvider
    {
        public ConditionDefinition condition;
        public RuleDefinitions.SituationalContext SituationalContext => RuleDefinitions.SituationalContext.None;
        public bool AutoCritical => false;
        public bool CriticalHitImmunity => false;
        public ConditionDefinition RequiredTargetCondition => null;
        public bool IgnoreCover => false;
        public bool CanRageToOvercomeSurprise => false;

        public void ComputeAttackModifier(RulesetCharacter myself, RulesetCharacter defender, RulesetAttackMode attackMode, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
            if (myself.ConditionsByCategory.Any(kv => kv.Value != null && kv.Value.Any(c => c.conditionDefinition == condition && c.SourceGuid != defender.guid)))
            {
                attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(-1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
            }
        }

        public void ComputeDefenseModifier(RulesetCharacter myself, RulesetCharacter attacker, int sustainedAttacks, bool defenderAlreadyAttackedByAttackerThisTurn, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
            return;
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


    public class OpportunityAttackImmunityIfAttackerHasConditionFromCaster : FeatureDefinition, ICombatAffinityProvider
    {
        public ConditionDefinition condition;
        public RuleDefinitions.SituationalContext SituationalContext => RuleDefinitions.SituationalContext.None;
        public bool CanRageToOvercomeSurprise => false;
        public bool AutoCritical => false;
        public bool CriticalHitImmunity => false;
        public ConditionDefinition RequiredTargetCondition => null;
        public bool IgnoreCover => false;

        public void ComputeAttackModifier(RulesetCharacter myself, RulesetCharacter defender, RulesetAttackMode attackMode, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
            return;
        }

        public void ComputeDefenseModifier(RulesetCharacter myself, RulesetCharacter attacker, int sustainedAttacks, bool defenderAlreadyAttackedByAttackerThisTurn, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin)
        {
            return;
        }

        public RuleDefinitions.AdvantageType GetAdvantageOnOpportunityAttackOnMe(RulesetCharacter myself, RulesetCharacter attacker)
        {
            return RuleDefinitions.AdvantageType.None;
        }

        public bool IsImmuneToOpportunityAttack(RulesetCharacter myself, RulesetCharacter attacker)
        {
            return attacker.conditionsByCategory.Any(kv => kv.Value.Any(c => c.sourceGuid == myself.Guid && c.conditionDefinition == condition));
        }
    }
}
