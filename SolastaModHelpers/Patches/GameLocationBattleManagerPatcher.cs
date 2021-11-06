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

        class GameLocationBattleManagerHandleCharacterMoveEndPatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterMoveEnd")]
            internal static class GameLocationBattleManager_HandleReactionToDamage_Patch
            {
                internal static void Postfix(GameLocationBattleManager __instance,
                                             GameLocationCharacter mover)
                {
                    if (__instance.Battle == null)
                    {
                        return;
                    }

                    var monster = mover.RulesetCharacter as RulesetCharacterMonster;
                    if (monster != null && !monster.monsterDefinition.groupAttacks && monster.monsterDefinition.AttackIterations.Count() > 1)
                    {
                        monster.RefreshAttackModes(false);
                    }
                }
            }
        }


        class GameLocationBattleManagerHandleReactionToDamagePatcher
        {
            [HarmonyPatch(typeof(GameLocationBattleManager), "HandleReactionToDamage")]
            internal static class GameLocationBattleManager_HandleReactionToDamage_Patch
            {
                internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result,
                                                                       GameLocationBattleManager __instance,
                                                                        GameLocationCharacter attacker,
                                                                        GameLocationCharacter defender,
                                                                        ActionModifier modifier,
                                                                        List<EffectForm> effectForms)
                {
                    while (__result.MoveNext())
                    {
                        yield return __result.Current;
                    }
                    var extra_events = Process(__instance, attacker, defender, modifier, effectForms);

                    while (extra_events.MoveNext())
                    {
                        yield return extra_events.Current;
                    }
                }

                internal static System.Collections.IEnumerator Process(GameLocationBattleManager __instance,
                            GameLocationCharacter attacker,
                            GameLocationCharacter defender,
                            ActionModifier modifier,
                            List<EffectForm> effectForms)
                {
                    if (__instance.battle == null)
                    {
                        yield break;
                    }
                    var hero_character = defender.RulesetCharacter;
                    if (hero_character != null)
                    {
                        var features = Helpers.Accessors.extractFeaturesHierarchically<ITargetApplyEffectOnDamageTakenFromCreature>(hero_character);
                        foreach (var f in features)
                        {
                            f.processDamageTargetTakenFromCreature(attacker, defender, modifier, effectForms);
                        }
                    }

                    if (defender?.RulesetCharacter != null
                        && !defender.RulesetCharacter.IsDeadOrDyingOrUnconscious && defender.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction) == ActionDefinitions.ActionStatus.Available
                        && defender.GetActionStatus(ActionDefinitions.Id.CastReaction, ActionDefinitions.ActionScope.Battle, ActionDefinitions.ActionStatus.Available) == ActionDefinitions.ActionStatus.Available)
                    {
                        var spells = Helpers.Misc.filterCharacterSpells(defender.RulesetCharacter,
                                                                        s => ((s as IMagicEffectReactionOnDamageDoneToCaster)?.canUseMagicalEffectInReactionToDamageDoneToCaster(attacker, defender)).GetValueOrDefault()
                                                                        );
                        foreach (var s in spells)
                        {

                            if (defender.GetActionStatus(ActionDefinitions.Id.CastReaction, ActionDefinitions.ActionScope.Battle, ActionDefinitions.ActionStatus.Available) != ActionDefinitions.ActionStatus.Available)
                            {
                                break;
                            }
                            RulesetSpellRepertoire matchingRepertoire;
                            int slotLevel = defender.RulesetCharacter.GetLowestSlotLevelAndRepertoireToCastSpell(s, out matchingRepertoire);
                            if (matchingRepertoire == null || !defender.RulesetCharacter.AreSpellComponentsValid(s))
                            {
                                continue;
                            }

                            CharacterActionParams reactionParams = new CharacterActionParams(defender, ActionDefinitions.Id.CastReaction);
                            reactionParams.IntParameter = 1;
                            reactionParams.TargetCharacters.Add(attacker);
                            reactionParams.ActionModifiers.Add(new ActionModifier());
                            IRulesetImplementationService service1 = ServiceRepository.GetService<IRulesetImplementationService>();
                            reactionParams.RulesetEffect = (RulesetEffect)service1.InstantiateEffectSpell(defender.RulesetCharacter, matchingRepertoire, s, slotLevel, false);
                            reactionParams.IsReactionEffect = true;
                            GameLocationActionManager service2 = ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
                            if (service2 == null)
                            {
                                break;
                            }

                            int count = service2.PendingReactionRequestGroups.Count;
                            service2.AddInterruptRequest(new ReactionRequestCastSpellInResponseToAttack(reactionParams));
                            yield return (object)__instance.WaitForReactions(attacker, service2, count);
                        }
                    }
                }
            }
        }


        /*class CharacterActionMagicEffectExecuteMagicAttackPatcher
        {
            [HarmonyPatch(typeof(CharacterActionMagicEffect), "ExecuteMagicAttack")]
            internal static class CharacterActionMagicEffect_ExecuteMagicAttack_Patch
            {
                internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result,
                                                                       CharacterActionMagicEffect __instance,
                                                                        RulesetEffect activeEffect,
                                                                        GameLocationCharacter target,
                                                                        ActionModifier attackModifier,
                                                                        List<EffectForm> actualEffectForms,
                                                                        bool firstTarget,
                                                                        bool checkMagicalAttackDamage
                                                                        )
                {
                    var battleService = ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;
                    EffectDescription effectDescription = activeEffect.EffectDescription;
                    bool needToRollDie = effectDescription.RangeType == RuleDefinitions.RangeType.MeleeHit || effectDescription.RangeType == RuleDefinitions.RangeType.RangeHit;

                    if (battleService != null && needToRollDie)
                    {
                        var extra_events = GameLocationBattleManagerHandleCharacterAttackPatcher.GameLocationBattleManager_HandleCharacterAttack_Patch
                                    .Process(battleService, __instance?.ActingCharacter, target, attackModifier, null);

                        while (extra_events.MoveNext())
                        {
                            yield return extra_events.Current;
                        }
                    }

                    while (__result.MoveNext())
                    {
                        yield return __result.Current;
                    }

                }
            }
        }*/


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

                    if (!attackerAttackMode.AutomaticHit)
                    {
                        RuleDefinitions.RollOutcome outcome;
                        int delta = 0;
                        int attackRoll = attacker.RulesetCharacter.RollAttackMode(attackerAttackMode, defender.RulesetActor, attackerAttackMode.SourceDefinition,
                                                                                  attackModifier.AttacktoHitTrends, attackModifier.IgnoreAdvantage, attackModifier.AttackAdvantageTrends,
                                                                                  false, attackModifier.AttackRollModifier, out outcome, out delta, -1, true);
                        AttackRollsData.storePrerolledData(attacker, new AttackRollInfo(attackRoll, outcome));
                        //Main.Logger.Log($"Prerolling an attack for {attacker.Name}: {attackRoll} -> {outcome}");
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
                        if (!unit.RulesetCharacter.IsDeadOrDyingOrUnconscious)
                        {
                            var powers = unit.RulesetCharacter.UsablePowers.Where(u => u.PowerDefinition is NewFeatureDefinitions.IReactionPowerOnAttackAttempt
                                                                                  && unit.RulesetCharacter.GetRemainingUsesOfPower(u) > 0
                                                                                  && (u.PowerDefinition as NewFeatureDefinitions.IReactionPowerOnAttackAttempt)
                                                                                    .canBeUsedOnAttackAttempt(unit, attacker, defender, attackModifier, attackerAttackMode)
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
                                if (p.powerDefinition.activationTime == RuleDefinitions.ActivationTime.Reaction &&
                                    (unit.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction, ActionDefinitions.ActionScope.Battle, false) != ActionDefinitions.ActionStatus.Available
                                    || unit.GetActionStatus(ActionDefinitions.Id.PowerReaction, ActionDefinitions.ActionScope.Battle, ActionDefinitions.ActionStatus.Available) != ActionDefinitions.ActionStatus.Available)
                                    )
                                {
                                    continue;
                                }
                                CharacterActionParams reactionParams = new CharacterActionParams(unit, (ActionDefinitions.Id)ExtendedActionId.ModifyAttackRollViaPower);
                                reactionParams.TargetCharacters.Add(attacker);
                                reactionParams.TargetCharacters.Add(defender);
                                reactionParams.ActionModifiers.Add(attackModifier);
                                reactionParams.AttackMode = attackerAttackMode;
                                reactionParams.UsablePower = p;
                                IRulesetImplementationService service1 = ServiceRepository.GetService<IRulesetImplementationService>();
                                reactionParams.RulesetEffect = (RulesetEffect)service1.InstantiateEffectPower(unit.RulesetCharacter, p, false);
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
                            EffectDescription effectDescription = activeEffect.EffectDescription;
                            if (provider.TriggerCondition == (RuleDefinitions.AdditionalDamageTriggerCondition)ExtendedEnums.AdditionalDamageTriggerCondition.RadiantOrFireSpellDamage 
                                && activeEffect is RulesetEffectSpell
                                && Helpers.Misc.hasDamageType(actualEffectForms, Helpers.DamageTypes.Fire, Helpers.DamageTypes.Radiant)
                                )
                            {
                                validTrigger = true;   
                            }
                            if (provider.TriggerCondition == (RuleDefinitions.AdditionalDamageTriggerCondition)ExtendedEnums.AdditionalDamageTriggerCondition.CantripDamage
                                && activeEffect is RulesetEffectSpell
                                && (activeEffect as RulesetEffectSpell).spellDefinition != null && (activeEffect as RulesetEffectSpell).spellDefinition.spellLevel == 0
                                )
                            {
                                validTrigger = true;
                            }
                            else if ((effectDescription.RangeType == RuleDefinitions.RangeType.MeleeHit || effectDescription.RangeType == RuleDefinitions.RangeType.RangeHit)
                                      && provider.TriggerCondition == (RuleDefinitions.AdditionalDamageTriggerCondition)ExtendedEnums.AdditionalDamageTriggerCondition.MagicalAttacksOnTargetWithConditionFromMe
                                      && defender.RulesetActor.HasConditionOfTypeAndSource(provider.RequiredTargetCondition, attacker.Guid)
                                      )
                            {
                                validTrigger = true;
                            }

                            if (validTrigger)
                            {
                                __instance.ComputeAndNotifyAdditionalDamage(attacker, defender, provider, actualEffectForms, null);
                            }
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
                        if (!unit.RulesetCharacter.IsDeadOrDyingOrUnconscious)
                        {
                            var powers = unit.RulesetCharacter.UsablePowers.Where(u => u.PowerDefinition is NewFeatureDefinitions.IReactionPowerOnDamage
                                                                                  && unit.RulesetCharacter.GetRemainingUsesOfPower(u) > 0
                                                                                  && (u.PowerDefinition as NewFeatureDefinitions.IReactionPowerOnDamage)
                                                                                    .canBeUsedOnDamage(unit, attacker, defender, null, true)
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
                                if (p.powerDefinition.activationTime == RuleDefinitions.ActivationTime.Reaction
                                    && (unit.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction, ActionDefinitions.ActionScope.Battle, false) != ActionDefinitions.ActionStatus.Available
                                       || unit.GetActionStatus(ActionDefinitions.Id.PowerReaction, ActionDefinitions.ActionScope.Battle, ActionDefinitions.ActionStatus.Available) != ActionDefinitions.ActionStatus.Available)
                                    )
                                {
                                    continue;
                                }
                                CharacterActionParams reactionParams = new CharacterActionParams(unit, ActionDefinitions.Id.PowerReaction);
                                reactionParams.TargetCharacters.Add(attacker);
                                reactionParams.ActionModifiers.Add(new ActionModifier());
                                IRulesetImplementationService service1 = ServiceRepository.GetService<IRulesetImplementationService>();
                                reactionParams.RulesetEffect = (RulesetEffect)service1.InstantiateEffectPower(unit.RulesetCharacter, p, false);
                                reactionParams.StringParameter = p.PowerDefinition.Name;
                                reactionParams.IsReactionEffect = true;
                                IGameLocationActionService service2 = ServiceRepository.GetService<IGameLocationActionService>();
                                int count = service2.PendingReactionRequestGroups.Count;
                                service2.ReactToUsePower(reactionParams, /*p.PowerDefinition.Name*/ "");
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
                                                                        RulesetEffect rulesetEffect,
                                                                        bool criticalHit,
                                                                        bool firstTarget)
                {
                    while (__result.MoveNext())
                    {
                        yield return __result.Current;
                    }
                    var extra_events = Process(__instance, attacker, defender, attackModifier, attackMode, rangedAttack, advantageType, actualEffectForms, rulesetEffect, criticalHit, firstTarget);

                    while (extra_events.MoveNext())
                    {
                        yield return extra_events.Current;
                    }

                    var features = Helpers.Accessors.extractFeaturesHierarchically<IInitiatorApplyEffectOnDamageDone>(attacker.RulesetCharacter);

                    foreach (var f in features)
                    {
                        f.processDamageInitiator(attacker, defender, attackModifier, attackMode, rangedAttack, attackMode == null);
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
                                                                        RulesetEffect rulesetEffect,
                                                                        bool criticalHit,
                                                                        bool firstTarget)
                {
                    if (__instance.battle == null)
                    {
                        yield break;
                    }

                    if (defender != null && defender.RulesetActor != null && (defender.RulesetActor is RulesetCharacterMonster || defender.RulesetActor is RulesetCharacterHero)
                        && attacker.RulesetCharacter is RulesetCharacterMonster)
                    {
                        attacker.RulesetCharacter.EnumerateFeaturesToBrowse<MonsterAdditionalDamage>(__instance.featuresToBrowseReaction);
                        foreach (FeatureDefinition featureDefinition in __instance.featuresToBrowseReaction)
                        {
                            MonsterAdditionalDamageProxy provider = (featureDefinition as MonsterAdditionalDamage).provider;
                            bool restrictions_ok = true;
                            foreach (var r in provider.restricitons)
                            {
                                if (r.isForbidden(attacker.RulesetCharacter))
                                {
                                    restrictions_ok = false;
                                    break;
                                }
                            }
                            if (!restrictions_ok)
                            {
                                continue;
                            }

                            if (provider.LimitedUsage != RuleDefinitions.FeatureLimitedUsage.None)
                            {
                                if (provider.LimitedUsage == RuleDefinitions.FeatureLimitedUsage.OnceInMyturn && (attacker.UsedSpecialFeatures.ContainsKey(featureDefinition.Name) || __instance.Battle != null && __instance.Battle.ActiveContender != attacker))
                                    continue;
                                else if (provider.LimitedUsage == RuleDefinitions.FeatureLimitedUsage.OncePerTurn && attacker.UsedSpecialFeatures.ContainsKey(featureDefinition.Name))
                                    continue;
                            }
                            CharacterActionParams reactionParams = (CharacterActionParams)null;
 
                            if (provider.TriggerCondition == RuleDefinitions.AdditionalDamageTriggerCondition.AdvantageOrNearbyAlly && attackMode != null)
                            {
                                if (!(advantageType == RuleDefinitions.AdvantageType.Advantage || advantageType != RuleDefinitions.AdvantageType.Disadvantage && __instance.IsConsciousCharacterOfSideNextToCharacter(defender, attacker.Side, attacker)))
                                    continue;
                            }
                            else if (provider.TriggerCondition == RuleDefinitions.AdditionalDamageTriggerCondition.SpendSpellSlot && attackModifier != null)
                            {
                                RulesetSpellRepertoire selectedSpellRepertoire = null;
                                foreach (RulesetSpellRepertoire spellRepertoire in attacker.RulesetCharacter.SpellRepertoires)
                                {
                                    if ((BaseDefinition)spellRepertoire.SpellCastingFeature == provider.spellcastingFeature)
                                    {
                                        bool flag3 = false;
                                        for (int spellLevel = 1; spellLevel <= spellRepertoire.MaxSpellLevelOfSpellCastingLevel; ++spellLevel)
                                        {
                                            int remaining = 0;
                                            int max = 0;
                                            spellRepertoire.GetSlotsNumber(spellLevel, out remaining, out max);
                                            if (remaining > 0)
                                            {
                                                selectedSpellRepertoire = spellRepertoire;
                                                flag3 = true;
                                                break;
                                            }
                                        }
                                        if (flag3)
                                        {
                                            reactionParams = new CharacterActionParams(attacker, ActionDefinitions.Id.SpendSpellSlot);
                                            reactionParams.IntParameter = 1;
                                            reactionParams.StringParameter = provider.NotificationTag;
                                            reactionParams.SpellRepertoire = selectedSpellRepertoire;
                                            IGameLocationActionService service = ServiceRepository.GetService<IGameLocationActionService>();
                                            int count = service.PendingReactionRequestGroups.Count;
                                            service.ReactToSpendSpellSlot(reactionParams);
                                            yield return __instance.WaitForReactions(attacker, service, count);
                                            if (!reactionParams.ReactionValidated)
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }
                                if (selectedSpellRepertoire == null)
                                {
                                    continue;
                                }
                                selectedSpellRepertoire = (RulesetSpellRepertoire)null;
                            }
                            else if (provider.TriggerCondition == RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasConditionCreatedByMe)
                            {
                            if (!defender.RulesetActor.HasConditionOfTypeAndSource(provider.RequiredTargetCondition, attacker.Guid))
                                continue;
                            }
                            else if (provider.TriggerCondition == RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasCondition)
                            {
                                if (!defender.RulesetActor.HasConditionOfType(provider.RequiredTargetCondition.Name))
                                    continue;
                            }
                            else if (provider.TriggerCondition == RuleDefinitions.AdditionalDamageTriggerCondition.TargetDoesNotHaveCondition)
                            {
                                if (defender.RulesetActor.HasConditionOfType(provider.RequiredTargetCondition.Name))
                                    continue;
                            }
                            else if (provider.TriggerCondition == RuleDefinitions.AdditionalDamageTriggerCondition.TargetIsWounded)
                            {
                                if (defender.RulesetCharacter != null && defender.RulesetCharacter.CurrentHitPoints >= defender.RulesetCharacter.GetAttribute("HitPoints").CurrentValue)
                                    continue;
                            }
                            __instance.ComputeAndNotifyAdditionalDamage(attacker, defender, provider, actualEffectForms, reactionParams);
                            if (!attacker.UsedSpecialFeatures.ContainsKey(featureDefinition.Name))
                            {
                                attacker.UsedSpecialFeatures[featureDefinition.Name] = 0;
                            }
                            attacker.UsedSpecialFeatures[featureDefinition.Name]++;
                            provider = null;
                            reactionParams = null;
                        }
                    }


                    if (attackMode != null 
                        && attackMode.Ranged
                        && defender.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction, ActionDefinitions.ActionScope.Battle, false) == ActionDefinitions.ActionStatus.Available
                        && defender.GetActionStatus(ActionDefinitions.Id.PowerReaction, ActionDefinitions.ActionScope.Battle, ActionDefinitions.ActionStatus.Available) == ActionDefinitions.ActionStatus.Available
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
                            && unit.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction, ActionDefinitions.ActionScope.Battle, false) == ActionDefinitions.ActionStatus.Available
                            && unit.GetActionStatus(ActionDefinitions.Id.PowerReaction, ActionDefinitions.ActionScope.Battle, ActionDefinitions.ActionStatus.Available) == ActionDefinitions.ActionStatus.Available)
                        {
                            var powers = unit.RulesetCharacter.UsablePowers.Where(u => u.PowerDefinition is NewFeatureDefinitions.IReactionPowerOnDamage
                                                                                  && unit.RulesetCharacter.GetRemainingUsesOfPower(u) > 0
                                                                                  && (u.PowerDefinition as NewFeatureDefinitions.IReactionPowerOnDamage)
                                                                                    .canBeUsedOnDamage(unit, attacker, defender, attackMode, false)
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
                                reactionParams.RulesetEffect = (RulesetEffect)service1.InstantiateEffectPower(unit.RulesetCharacter, p, false);
                                reactionParams.StringParameter = p.PowerDefinition.Name;
                                reactionParams.IsReactionEffect = true;
                                IGameLocationActionService service2 = ServiceRepository.GetService<IGameLocationActionService>();
                                int count = service2.PendingReactionRequestGroups.Count;
                                service2.ReactToUsePower(reactionParams, /*p.PowerDefinition.Name*/ "");
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


                    if (action.SaveOutcome != RuleDefinitions.RollOutcome.CriticalFailure || action.SaveOutcome != RuleDefinitions.RollOutcome.Failure)
                    {
                        yield return null;
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
                    var units = __instance.Battle?.AllContenders;
                    if (units == null)
                    {
                        units = new List<GameLocationCharacter>() {caster, defender};
                    }

                    var save_data = NewFeatureDefinitions.SavingthrowRollsData.getPrerolledData(defender);
                    
                    
                    foreach (GameLocationCharacter unit in units)
                    {
                        if (save_data.outcome != RuleDefinitions.RollOutcome.Failure)
                        {
                            break;
                        }
                        if (!unit.RulesetCharacter.IsDeadOrDyingOrUnconscious)
                        {
                            var powers = unit.RulesetCharacter.UsablePowers.Where(u => u.PowerDefinition is NewFeatureDefinitions.IModifyFailedSavePower
                                                                                  && unit.RulesetCharacter.GetRemainingUsesOfPower(u) > 0
                                                                                  && (u.PowerDefinition as NewFeatureDefinitions.IModifyFailedSavePower)
                                                                                    .canBeUsedOnFailedSave(unit, caster, defender, saveModifier, rulesetEffect)
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
                                if (p.powerDefinition.activationTime == RuleDefinitions.ActivationTime.Reaction &&
                                    (unit.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction, ActionDefinitions.ActionScope.Battle, false) != ActionDefinitions.ActionStatus.Available
                                    || unit.GetActionStatus(ActionDefinitions.Id.PowerReaction, ActionDefinitions.ActionScope.Battle, ActionDefinitions.ActionStatus.Available) != ActionDefinitions.ActionStatus.Available)
                                    )
                                {
                                    continue;
                                }
                                CharacterActionParams reactionParams = new CharacterActionParams(unit, (ActionDefinitions.Id)ExtendedActionId.ConsumePowerUse);
                                reactionParams.ActionModifiers.Add(saveModifier);
                                reactionParams.targetCharacters.Add(defender);
                                reactionParams.UsablePower = p;
                                reactionParams.isReactionEffect = p.powerDefinition.ActivationTime == RuleDefinitions.ActivationTime.Reaction;
                                reactionParams.RulesetEffect = rulesetEffect;
                                GameLocationActionManager service = ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
                                if (service == null)
                                {
                                    yield return null;
                                }
                                int count = service.PendingReactionRequestGroups.Count;
                                service.AddInterruptRequest(new ReactionRequestConsumePowerUse(reactionParams));
                                yield return __instance.WaitForReactions(unit, service, count);
                                if (reactionParams.ReactionValidated)
                                {
                                    int old_value = save_data.total_roll_value;
                                    save_data.total_roll_value += (p.powerDefinition as IModifyFailedSavePower).getSavingThrowBonus(unit, caster, defender, saveModifier, rulesetEffect);
                                    save_data.outcome = save_data.total_roll_value < save_data.dc_value ? RuleDefinitions.RollOutcome.Failure : RuleDefinitions.RollOutcome.Success;                                  
                                    action.SaveOutcome = save_data.outcome;
                                    //Main.Logger.Log("New Roll Value: " + save_data.total_roll_value.ToString());
                                    
                                    var game_console = ServiceRepository.GetService<IGameService>()?.Game?.GameConsole;
                                    if (game_console != null)
                                    {
                                        GameConsoleEntry entry = new GameConsoleEntry("Feedback/&RollResultModifiedTitle", game_console.consoleTableDefinition);
                                        game_console.AddCharacterEntry(defender.RulesetActor, entry);
                                        entry.AddParameter(ConsoleStyleDuplet.ParameterType.AbilityInfo, old_value.ToString());
                                        entry.AddParameter(ConsoleStyleDuplet.ParameterType.AbilityInfo, save_data.total_roll_value.ToString());
                                        game_console.AddEntry(entry);
                                    }
                                }
                            }
                        }
                    }
                    NewFeatureDefinitions.SavingthrowRollsData.removePrerolledData(defender);

                   var power = defender.RulesetCharacter?.UsablePowers.Where(u => u.PowerDefinition is NewFeatureDefinitions.RerollFailedSavePower
                                                      && defender.RulesetCharacter.GetRemainingUsesOfPower(u) > 0
                                                     ).FirstOrDefault();
                    if (power != null)
                    {
                        var reactionParams = new CharacterActionParams(defender, (ActionDefinitions.Id)ExtendedActionId.ConsumePowerUse);
                        reactionParams.RulesetEffect = rulesetEffect;
                        reactionParams.UsablePower = power;
                        reactionParams.targetCharacters.Add(defender);
                        GameLocationActionManager service = ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
                        if (service == null)
                        {
                            yield return null;
                        }
                        int count = service.PendingReactionRequestGroups.Count;
                        service.AddInterruptRequest(new ReactionRequestConsumePowerUse(reactionParams));
                        yield return __instance.WaitForReactions(defender, service, count);
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
