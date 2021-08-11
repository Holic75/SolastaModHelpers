using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IApplyEffectOnTargetSavingthrowRoll
    {
        void processSavingthrow(RulesetCharacter caster,
                                RulesetActor target,
                                BaseDefinition source,
                                RuleDefinitions.RollOutcome saveOutcome);
    }

    public interface IInitiatorApplyEffectOnTargetKill
    {
        void processTargetKill(RulesetCharacter attacker, RulesetCharacter target);
    }


    public interface IInitiatorApplyEffectOnCharacterDeath
    {
        void processDeath(RulesetCharacter attacker, RulesetCharacter target);
    }


    public interface ICasterApplyEffectOnEffectApplication
    {
        void processCasterEffectApplication(RulesetCharacter character, List<EffectForm> effectForms, RulesetImplementationDefinitions.ApplyFormsParams formsParams);
    }


    public interface ITargetApplyEffectOnEffectApplication
    {
        void processTargetEffectApplication(RulesetCharacter target, List<EffectForm> effectForms, RulesetImplementationDefinitions.ApplyFormsParams formsParams);
    }


    public interface IApplyEffectOnConditionApplication
    {
        void processConditionApplication(RulesetActor target, ConditionDefinition condition, RulesetImplementationDefinitions.ApplyFormsParams fromParams);
    }


    public interface ICasterApplyEffectOnConditionApplication
    {
        void processCasterConditionApplication(RulesetActor target, RulesetCondition condition);
    }


    public interface IApplyEffectOnPowerUse
    {
        void processPowerUse(RulesetCharacter character, RulesetUsablePower power);
    }


    public interface IApplyEffectOnConditionRemoval
    {
        void processConditionRemoval(RulesetActor actor, ConditionDefinition condition);
    }


    public interface IInitiatorApplyEffectOnAttack
    {
        void processAttackInitiator(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode);
    }


    public interface IInitiatorApplyEffectOnAttackHit
    {
        void processAttackHitInitiator(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, 
                                        int attackRoll,
                                        int successDelta,
                                        bool ranged);
    }

    public interface ITargetApplyEffectOnDamageTaken
    {
        void processDamageTarget(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier modifier, List<EffectForm> effect_forms);
    }


    public interface IInitiatorApplyEffectOnDamageDone
    {
        void processDamageInitiator(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier modifier, RulesetAttackMode attackMode, bool rangedAttack, bool isSpell);
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


    public class TargetRemoveConditionIfAffectedByHostileNonCaster : FeatureDefinition, ITargetApplyEffectOnEffectApplication
    {
        public ConditionDefinition condition;

        public void processTargetEffectApplication(RulesetCharacter target, List<EffectForm> effectForms, RulesetImplementationDefinitions.ApplyFormsParams formsParams)
        {

            var caster = formsParams.sourceCharacter;
            if (caster == null)
            {
                return;
            }

            if (caster.side == target.side)
            {
                return;
            }

            foreach (var conditions in target.ConditionsByCategory.Values.ToArray())
            {
                foreach (var c in conditions.ToArray())
                {
                    if (c.ConditionDefinition == condition && c.SourceGuid != caster.Guid)
                    {
                        target.RemoveCondition(c, false, true);
                    }
                }
            }
        }
    }



    public class HealAtTurnEndIfHasConditionBasedOnCasterLevel: FeatureDefinition, IApplyEffectOnTurnEnd
    {
        public ConditionDefinition casterCondition;
        public List<(int, int)> levelHealing;
        public CharacterClassDefinition characterClass;
        public bool allowParentConditions;

        public void processTurnEnd(GameLocationCharacter character)
        {
            var target = character.RulesetCharacter;
            foreach (var cc in target.conditionsByCategory)
            {
                foreach (var c in cc.Value)
                {
                    if (c.conditionDefinition == casterCondition || (allowParentConditions && c.conditionDefinition.parentCondition == casterCondition))
                    {
                        var caster = RulesetEntity.GetEntity<RulesetCharacter>(c.sourceGuid) as RulesetCharacterHero;
                        if (caster == null )
                        {
                            continue;
                        }
                        if (!caster.classesAndLevels.ContainsKey(characterClass))
                        {
                            continue;
                        }
                        var lvl = caster.classesAndLevels[characterClass];
                        foreach (var lh in levelHealing)
                        {
                            if (lvl <= lh.Item1)
                            {
                                int amount = Math.Min(target.GetAttribute("HitPoints").CurrentValue - target.CurrentHitPoints, lh.Item2);
                                if (amount != 0)
                                {
                                    target.ReceiveHealing(amount, true, c.sourceGuid);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
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


    public class InitiatorApplyPowerToSelfOnTargetSlain: FeatureDefinition, IInitiatorApplyEffectOnTargetKill
    {
        public FeatureDefinitionPower power;
        public CharacterClassDefinition scaleClass;

        public void processTargetKill(RulesetCharacter attacker, RulesetCharacter target)
        {
            CharacterActionParams actionParams;

            var attacker_game_location_character = Helpers.Misc.findGameLocationCharacter(attacker);
            if (attacker_game_location_character == null)
            {
                return;
            }

            actionParams = new CharacterActionParams(attacker_game_location_character, ActionDefinitions.Id.PowerNoCost, attacker_game_location_character);

            RulesetUsablePower usablePower = new RulesetUsablePower(power, (CharacterRaceDefinition)null, scaleClass);
            IRulesetImplementationService service = ServiceRepository.GetService<IRulesetImplementationService>();
            actionParams.RulesetEffect = (RulesetEffect)service.InstantiateEffectPower(attacker, usablePower, false);
            actionParams.StringParameter = power.Name;
            ServiceRepository.GetService<IGameLocationActionService>().ExecuteInstantSingleAction(actionParams);
        }
    }


    public class ProvideConditionForTurnDuration: FeatureDefinition, IApplyEffectOnTurnStart, IApplyEffectOnTurnEnd
    {
        public ConditionDefinition condition;
        public List<IRestriction> restrictions = new List<IRestriction>();

        public void processTurnEnd(GameLocationCharacter character)
        {
            foreach (var conditions in character.RulesetCharacter.ConditionsByCategory.Values.ToArray())
            {
                foreach (var c in conditions.ToArray())
                {
                    if (c.ConditionDefinition == condition)
                    {
                        character.RulesetCharacter.RemoveCondition(c, true, true);
                    }
                }
            }
        }

        public void processTurnStart(GameLocationCharacter character)
        {

            foreach (var r in restrictions)
            {
                if (r.isForbidden(character.RulesetCharacter))
                {
                    return;
                }
            }

            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(character.RulesetCharacter.Guid,
                                                                                       this.condition, RuleDefinitions.DurationType.Round, 1, RuleDefinitions.TurnOccurenceType.EndOfTurn,
                                                                                       character.RulesetCharacter.Guid,
                                                                                       character.RulesetCharacter.CurrentFaction.Name);
            character.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
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


    public class ApplyConditionOnPowerUseToSelf : FeatureDefinition, IApplyEffectOnPowerUse
    {
        public ConditionDefinition condition;
        public int durationValue;
        public RuleDefinitions.DurationType durationType;
        public RuleDefinitions.TurnOccurenceType turnOccurence;
        public List<FeatureDefinitionPower> powers;

        public void processPowerUse(RulesetCharacter character, RulesetUsablePower usablePower)
        {
            if (!powers.Contains(usablePower?.PowerDefinition))
            {
                return;
            }

            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(character.Guid,
                                                               condition, durationType, durationValue, turnOccurence,
                                                               character.Guid,
                                                               character.CurrentFaction.Name);
            character.AddConditionOfCategory("CustomCondition", active_condition, true);
        }
    }


    public class ApplyConditionOnPowerUseToTarget : FeatureDefinition, IApplyEffectOnTargetSavingthrowRoll
    {
        public ConditionDefinition condition;
        public int durationValue;
        public RuleDefinitions.DurationType durationType;
        public RuleDefinitions.TurnOccurenceType turnOccurence;
        public FeatureDefinitionPower power;
        public bool onlyOnFailedSave;
        public bool onlyOnSucessfulSave;

        public void processSavingthrow(RulesetCharacter caster,
                                        RulesetActor target,
                                        BaseDefinition source,
                                        RuleDefinitions.RollOutcome saveOutcome)
        {
            if (target == null || caster == null)
            {
                return;
            }

            if (source != power)
            {
                return;
            }

            if (onlyOnFailedSave && (saveOutcome == RuleDefinitions.RollOutcome.Success || saveOutcome == RuleDefinitions.RollOutcome.CriticalSuccess))
            {
                return;
            }
            if (onlyOnSucessfulSave && (saveOutcome == RuleDefinitions.RollOutcome.Failure || saveOutcome == RuleDefinitions.RollOutcome.CriticalFailure))
            {
                return;
            }

            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(target.Guid,
                                                                                       condition, durationType, durationValue, turnOccurence,
                                                                                       caster.Guid,
                                                                                       caster.CurrentFaction.Name);
            target.AddConditionOfCategory("CustomCondition", active_condition, true);
        }
    }


    public class InitiatorApplyConditionOnAttackToAttacker : FeatureDefinition, IInitiatorApplyEffectOnAttack
    {
        public ConditionDefinition condition;
        public int durationValue;
        public RuleDefinitions.DurationType durationType;
        public RuleDefinitions.TurnOccurenceType turnOccurence;

        public void processAttackInitiator(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode)
        {
            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(attacker.RulesetCharacter.Guid, 
                                                                                       condition, durationType, durationValue, turnOccurence,
                                                                                       attacker.RulesetCharacter.Guid,
                                                                                       attacker.RulesetCharacter.CurrentFaction.Name);
            attacker.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
        }
    }


    public class TargetApplyConditionOnDamageTaken : FeatureDefinition, ITargetApplyEffectOnDamageTaken
    {
        public ConditionDefinition condition;
        public int durationValue;
        public RuleDefinitions.DurationType durationType;
        public RuleDefinitions.TurnOccurenceType turnOccurence;


        public void processDamageTarget(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier modifier, List<EffectForm> effect_forms)
        {
            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(defender.RulesetCharacter.Guid,
                                                                                       condition, durationType, durationValue, turnOccurence,
                                                                                       defender.RulesetCharacter.Guid,
                                                                                       defender.RulesetCharacter.CurrentFaction.Name);
            defender.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
        }
    }


    public class InitiatorApplyConditionOnDamageDone : FeatureDefinition, IInitiatorApplyEffectOnDamageDone
    {
        public ConditionDefinition condition;
        public int durationValue;
        public RuleDefinitions.DurationType durationType;
        public RuleDefinitions.TurnOccurenceType turnOccurence;

        public bool onlyWeapon;

        public void processDamageInitiator(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier modifier, RulesetAttackMode attackMode, bool rangedAttack, bool isSpell)
        {
            if (onlyWeapon && isSpell)
            {
                return;
            }
            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(defender.RulesetCharacter.Guid,
                                                                                       condition, durationType, durationValue, turnOccurence,
                                                                                       attacker.RulesetCharacter.Guid,
                                                                                       attacker.RulesetCharacter.CurrentFaction.Name);
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


    public class InitiatorApplyConditionOnAttackToAttackerUntilTurnStart : FeatureDefinition, IInitiatorApplyEffectOnAttack, IApplyEffectOnTurnStart
    {
        public ConditionDefinition condition;
        public List<ConditionDefinition> extraConditionsToRemove = new List<ConditionDefinition>();

        public void processAttackInitiator(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode)
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


    public class RageWatcher : RemoveConditionOnTurnEndIfNoCondition, ITargetApplyEffectOnDamageTaken, IInitiatorApplyEffectOnAttack, IApplyEffectOnBattleEnd
    {
        public void processAttackInitiator(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attack_modifier, RulesetAttackMode attack_mode)
        {
            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(attacker.RulesetCharacter.Guid,
                                                                                       this.requiredCondition, RuleDefinitions.DurationType.Round, 1, RuleDefinitions.TurnOccurenceType.EndOfTurn,
                                                                                       attacker.RulesetCharacter.Guid,
                                                                                       attacker.RulesetCharacter.CurrentFaction.Name);
            attacker.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
        }

        public void processDamageTarget(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier modifier, List<EffectForm> effect_forms)
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


    public class GrantPowerOnConditionApplication : FeatureDefinition, IApplyEffectOnConditionApplication, IApplyEffectOnConditionRemoval
    {
        public FeatureDefinitionPower power;
        public ConditionDefinition condition;

        public void processConditionApplication(RulesetActor target, ConditionDefinition applied_condition, RulesetImplementationDefinitions.ApplyFormsParams fromParams)
        {
            if (applied_condition != condition)
            {
                return;
            }

            var character = target as RulesetCharacter;
            if (character == null)
            {
                return;
            }

            if (character.UsablePowers.Any(p => p.powerDefinition == power))
            {
                return;
            }

            var usable_power = new RulesetUsablePower(power, null, null);
            character.UsablePowers.Add(usable_power);
            usable_power.Recharge();
        }


        public void processConditionRemoval(RulesetActor actor, ConditionDefinition removed_condition)
        {
            if (removed_condition != condition)
            {
                return;
            }

            var character = actor as RulesetCharacter;
            if (character == null)
            {
                return;
            }

            character.UsablePowers.RemoveAll(p => p.powerDefinition == power);

        }
    }



    public class RemoveConditionsOnConditionApplication: FeatureDefinition, IApplyEffectOnConditionApplication
    {
        public List<ConditionDefinition> appliedConditions;
        public List<ConditionDefinition> removeConditions;

        public void processConditionApplication(RulesetActor target, ConditionDefinition applied_condition, RulesetImplementationDefinitions.ApplyFormsParams fromParams)
        {
            if (!appliedConditions.Contains(applied_condition))
            {
                return;
            }

            foreach (var conditions in target.ConditionsByCategory.Values.ToArray())
            {
                foreach (var c in conditions.ToArray())
                {
                    if (removeConditions.Contains(c.conditionDefinition))
                    {
                        target.RemoveCondition(c, true, true);
                    }
                }
            }
        }
    }


    public class AddExtraConditionToTargetOnConditionApplication : FeatureDefinition, ICasterApplyEffectOnConditionApplication
    {
        public ConditionDefinition requiredCondition;
        public ConditionDefinition extraCondition;

        public void processCasterConditionApplication(RulesetActor target, RulesetCondition condition)
        {
            if (requiredCondition != condition?.conditionDefinition)
            {
                return;
            }

            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(target.Guid,
                                                               this.extraCondition, RuleDefinitions.DurationType.Permanent, 1, RuleDefinitions.TurnOccurenceType.EndOfTurn,
                                                               condition.sourceGuid,
                                                               condition.sourceFactionName);
            target.AddConditionOfCategory("CustomCondition", active_condition, true);
        }
    }


    public class IncreaseMonsterHitPointsToTargetOnConditionApplication : FeatureDefinition, ICasterApplyEffectOnConditionApplication
    {
        public ConditionDefinition requiredCondition;
        public int hdMultiplier = 1;

        public void processCasterConditionApplication(RulesetActor target, RulesetCondition condition)
        {
            if (requiredCondition != condition?.conditionDefinition)
            {
                return;
            }
            var monster = target as RulesetCharacterMonster;
            if (monster == null)
            {
                return;
            }
            int val = hdMultiplier * monster.monsterDefinition.hitDice;
            monster.GetAttribute("HitPoints").AddModifier(RulesetAttributeModifier.BuildAttributeModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive,
                                                                                                         val, "CustomCondition"));
            monster.CurrentHitPoints += hdMultiplier * monster.monsterDefinition.hitDice;
        }
    }



    public class FrenzyWatcher : FeatureDefinition, IApplyEffectOnConditionRemoval
    {
        public List<ConditionDefinition> requiredConditions;
        public ConditionDefinition targetCondition;
        public ConditionDefinition afterCondition;

        public void processConditionRemoval(RulesetActor actor, ConditionDefinition condition)
        {
            var character = actor as RulesetCharacter;
            if (requiredConditions.Contains(condition) && character != null)
            {
                performConditionRemoval(character);
            }
        }


        void performConditionRemoval(RulesetCharacter actor)
        {
            foreach (var conditions in actor.ConditionsByCategory.Values.ToArray())
            {
                foreach (var c in conditions.ToArray())
                {
                    if (c.ConditionDefinition == targetCondition)
                    {
                        actor.RemoveCondition(c, true, true);
                    }
                }
            }

            if (afterCondition == null)
            {
                return;
            }

            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(actor.Guid,
                                                                           this.afterCondition, RuleDefinitions.DurationType.UntilShortRest, 1, RuleDefinitions.TurnOccurenceType.EndOfTurn,
                                                                           actor.Guid,
                                                                           actor.CurrentFaction.Name);
            actor.AddConditionOfCategory("10Combat", active_condition, true);
        }
    }
}
