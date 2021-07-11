using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface ICustomEffectBasedOnCasterLevel
    {
        EffectDescription getCustomEffect(int caster_level);
    }

    public interface IPerformAttackAfterMagicEffectUse
    {
        void performAttackAfterUse(CharacterActionMagicEffect action_magic_effect);
        bool canUse(GameLocationCharacter character, GameLocationCharacter target);
    }


    public class SpellWithCasterLevelDependentEffects:  SpellDefinition, ICustomEffectBasedOnCasterLevel
    {
        public List<(int, EffectDescription)> levelEffectList = new List<(int, EffectDescription)>();
        public int minCustomEffectLevel;

        public EffectDescription getCustomEffect(int caster_level)
        {
            if (caster_level < minCustomEffectLevel)
            {
                return this.effectDescription;
            }

            foreach (var e in levelEffectList)
            {
                if (caster_level <= e.Item1)
                {
                    return e.Item2;
                }
            }

            return this.effectDescription;
        }
    }

    public class SpellFollowedByMeleeAttack : SpellWithCasterLevelDependentEffects, IPerformAttackAfterMagicEffectUse
    {
        public bool canUse(GameLocationCharacter caster, GameLocationCharacter target)
        {
            var attack_mode = caster.FindActionAttackMode(ActionDefinitions.Id.AttackMain);
            if (attack_mode == null)
            {
                return false;
            }

            IGameLocationBattleService gameLocationBattleService = ServiceRepository.GetService<IGameLocationBattleService>();
            if (gameLocationBattleService == null)
            {
                return false;
            }

            ActionModifier attackModifier = new ActionModifier();
            BattleDefinitions.AttackEvaluationParams attackEvalParams = new BattleDefinitions.AttackEvaluationParams();
            attackEvalParams.FillForPhysicalReachAttack(caster, caster.LocationPosition, attack_mode, target, target.LocationPosition, attackModifier);
            if (gameLocationBattleService.CanAttack(attackEvalParams))
            {
                return true;
            }
            return false;
        }

        public void performAttackAfterUse(CharacterActionMagicEffect action_magic_effect)
        {
            var action_params = action_magic_effect?.actionParams;
            if (action_params == null)
            {
                return;
            }

            if (action_magic_effect.Countered || action_magic_effect.ExecutionFailed)
            {
                return;
            }
            var caster = action_params.actingCharacter;
            if (caster == null || action_params.targetCharacters.Count != 1)
            {
                return;
            }

            var target = action_params.targetCharacters[0];
            if (target == null)
            {
                return;
            }


            var attack_mode = caster.FindActionAttackMode(ActionDefinitions.Id.AttackMain);
            if (attack_mode == null)
            {
                return;
            }
            
            IGameLocationBattleService gameLocationBattleService = ServiceRepository.GetService<IGameLocationBattleService>();
            IGameLocationActionService gameLocationActionService = ServiceRepository.GetService<IGameLocationActionService>();
            CharacterActionParams attackActionParams = new CharacterActionParams(caster, ActionDefinitions.Id.AttackFree);
            attackActionParams.AttackMode = attack_mode;
            ActionModifier attackModifier = new ActionModifier();
            BattleDefinitions.AttackEvaluationParams attackEvalParams = new BattleDefinitions.AttackEvaluationParams();
            attackEvalParams.FillForPhysicalReachAttack(caster, caster.LocationPosition, attack_mode, target, target.LocationPosition, attackModifier);
            if (gameLocationBattleService.CanAttack(attackEvalParams))
            {
                attackActionParams.TargetCharacters.Add(target);
                attackActionParams.ActionModifiers.Add(attackModifier);
                gameLocationActionService.ExecuteAction(attackActionParams, (CharacterAction.ActionExecutedHandler)null, true);
            }
            target = (GameLocationCharacter)null;
            attackActionParams = (CharacterActionParams)null;
            attackModifier = (ActionModifier)null;
            attackEvalParams = new BattleDefinitions.AttackEvaluationParams();
        }
    }

}
