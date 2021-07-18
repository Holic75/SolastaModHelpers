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
                internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result,
                                                                        GameLocationBattleManager __instance,
                                                                        GameLocationCharacter attacker,
                                                                        GameLocationCharacter defender,
                                                                        ActionModifier attackModifier,
                                                                        RulesetAttackMode attackerAttackMode
                                                                        )
                {
                    while (__result.MoveNext())
                    {
                        yield return __result.Current;
                    }
                    var extra_events = Process(__instance, attacker, defender, attackModifier, attackerAttackMode);

                    while (extra_events.MoveNext())
                    {
                        yield return extra_events.Current;
                    }
                }


                internal static System.Collections.IEnumerator Process(GameLocationBattleManager __instance,
                                            GameLocationCharacter attacker,
                                            GameLocationCharacter defender,
                                            ActionModifier attackModifier,
                                            RulesetAttackMode attackerAttackMode)
                {
                    if (__instance.battle == null)
                    {
                        yield break;
                    }

                    var features = Helpers.Accessors.extractFeaturesHierarchically<IInitiatorApplyEffectOnAttack>(attacker.RulesetCharacter);

                    foreach (var f in features)
                    {
                        f.processAttackInitiator(attacker, defender, attackModifier, attackerAttackMode);
                    }

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
                                yield return __instance.WaitForReactions(attacker, service2, count);
                            }
                        }
                    }
                }
            }
        }


        class GameLocationBattleManagerHandleCharacterAttackHitPatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterAttackHit")]
            internal static class GameLocationBattleManager_HandleCharacterAttackHit_Patch
            {
                internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result,
                                                                        GameLocationBattleManager __instance,
                                                                        GameLocationCharacter attacker,
                                                                        GameLocationCharacter defender,
                                                                        ActionModifier attackModifier,
                                                                        int attackRoll,
                                                                        int successDelta,
                                                                        bool ranged
                                                                        )
                {
                    while (__result.MoveNext())
                    {
                        yield return __result.Current;
                    }
                    var extra_events = Process(__instance, attacker, defender, attackModifier, attackRoll, successDelta, ranged);

                    while (extra_events.MoveNext())
                    {
                        yield return extra_events.Current;
                    }
                }


                internal static System.Collections.IEnumerator Process(GameLocationBattleManager __instance,
                                            GameLocationCharacter attacker,
                                            GameLocationCharacter defender,
                                            ActionModifier attackModifier,
                                            int attackRoll,
                                            int successDelta,
                                            bool ranged)
                {
                    if (__instance.battle == null)
                    {
                        yield break;
                    }

                    var features = Helpers.Accessors.extractFeaturesHierarchically<IInitiatorApplyEffectOnAttackHit>(attacker.RulesetCharacter);

                    foreach (var f in features)
                    {
                        f.processAttackHitInitiator(attacker, defender, attackModifier, attackRoll, successDelta, ranged);
                    }
                }
            }
        }


        class GameLocationBattleManagerHandleCharacterMagicalDamagePatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterMagicalAttackDamage")]
            internal static class GameLocationBattleManager_HandleCharacterMagicalAttackDamage_Patch
            {
                internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result,
                                                                         GameLocationBattleManager __instance,
                                                                        GameLocationCharacter attacker,
                                                                        GameLocationCharacter defender,
                                                                        ActionModifier magicModifier,
                                                                        RulesetEffect activeEffect,
                                                                        List<EffectForm> actualEffectForms,
                                                                        bool firstTarget)
                {
                    var features = Helpers.Accessors.extractFeaturesHierarchically<IAdditionalDamageProvider>(attacker.RulesetCharacter);
                    foreach (FeatureDefinition featureDefinition1 in features)
                    {
                        FeatureDefinition featureDefinition = featureDefinition1;
                        IAdditionalDamageProvider provider = featureDefinition as IAdditionalDamageProvider;
                        bool validTrigger = false;
                        bool validUses = true;
                        if ((uint)provider.LimitedUsage > 0U)
                        {
                            if (provider.LimitedUsage == RuleDefinitions.FeatureLimitedUsage.OnceInMyturn && (attacker.UsedSpecialFeatures.ContainsKey(featureDefinition.Name) || __instance.Battle != null && __instance.Battle.ActiveContender != attacker))
                                validUses = false;
                            else if (provider.LimitedUsage == RuleDefinitions.FeatureLimitedUsage.OncePerTurn && attacker.UsedSpecialFeatures.ContainsKey(featureDefinition.Name))
                                validUses = false;
                        }
                        if (validUses)
                        {
                            Main.Logger.Log("Checking: " + (activeEffect is RulesetEffectSpell).ToString());
                            if (provider.TriggerCondition == (RuleDefinitions.AdditionalDamageTriggerCondition)ExtendedEnums.AdditionalDamageTriggerCondition.RadiantOrFireSpellDamage 
                                && activeEffect is RulesetEffectSpell
                                && Helpers.Misc.hasDamageType(activeEffect.EffectDescription.effectForms, Helpers.DamageTypes.Fire, Helpers.DamageTypes.Radiant)
                                )
                            {
                                if (((activeEffect.EffectDescription.RangeType != RuleDefinitions.RangeType.Distance ? 0 : (!activeEffect.EffectDescription.IsAoE ? 1 : 0)) & (firstTarget ? 1 : 0)) != 0)
                                    validTrigger = true;
                                else if (activeEffect.EffectDescription.RangeType != RuleDefinitions.RangeType.Distance || activeEffect.EffectDescription.IsAoE)
                                    validTrigger = true;
                            }
                            if (validTrigger)
                                __instance.ComputeAndNotifyAdditionalDamage(attacker, defender, provider, actualEffectForms, (CharacterActionParams)null);
                        }
                        provider = (IAdditionalDamageProvider)null;
                        featureDefinition = (FeatureDefinition)null;
                    }

                    while (__result.MoveNext())
                    {
                        yield return __result.Current;
                    }
                    var extra_events = Process(__instance, attacker, defender, magicModifier, activeEffect, actualEffectForms, firstTarget);

                    while (extra_events.MoveNext())
                    {
                        yield return extra_events.Current;
                    }

                }


                internal static System.Collections.IEnumerator Process(GameLocationBattleManager __instance,
                            GameLocationCharacter attacker,
                            GameLocationCharacter defender,
                            ActionModifier magicModifier,
                            RulesetEffect activeEffect,
                            List<EffectForm> actualEffectForms,
                            bool firstTarget)
                {
                    if (__instance.battle == null)
                    {
                        yield break;
                    }

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
                                yield return __instance.WaitForReactions(attacker, service2, count);
                            }
                        }
                    }
                }
            }
        }


        class GameLocationBattleManagerHandleCharacterAttackDamagePatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterAttackDamage")]
            internal static class GameLocationBattleManager_HandleCharacterAttackDamage_Patch
            {
                internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result,
                                                                        GameLocationBattleManager __instance,
                                                                        GameLocationCharacter attacker,
                                                                        GameLocationCharacter defender,
                                                                        ActionModifier attackModifier,
                                                                        RulesetAttackMode attackMode,
                                                                        bool rangedAttack,
                                                                        RuleDefinitions.AdvantageType advantageType,
                                                                        List<EffectForm> actualEffectForms,
                                                                        bool isSpell)
                {
                    while (__result.MoveNext())
                    {
                        yield return __result.Current;
                    }
                    var extra_events = Process(__instance, attacker, defender, attackModifier, attackMode, rangedAttack, advantageType, actualEffectForms, isSpell);

                    while (extra_events.MoveNext())
                    {
                        yield return extra_events.Current;
                    }

                    var features = Helpers.Accessors.extractFeaturesHierarchically<IInitiatorApplyEffectOnDamageDone>(attacker.RulesetCharacter);

                    foreach (var f in features)
                    {
                        f.processDamageInitiator(attacker, defender, attackModifier, attackMode, rangedAttack, isSpell);
                    }
                }


                internal static System.Collections.IEnumerator Process(GameLocationBattleManager __instance,
                            GameLocationCharacter attacker,
                            GameLocationCharacter defender,
                            ActionModifier attackModifier,
                            RulesetAttackMode attackMode,
                            bool rangedAttack,
                            RuleDefinitions.AdvantageType advantageType,
                            List<EffectForm> actualEffectForms,
                            bool isSpell)
                {
                    if (__instance.battle == null)
                    {
                        yield break;
                    }


                    if (attackMode != null 
                        && attackMode.Ranged
                        && defender.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction, ActionDefinitions.ActionScope.Battle, false) == ActionDefinitions.ActionStatus.Available
                        && Helpers.Accessors.extractFeaturesHierarchically<DeflectMissileCustom>(defender.RulesetCharacter).Any())
                    {
                        CharacterActionParams reactionParams = new CharacterActionParams(defender, (ActionDefinitions.Id)ExtendedActionId.DeflectMissileCustom);
                        reactionParams.ActionModifiers.Add(attackModifier);
                        reactionParams.TargetCharacters.Add(attacker);
                        IGameLocationActionService service = ServiceRepository.GetService<IGameLocationActionService>();
                        int count = service.PendingReactionRequestGroups.Count;
                        (service as GameLocationActionManager)?.AddInterruptRequest((ReactionRequest)new ReactionRequestDeflectMissileCustom(reactionParams));
                        yield return __instance.WaitForReactions(attacker, service, count);
                    }

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
                                yield return __instance.WaitForReactions(attacker, service2, count);
                            }
                        }
                    }
                }
            }
        }


        class GameLocationBattleManagerHandleFailedSavingThrowAgainstEffectPatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleFailedSavingThrowAgainstEffect")]
            internal static class GameLocationBattleManager_HandleFailedSavingThrowAgainstEffect_Patch
            {
                internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result,
                                                                        GameLocationBattleManager __instance,
                                                                        CharacterActionMagicEffect action,
                                                                        GameLocationCharacter caster,
                                                                        GameLocationCharacter defender,
                                                                        RulesetEffect rulesetEffect,
                                                                        ActionModifier saveModifier,
                                                                        bool hasHitVisual
                                                                        )
                {
                    while (__result.MoveNext())
                    {
                        yield return __result.Current;
                    }
                    var extra_events = Process(__instance, action, caster, defender, rulesetEffect, saveModifier, hasHitVisual);

                    while (extra_events.MoveNext())
                    {
                        yield return extra_events.Current;
                    }
                }


                internal static System.Collections.IEnumerator Process(GameLocationBattleManager __instance,
                                                                        CharacterActionMagicEffect action,
                                                                        GameLocationCharacter caster,
                                                                        GameLocationCharacter defender,
                                                                        RulesetEffect rulesetEffect,
                                                                        ActionModifier saveModifier,
                                                                        bool hasHitVisual)
                {
                    if (action.SaveOutcome != RuleDefinitions.RollOutcome.CriticalFailure || action.SaveOutcome != RuleDefinitions.RollOutcome.Failure)
                    {
                        yield return null;
                    }
                    var power = defender.RulesetCharacter.UsablePowers.Where(u => u.PowerDefinition is NewFeatureDefinitions.RerollFailedSavePower
                                                      && defender.RulesetCharacter.GetRemainingUsesOfPower(u) > 0
                                                     ).FirstOrDefault();
                    if (power != null)
                    {
                        var reactionParams = new CharacterActionParams(defender, (ActionDefinitions.Id)ExtendedActionId.ConsumePowerUse);
                        reactionParams.RulesetEffect = rulesetEffect;
                        reactionParams.UsablePower = power;
                        IGameLocationActionService service = ServiceRepository.GetService<IGameLocationActionService>();
                        int count = service.PendingReactionRequestGroups.Count;
                        (service as GameLocationActionManager)?.AddInterruptRequest(new ReactionRequestConsumePowerUse(reactionParams));
                        yield return (object)__instance.WaitForReactions(defender, service, count);
                        if (reactionParams.ReactionValidated)
                        {
                            RuleDefinitions.RollOutcome saveOutcome = RuleDefinitions.RollOutcome.Neutral;
                            action.RolledSaveThrow = rulesetEffect.TryRollSavingThrow(action.ActingCharacter.RulesetCharacter, action.ActingCharacter.Side, defender.RulesetActor, saveModifier, hasHitVisual, out saveOutcome);
                            action.SaveOutcome = saveOutcome;
                        }
                    }
                }
            }
        }
    }
}
