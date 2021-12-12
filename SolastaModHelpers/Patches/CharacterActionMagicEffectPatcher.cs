using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolastaModHelpers.Patches
{
    class CharacterActionMagicEffectPatcher
    {
        //enable to perform automatic attacks after spell cast (like for sunlight blade cantrip) and chain effects
        [HarmonyPatch(typeof(CharacterActionMagicEffect), "ExecuteImpl")]
        internal static class CharacterActionMagicEffect_ExecuteImpl_Patch
        {
            internal static void Prefix(CharacterActionMagicEffect __instance)
            {
                if ((__instance.GetBaseDefinition() as NewFeatureDefinitions.IPerformAttackAfterMagicEffectUse) != null)
                {
                    __instance.ActionParams.skipAnimationsAndVFX = true;
                }
            }


            internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result, CharacterActionMagicEffect __instance)
            {                
                while (__result.MoveNext())
                {
                    yield return __result.Current;
                }

                var attack_params = (__instance.GetBaseDefinition() as NewFeatureDefinitions.IPerformAttackAfterMagicEffectUse)?.performAttackAfterUse(__instance);
                if (attack_params != null)
                {
                    var attack_action = new CharacterActionAttack(attack_params);
                    var enums = attack_action.Execute();
                    while (enums.MoveNext())
                    {
                        yield return enums.Current;
                    }
                }

                var chain_action = (__instance.GetBaseDefinition() as NewFeatureDefinitions.IChainMagicEffect)?.getNextMagicEffect(__instance);
                if (chain_action != null)
                {
                    var enums = chain_action.Execute();
                    while (enums.MoveNext())
                    {
                        yield return enums.Current;
                    }
                }
            }
        }

        //allow to refill formParams at a later stage when using spell or power in case actual effect description was modified (as with caster dependent effects)
        [HarmonyPatch(typeof(CharacterActionCastSpell))]
        [HarmonyPatch("ApplyMagicEffect", MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(GameLocationCharacter), typeof(ActionModifier), typeof(int), typeof(int), typeof(RuleDefinitions.RollOutcome), typeof(bool), typeof(RuleDefinitions.RollOutcome), typeof(int) },
                     new ArgumentType[] {ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]
        internal static class CharacterActionMagicEffect_ApplyMagicEffect_Patch
        {
            internal static bool Prefix(GameLocationCharacter target,
                                        ActionModifier actionModifier,
                                        int targetIndex,
                                        int targetCount,
                                        RuleDefinitions.RollOutcome outcome,
                                        bool rolledSaveThrow,
                                        RuleDefinitions.RollOutcome saveOutcome,
                                        ref int damageReceived,
                                        CharacterActionCastSpell __instance)
            {
                ApplyForms(__instance, __instance.ActingCharacter, __instance.activeSpell, __instance.AddDice, __instance.AddHP,
                           __instance.AddTempHP, __instance.activeSpell.SlotLevel, target, actionModifier, outcome, rolledSaveThrow,
                           saveOutcome, targetIndex, targetCount, __instance.TargetItem, RuleDefinitions.EffectSourceType.Spell, ref damageReceived);
                return false;
            }


            static void ApplyForms(CharacterActionMagicEffect magic_effect,
                                   GameLocationCharacter caster,
                                   RulesetEffect activeEffect,
                                   int addDice,
                                   int addHP,
                                   int addTempHP,
                                   int effectLevel,
                                   GameLocationCharacter target,
                                   ActionModifier actionModifier,
                                   RuleDefinitions.RollOutcome attack_outcome,
                                   bool rolledSaveThrow,
                                   RuleDefinitions.RollOutcome saveOutcome,
                                   int targetIndex,
                                   int totalTargetsNumber,
                                   RulesetItem targetITem,
                                   RuleDefinitions.EffectSourceType sourceType,
                                   ref int damageReceived)
            {
                var criticalSuccess = attack_outcome == RuleDefinitions.RollOutcome.CriticalSuccess;
                IRulesetImplementationService service = ServiceRepository.GetService<IRulesetImplementationService>();
                RulesetImplementationDefinitions.ApplyFormsParams formsParams = new RulesetImplementationDefinitions.ApplyFormsParams();
                formsParams.FillSourceAndTarget(caster.RulesetCharacter, target.RulesetActor);
                formsParams.FillFromActiveEffect(activeEffect);
                formsParams.FillSpecialParameters(rolledSaveThrow, addDice, addHP, addTempHP, effectLevel, actionModifier, saveOutcome, criticalSuccess, targetIndex, totalTargetsNumber, magic_effect.TargetItem);
                formsParams.effectSourceType = sourceType;
                formsParams.targetSubstitute = magic_effect.ActionParams.TargetSubstitute;
                if (activeEffect.EffectDescription.RangeType == RuleDefinitions.RangeType.MeleeHit || activeEffect.EffectDescription.RangeType == RuleDefinitions.RangeType.RangeHit)
                    formsParams.attackOutcome = attack_outcome;
                damageReceived = service.ApplyEffectForms(magic_effect.actualEffectForms[targetIndex], formsParams);
            }
        }
    }
}
