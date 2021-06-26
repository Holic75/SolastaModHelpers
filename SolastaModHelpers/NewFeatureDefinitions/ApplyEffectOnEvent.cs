using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IApplyEffectOnConditionApplication
    {
        void processCondtionApplication(RulesetActor actor, ConditionDefinition condtion);
    }


    public interface IApplyEffectOnAttack
    {
        void processAttack(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode);
    }

    public interface IApplyEffectOnDamageTaken
    {
        void processDamage(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier modifier, List<EffectForm> effect_forms);
    }


    public interface IApplyEffectOnTurnStart
    {
        void processTurnStart(GameLocationCharacter character);
    }


    public interface IApplyEffectOnTurnEnd
    {
        void processTurnEnd(GameLocationCharacter character);
    }


    public interface IApplyEffectOnBattleEnd
    {
        void processBattleEnd(GameLocationCharacter character);
    }


    public abstract class ApplyPowerOnTurnEndBase : FeatureDefinition, IApplyEffectOnTurnEnd
    {
        abstract protected FeatureDefinitionPower getPower(GameLocationCharacter character);
        public void processTurnEnd(GameLocationCharacter character)
        {
            var power_to_apply = getPower(character);
            if (power_to_apply == null)
            {
                return;
            }

            CharacterActionParams actionParams;
            if (power_to_apply.EffectDescription.RangeType == RuleDefinitions.RangeType.Self)
            {
                actionParams = new CharacterActionParams(character, ActionDefinitions.Id.PowerNoCost);
            }
            else
            {
                actionParams = new CharacterActionParams(character, ActionDefinitions.Id.PowerNoCost, character);
            }
            RulesetUsablePower usablePower = new RulesetUsablePower(power_to_apply, (CharacterRaceDefinition)null, (CharacterClassDefinition)null);
            IRulesetImplementationService service = ServiceRepository.GetService<IRulesetImplementationService>();
            actionParams.RulesetEffect = (RulesetEffect)service.InstantiateEffectPower(character.RulesetCharacter, usablePower, false);
            actionParams.StringParameter = power_to_apply.Name;
            //actionParams.IsReactionEffect = true;
            ServiceRepository.GetService<IGameLocationActionService>().ExecuteInstantSingleAction(actionParams);
        }
    }

    public class ApplyPowerOnTurnEnd : ApplyPowerOnTurnEndBase
    {
        public FeatureDefinitionPower power;
        protected override FeatureDefinitionPower getPower(GameLocationCharacter character)
        {
            return power;
        }
    }


    public class ApplyPowerOnTurnEndBasedOnClassLevel : ApplyPowerOnTurnEndBase
    {
        public List<(int, FeatureDefinitionPower)> powerLevelList;
        public CharacterClassDefinition characterClass;
        public CharacterSubclassDefinition requiredSubclass = null;

        protected override FeatureDefinitionPower getPower(GameLocationCharacter character)
        {
            var hero = (character.RulesetCharacter as RulesetCharacterHero);

            if (hero == null)
            {
                return null;
            }

            if (!hero.ClassesAndLevels.ContainsKey(characterClass))
            {
                return null;
            }

            if (requiredSubclass != null && !hero.ClassesAndSubclasses.ContainsValue(requiredSubclass))
            {
                return null;
            }
            
            var level = hero.ClassesAndLevels[characterClass];

            foreach (var l in powerLevelList)
            {
                if (l.Item1 >= level)
                {
                    return l.Item2;
                }
            }
            return null;
        }
    }


    public class ApplyConditionOnAttackToAttacker : FeatureDefinition, IApplyEffectOnAttack
    {
        public ConditionDefinition condition;
        public int durationValue;
        public RuleDefinitions.DurationType durationType;
        public RuleDefinitions.TurnOccurenceType turnOccurence;

        public void processAttack(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode)
        {
            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(attacker.RulesetCharacter.Guid, 
                                                                                       condition, durationType, durationValue, turnOccurence,
                                                                                       attacker.RulesetCharacter.Guid,
                                                                                       attacker.RulesetCharacter.CurrentFaction.Name);
            attacker.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
        }
    }


    public class ApplyConditionOnDamageTakenToTarget : FeatureDefinition, IApplyEffectOnDamageTaken
    {
        public ConditionDefinition condition;
        public int durationValue;
        public RuleDefinitions.DurationType durationType;
        public RuleDefinitions.TurnOccurenceType turnOccurence;


        public void processDamage(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier modifier, List<EffectForm> effect_forms)
        {
            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(defender.RulesetCharacter.Guid,
                                                                                       condition, durationType, durationValue, turnOccurence,
                                                                                       defender.RulesetCharacter.Guid,
                                                                                       defender.RulesetCharacter.CurrentFaction.Name);
            defender.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
        }
    }


    public class RemoveConditionOnTurnEndIfNoCondition : FeatureDefinition, IApplyEffectOnTurnEnd
    {
        public ConditionDefinition requiredCondition;
        public ConditionDefinition conditionToRemove;

        public void processTurnEnd(GameLocationCharacter character)
        {
            var ruleset_character = character?.RulesetCharacter;
            if (ruleset_character == null)
            {
                return;
            }

            if (ruleset_character.ConditionsByCategory.Values.Any(c => c.Any(cc => cc.ConditionDefinition == requiredCondition)))
            {
                return;
            }

            foreach (var conditions in ruleset_character.ConditionsByCategory.Values.ToArray())
            {
                foreach (var c in conditions.ToArray())
                {
                    if (c.ConditionDefinition == conditionToRemove)
                    {
                        ruleset_character.RemoveCondition(c, true, true);
                    }
                }
            }
        }
    }


    public class RemoveConditionAtTurnStartIfNoCondition : FeatureDefinition, IApplyEffectOnTurnStart
    {
        public ConditionDefinition requiredCondition;
        public ConditionDefinition conditionToRemove;

        public void processTurnStart(GameLocationCharacter character)
        {
            var ruleset_character = character?.RulesetCharacter;
            if (ruleset_character == null)
            {
                return;
            }

            if (ruleset_character.ConditionsByCategory.Values.Any(c => c.Any(cc => cc.ConditionDefinition == requiredCondition)))
            {
                return;
            }

            foreach (var conditions in ruleset_character.ConditionsByCategory.Values.ToArray())
            {
                foreach (var c in conditions.ToArray())
                {
                    if (c.ConditionDefinition == conditionToRemove)
                    {
                        ruleset_character.RemoveCondition(c, true, true);
                    }
                }
            }
        }
    }


    public class ApplyConditionOnAttackToAttackerUnitUntilTurnStart : FeatureDefinition, IApplyEffectOnAttack, IApplyEffectOnTurnStart
    {
        public ConditionDefinition condition;
        public List<ConditionDefinition> extraConditionsToRemove = new List<ConditionDefinition>();

        public void processAttack(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode)
        {
            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(attacker.RulesetCharacter.Guid,
                                                                                       condition, RuleDefinitions.DurationType.Round, 1, RuleDefinitions.TurnOccurenceType.StartOfTurn,
                                                                                       attacker.RulesetCharacter.Guid,
                                                                                       attacker.RulesetCharacter.CurrentFaction.Name);
            attacker.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
        }

        public void processTurnStart(GameLocationCharacter character)
        {
            var ruleset_character = character?.RulesetCharacter;
            if (ruleset_character == null)
            {
                return;
            }

            foreach (var conditions in ruleset_character.ConditionsByCategory.Values.ToArray())
            {
                foreach (var c in conditions.ToArray())
                {
                    if (c.ConditionDefinition == condition || extraConditionsToRemove.Contains(c.conditionDefinition))
                    {
                        ruleset_character.RemoveCondition(c, true, true);
                    }
                }
            }
        }
    }


    public class RageWatcher : RemoveConditionOnTurnEndIfNoCondition, IApplyEffectOnDamageTaken, IApplyEffectOnAttack, IApplyEffectOnBattleEnd
    {
        public void processAttack(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode)
        {
            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(attacker.RulesetCharacter.Guid,
                                                                                       this.requiredCondition, RuleDefinitions.DurationType.Round, 1, RuleDefinitions.TurnOccurenceType.EndOfTurn,
                                                                                       attacker.RulesetCharacter.Guid,
                                                                                       attacker.RulesetCharacter.CurrentFaction.Name);
            attacker.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
        }

        public void processDamage(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier modifier, List<EffectForm> effect_forms)
        {
            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(defender.RulesetCharacter.Guid,
                                                                                       this.requiredCondition, RuleDefinitions.DurationType.Round, 1, RuleDefinitions.TurnOccurenceType.EndOfTurn,
                                                                                       defender.RulesetCharacter.Guid,
                                                                                       defender.RulesetCharacter.CurrentFaction.Name);
            defender.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
        }

        public void processBattleEnd(GameLocationCharacter character)
        {
            var ruleset_character = character?.RulesetCharacter;
            if (ruleset_character == null)
            {
                return;
            }

            foreach (var conditions in ruleset_character.ConditionsByCategory.Values.ToArray())
            {
                foreach (var c in conditions.ToArray())
                {
                    if (c.ConditionDefinition == conditionToRemove)
                    {
                        ruleset_character.RemoveCondition(c, true, true);
                    }
                }
            }
        }
    }



    public class RemoveConditionsOnConditionApplication: FeatureDefinition, IApplyEffectOnConditionApplication
    {
        public List<ConditionDefinition> appliedConditions;
        public List<ConditionDefinition> removeConditions;

        public void processCondtionApplication(RulesetActor actor, ConditionDefinition applied_condition)
        {
            if (!appliedConditions.Contains(applied_condition))
            {
                return;
            }

            foreach (var conditions in actor.ConditionsByCategory.Values.ToArray())
            {
                foreach (var c in conditions.ToArray())
                {
                    if (removeConditions.Contains(c.conditionDefinition))
                    {
                        actor.RemoveCondition(c, true, true);
                    }
                }
            }
        }
    }

}
