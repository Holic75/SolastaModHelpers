using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA;

namespace SolastaModHelpers.Patches
{
    class RulesetImplementationManagerPatcher
    {
        [HarmonyPatch(typeof(RulesetImplementationManagerLocation), "ApplyItemPropertyForm")]
        class RulesetImplementationManager_ApplyItemPropertyForm
        {
            static bool Prefix(RulesetImplementationManagerLocation __instance,
                               EffectForm effectForm,
                               ref RulesetImplementationDefinitions.ApplyFormsParams formsParams
                              )
            {
                if (formsParams.activeEffect?.EffectDescription == null 
                    || formsParams.activeEffect?.EffectDescription.targetType == RuleDefinitions.TargetType.Item 
                    || formsParams.activeEffect?.EffectDescription.targetType == RuleDefinitions.TargetType.FreeSlot)
                {
                    return true;
                }

                if (formsParams.targetItem == null && formsParams.activeEffect?.EffectDescription.itemSelectionType == ActionDefinitions.ItemSelectionType.Weapon)
                {
                    var character = (formsParams.targetCharacter as RulesetCharacter) ?? formsParams.sourceCharacter;
                    formsParams.targetItem = character.CharacterInventory?.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0]?.equipedItem;
                }
                else if (formsParams.targetItem == null && formsParams.activeEffect?.EffectDescription.itemSelectionType == ActionDefinitions.ItemSelectionType.Equiped)
                {
                    var character = (formsParams.targetCharacter as RulesetCharacter) ?? formsParams.sourceCharacter;
                    formsParams.targetItem = character.CharacterInventory?.InventorySlotsByType[EquipmentDefinitions.SlotTypeTorso][0]?.equipedItem;
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(RulesetImplementationManagerLocation), "ApplyLightSourceForm")]
        class RulesetImplementationManager_ApplyLightSourceForm
        {
            static bool Prefix(RulesetImplementationManagerLocation __instance,
                               EffectForm effectForm,
                               ref RulesetImplementationDefinitions.ApplyFormsParams formsParams
                              )
            {
                if (formsParams.activeEffect?.EffectDescription == null)
                {
                    return true;
                }

                if (!formsParams.activeEffect.EffectDescription.effectForms.Any(f => f.formType == EffectForm.EffectFormType.ItemProperty))
                {
                    return true;
                }

                if (formsParams.targetItem == null && formsParams.activeEffect?.EffectDescription.itemSelectionType == ActionDefinitions.ItemSelectionType.Weapon)
                {
                    var character = (formsParams.targetCharacter as RulesetCharacter) ?? formsParams.sourceCharacter;
                    formsParams.targetItem = character.CharacterInventory?.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0]?.equipedItem;
                }
                else if (formsParams.targetItem == null && formsParams.activeEffect?.EffectDescription.itemSelectionType == ActionDefinitions.ItemSelectionType.Equiped)
                {
                    var character = (formsParams.targetCharacter as RulesetCharacter) ?? formsParams.sourceCharacter;
                    formsParams.targetItem = character.CharacterInventory?.InventorySlotsByType[EquipmentDefinitions.SlotTypeTorso][0]?.equipedItem;
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(RulesetImplementationManager), "ApplyConditionForm")]
        class RulesetImplementationManager_ApplyConditionForm
        {
            internal static void Postfix(RulesetImplementationManager __instance,
                                        EffectForm effectForm,
                                        RulesetImplementationDefinitions.ApplyFormsParams formsParams,
                                        bool retargeting,
                                        int sourceAmount)
            {
                var condition = effectForm.ConditionForm?.ConditionDefinition;
                if (condition == null || effectForm.ConditionForm.operation != ConditionForm.ConditionOperation.Add)
                {
                    return;
                }

                var actor = formsParams.targetCharacter as RulesetActor;
                if (actor == null)
                {
                    return;
                }

                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IApplyEffectOnConditionApplication>(actor);
                foreach (var f in features)
                {
                    f.processConditionApplication(actor, condition, formsParams);
                }

                var concentrated_spell = (actor as RulesetCharacter)?.concentratedSpell;
                if (concentrated_spell != null && condition.Features.OfType<NewFeatureDefinitions.IForbidSpellcasting>().Any(f => f.shouldBreakConcentration(actor)))
                {
                    (actor as RulesetCharacter)?.BreakConcentration();
                }
            }
        }


        [HarmonyPatch(typeof(RulesetImplementationManager), "ApplyEffectForms")]
        class RulesetimplementationManager_ApplyEffectForms
        {
            static bool Prefix(RulesetImplementationManager __instance,
                                    ref List<EffectForm> effectForms,
                                    RulesetImplementationDefinitions.ApplyFormsParams formsParams,
                                    bool retargeting,
                                    bool proxyOnly,
                                    bool forceSelfConditionOnly
                               )
            {

                var caster = formsParams.sourceCharacter;
                if (caster == null)
                {
                    return true;
                }

                /*var base_definition = formsParams.activeEffect?.SourceDefinition as NewFeatureDefinitions.ICustomEffectBasedOnCaster;
                if (base_definition != null)
                {
                    var effect = base_definition.getCustomEffect(formsParams);
                    effectForms = effect.effectForms;
                }*/
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.ICasterApplyEffectOnEffectApplication>(caster);
                foreach (var f in features)
                {
                    f.processCasterEffectApplication(caster, effectForms, formsParams);
                }

                var target = formsParams.targetCharacter as RulesetCharacter;
                if (target == null)
                {
                    return true;
                }

                var features2 = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.ITargetApplyEffectOnEffectApplication>(target);
                foreach (var f in features2)
                {
                    f.processTargetEffectApplication(target, effectForms, formsParams);
                }
                return true;
            }


            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var save_outcome_failure = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ldfld && x.operand.ToString().Contains("saveOutcome"));
                var save_outcome_critical_failure = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ldfld && x.operand.ToString().Contains("saveOutcome"));
                codes[save_outcome_failure + 1] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldc_I4_1);
                codes[save_outcome_critical_failure + 1] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldc_I4_0);
                return codes.AsEnumerable();
            }
        }


        [HarmonyPatch(typeof(RulesetImplementationManager), "TryRollSavingThrow")]
        internal class RulesetimplementationManager_TryRollSavingThrow
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var compute_target_savingthrow = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("ComputeSavingThrowModifier"));

                codes[compute_target_savingthrow] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Action<RulesetActor, string, EffectForm.EffectFormType, string, 
                                                                            string, string, ActionModifier, List<ISavingThrowAffinityProvider>,
                                                                            int, RulesetCharacter>(ComputeSavingThrowModifier).Method
                                                                 );
                codes.Insert(compute_target_savingthrow,
                             new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1) // caster
                            );
                return codes.AsEnumerable();
            }


            static void ComputeSavingThrowModifier(RulesetActor target,
                                                  string abilityType,
                                                  EffectForm.EffectFormType formType,
                                                  string schoolOfMagic,
                                                  string damageType,
                                                  string conditionType,
                                                  ActionModifier effectModifier,
                                                  List<ISavingThrowAffinityProvider> accountedProviders,
                                                  int savingThrowContextField,
                                                  RulesetCharacter caster
                                                 )
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<ISavingThrowAffinityProvider>(target);
                foreach (var  f in features)
                {
                    var caster_feature = f as NewFeatureDefinitions.ICasterDependentSavingthrowAffinityProvider;

                    if (caster_feature != null && !caster_feature.checkCaster(caster, target))
                    {
                        accountedProviders.Add(f);
                    }
                }
                target.ComputeSavingThrowModifier(abilityType, formType, schoolOfMagic, damageType, conditionType, effectModifier, accountedProviders, savingThrowContextField);
            }
        }
    }

}

