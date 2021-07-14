using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface ICustomEffectBasedOnCaster
    {
        EffectDescription getCustomEffect(RulesetImplementationDefinitions.ApplyFormsParams formsParams);
    }

    public interface IPerformAttackAfterMagicEffectUse
    {
        void performAttackAfterUse(CharacterActionMagicEffect action_magic_effect);
        bool canUse(GameLocationCharacter character, GameLocationCharacter target);
    }


    public class SpellWithCasterLevelDependentEffects:  SpellDefinition, ICustomEffectBasedOnCaster
    {
        public List<(int, EffectDescription)> levelEffectList = new List<(int, EffectDescription)>();
        public int minCustomEffectLevel = 100;

        public EffectDescription getCustomEffect(RulesetImplementationDefinitions.ApplyFormsParams formsParams)
        {
            int caster_level = formsParams.classLevel;
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


    public class SpellWithCasterFeatureDependentEffects : SpellDefinition, ICustomEffectBasedOnCaster
    {
        public List<(List<FeatureDefinition>, EffectDescription)> featuresEffectList = new List<(List<FeatureDefinition>, EffectDescription)>();

        public EffectDescription getCustomEffect(RulesetImplementationDefinitions.ApplyFormsParams formsParams)
        {
            var caster = formsParams.sourceCharacter;

            var caster_features = Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinition>(caster).ToHashSet();

            foreach (var fe in featuresEffectList)
            {
                bool is_ok = true;
                foreach (var f in fe.Item1)
                {
                    if (!caster_features.Contains(f))
                    {
                        is_ok = false;
                        break;
                    }
                }

                if (is_ok)
                {
                    return fe.Item2;
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
