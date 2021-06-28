using HarmonyLib;
using SolastaModHelpers.NewFeatureDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class GameLocationBattleManagerPatcher
    {
        class GameLocationBattleManagerHandleReactionToDamagePatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleReactionToDamage")]
            internal static class GameLocationBattleManager_HandleReactionToDamage_Patch
            {


                internal static void Postfix(GameLocationBattleManager __instance,
                                            GameLocationCharacter attacker,
                                            GameLocationCharacter defender,
                                            ActionModifier modifier,
                                            List<EffectForm> effectForms,
                                            ref System.Collections.IEnumerator __result)
                {
                    var hero_character = defender.RulesetCharacter;
                    if (hero_character != null)
                    {
                        var features = Helpers.Accessors.extractFeaturesHierarchically<ITargetApplyEffectOnDamageTaken>(hero_character);
                        foreach (var f in features)
                        {
                            f.processDamageTarget(attacker, defender, modifier, effectForms);
                        }
                    }
                }
            }
        }

        class GameLocationBattleManagerHandleCharacterAttackPatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterAttack")]
            internal static class GameLocationBattleManager_HandleCharacterAttack_Patch
            {
                internal static void Postfix(GameLocationBattleManager __instance,
                                                            GameLocationCharacter attacker,
                                                            GameLocationCharacter defender,
                                                            ActionModifier attackModifier,
                                                            RulesetAttackMode attackerAttackMode,
                                                            ref System.Collections.IEnumerator __result)
                {
                    if (__instance.battle == null)
                    {
                        return;
                    }

                    var hero_character = attacker.RulesetCharacter;
                    if (hero_character != null)
                    {
                        var features = Helpers.Accessors.extractFeaturesHierarchically<IInitiatorApplyEffectOnAttack>(hero_character);

                        foreach (var f in features)
                        {
                            f.processAttackInitiator(attacker, defender, attackModifier, attackerAttackMode);
                        }
                    }

                    List<System.Collections.IEnumerator> extra_events = new List<System.Collections.IEnumerator>();

                    var units = __instance.Battle.AllContenders;                   
                    foreach (GameLocationCharacter unit in units)
                    {
                        if (!unit.RulesetCharacter.IsDeadOrDyingOrUnconscious 
                            && unit.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction, ActionDefinitions.ActionScope.Battle, false) == ActionDefinitions.ActionStatus.Available)
                        {
                            var powers = unit.RulesetCharacter.UsablePowers.Where(u => u.PowerDefinition is NewFeatureDefinitions.IReactionPowerOnAttackAttempt 
                                                                                  && unit.RulesetCharacter.GetRemainingUsesOfPower(u) > 0
                                                                                  && (u.PowerDefinition as NewFeatureDefinitions.IReactionPowerOnAttackAttempt)
                                                                                    .canBeUsed(unit, attacker, defender, attackerAttackMode)
                                                                                 ).ToArray();
                            var overriden_powers = powers.Aggregate(new List<FeatureDefinitionPower>(), (old, next) =>
                                                                                 {
                                                                                     if (next.PowerDefinition?.overriddenPower != null)
                                                                                     {
                                                                                         old.Add(next.PowerDefinition?.overriddenPower);
                                                                                     }
                                                                                     return old;
                                                                                 });
                            powers = powers.Where(pp => !overriden_powers.Contains(pp.powerDefinition)).ToArray();

                            foreach (var p in powers)
                            {
                                CharacterActionParams reactionParams = new CharacterActionParams(unit, (ActionDefinitions.Id)ExtendedActionId.ModifyAttackRollViaPower);
                                reactionParams.TargetCharacters.Add(attacker);
                                reactionParams.TargetCharacters.Add(defender);
                                reactionParams.ActionModifiers.Add(attackModifier);
                                reactionParams.AttackMode = attackerAttackMode;
                                reactionParams.UsablePower = p;
                                IRulesetImplementationService service1 = ServiceRepository.GetService<IRulesetImplementationService>();
                                reactionParams.RulesetEffect = (RulesetEffect)service1.InstantiateEffectPower(attacker.RulesetCharacter, p, false);
                                reactionParams.StringParameter = p.PowerDefinition.Name;                                
                                reactionParams.IsReactionEffect = true;
                                IGameLocationActionService service2 = ServiceRepository.GetService<IGameLocationActionService>();
                                int count = service2.PendingReactionRequestGroups.Count;
                                (service2 as GameLocationActionManager)?.AddInterruptRequest((ReactionRequest)new ReactionRequestUsePower(reactionParams, "ModifyAttackRollViaPower"));
                                extra_events.Add(__instance.WaitForReactions(attacker, service2, count));
                            }
                        }
                    }

                    var all_events = new List<object>();
                    while (__result.MoveNext())
                    {
                        all_events.Add(__result.Current);
                    }
                    all_events.AddRange(extra_events);
                    __result = Helpers.Accessors.convertToEnumerator(all_events);
                }
            }
        }


        class GameLocationBattleManagerHandleCharacterMagicalDamagePatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterMagicalAttackDamage")]
            internal static class GameLocationBattleManager_HandleCharacterMagicalAttackDamage_Patch
            {
                internal static void Postfix(GameLocationBattleManager __instance,
                                            GameLocationCharacter attacker,
                                            GameLocationCharacter defender,
                                            ActionModifier magicModifier,
                                            RulesetEffect activeEffect,
                                            List<EffectForm> actualEffectForms,
                                            bool firstTarget,
                                            ref System.Collections.IEnumerator __result)
                {
                    if (__instance.battle == null)
                    {
                        return;
                    }

                    List<System.Collections.IEnumerator> extra_events = new List<System.Collections.IEnumerator>();

                    var units = __instance.Battle.AllContenders;
                    foreach (GameLocationCharacter unit in units)
                    {
                        if (!unit.RulesetCharacter.IsDeadOrDyingOrUnconscious
                            && unit.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction, ActionDefinitions.ActionScope.Battle, false) == ActionDefinitions.ActionStatus.Available)
                        {
                            var powers = unit.RulesetCharacter.UsablePowers.Where(u => u.PowerDefinition is NewFeatureDefinitions.IReactionPowerOnDamage
                                                                                  && unit.RulesetCharacter.GetRemainingUsesOfPower(u) > 0
                                                                                  && (u.PowerDefinition as NewFeatureDefinitions.IReactionPowerOnDamage)
                                                                                    .canBeUsed(unit, attacker, defender, null, true)
                                                                                 ).ToArray();

                            var overriden_powers = powers.Aggregate(new List<FeatureDefinitionPower>(), (old, next) =>
                            {
                                if (next.PowerDefinition?.overriddenPower != null)
                                {
                                    old.Add(next.PowerDefinition?.overriddenPower);
                                }
                                return old;
                            });
                            powers = powers.Where(pp => !overriden_powers.Contains(pp.powerDefinition)).ToArray();

                            foreach (var p in powers)
                            {
                                CharacterActionParams reactionParams = new CharacterActionParams(unit, ActionDefinitions.Id.PowerReaction);
                                reactionParams.TargetCharacters.Add(attacker);
                                reactionParams.ActionModifiers.Add(new ActionModifier());
                                IRulesetImplementationService service1 = ServiceRepository.GetService<IRulesetImplementationService>();
                                reactionParams.RulesetEffect = (RulesetEffect)service1.InstantiateEffectPower(defender.RulesetCharacter, p, false);
                                reactionParams.StringParameter = p.PowerDefinition.Name;
                                reactionParams.IsReactionEffect = true;
                                IGameLocationActionService service2 = ServiceRepository.GetService<IGameLocationActionService>();
                                int count = service2.PendingReactionRequestGroups.Count;
                                service2.ReactToUsePower(reactionParams);
                                extra_events.Add(__instance.WaitForReactions(attacker, service2, count));
                            }
                        }
                    }

                    var all_events = new List<object>();
                    while (__result.MoveNext())
                    {
                        all_events.Add(__result.Current);
                    }
                    all_events.AddRange(extra_events);
                    __result = Helpers.Accessors.convertToEnumerator(all_events);
                }
            }
        }


        class GameLocationBattleManagerHandleCharacterAttackDamagePatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterAttackDamage")]
            internal static class GameLocationBattleManager_HandleCharacterAttackDamage_Patch
            {
                internal static void Postfix(GameLocationBattleManager __instance,
                                            GameLocationCharacter attacker,
                                            GameLocationCharacter defender,
                                            ActionModifier attackModifier,
                                            RulesetAttackMode attackMode,
                                            bool rangedAttack,
                                            RuleDefinitions.AdvantageType advantageType,
                                            List<EffectForm> actualEffectForms,
                                            ref System.Collections.IEnumerator __result)
                {
                    if (__instance.battle == null)
                    {
                        return;
                    }

                    List<System.Collections.IEnumerator> extra_events = new List<System.Collections.IEnumerator>();

                    var units = __instance.Battle.AllContenders;
                    foreach (GameLocationCharacter unit in units)
                    {
                        if (!unit.RulesetCharacter.IsDeadOrDyingOrUnconscious
                            && unit.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction, ActionDefinitions.ActionScope.Battle, false) == ActionDefinitions.ActionStatus.Available)
                        {
                            var powers = unit.RulesetCharacter.UsablePowers.Where(u => u.PowerDefinition is NewFeatureDefinitions.IReactionPowerOnDamage
                                                                                  && unit.RulesetCharacter.GetRemainingUsesOfPower(u) > 0
                                                                                  && (u.PowerDefinition as NewFeatureDefinitions.IReactionPowerOnDamage)
                                                                                    .canBeUsed(unit, attacker, defender, attackMode, false)
                                                                                 ).ToArray();

                            var overriden_powers = powers.Aggregate(new List<FeatureDefinitionPower>(), (old, next) =>
                            {
                                if (next.PowerDefinition?.overriddenPower != null)
                                {
                                    old.Add(next.PowerDefinition?.overriddenPower);
                                }
                                return old;
                            });
                            powers = powers.Where(pp => !overriden_powers.Contains(pp.powerDefinition)).ToArray();

                            foreach (var p in powers)
                            {
                                CharacterActionParams reactionParams = new CharacterActionParams(unit, ActionDefinitions.Id.PowerReaction);
                                reactionParams.TargetCharacters.Add(attacker);
                                reactionParams.ActionModifiers.Add(new ActionModifier());
                                IRulesetImplementationService service1 = ServiceRepository.GetService<IRulesetImplementationService>();
                                reactionParams.RulesetEffect = (RulesetEffect)service1.InstantiateEffectPower(defender.RulesetCharacter, p, false);
                                reactionParams.StringParameter = p.PowerDefinition.Name;
                                reactionParams.IsReactionEffect = true;
                                IGameLocationActionService service2 = ServiceRepository.GetService<IGameLocationActionService>();
                                int count = service2.PendingReactionRequestGroups.Count;
                                service2.ReactToUsePower(reactionParams);
                                extra_events.Add(__instance.WaitForReactions(attacker, service2, count));
                            }
                        }
                    }

                    var all_events = new List<object>();
                    while (__result.MoveNext())
                    {
                        all_events.Add(__result.Current);
                    }
                    all_events.AddRange(extra_events);
                    __result = Helpers.Accessors.convertToEnumerator(all_events);
                }
            }
        }
    }
}
