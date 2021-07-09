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
        [HarmonyPatch(typeof(RulesetImplementationManager), "ApplyDamageForm")]
        internal class RulesetimplementationManager_ApplyDamageForm
        {
            internal static int dice_num_bonus = 0;
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var dice_number_store_load = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("DiceNumber")) + 10;
                if (codes[dice_number_store_load].opcode != System.Reflection.Emit.OpCodes.Stloc_S)
                {
                    throw new Exception("failed to patch RulesetimplementationManager_ApplyDamageForm");
                }

                codes.InsertRange(dice_number_store_load,
                              new HarmonyLib.CodeInstruction[]
                              {
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_2), //formParams
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<int, RulesetImplementationDefinitions.ApplyFormsParams, int>(processDiceNumber).Method
                                                                 )
                              }
                            );
                return codes.AsEnumerable();
            }

            static int processDiceNumber(int base_dice_num, RulesetImplementationDefinitions.ApplyFormsParams form_params)
            {
                var dice_num = base_dice_num;
                var character = (form_params.sourceCharacter as RulesetCharacter);
                if (character == null)
                {
                    return dice_num;
                }
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IDamageDiceIncrease>(character);
                foreach (var f in features)
                {
                    dice_num += f.extraDice(form_params);
                }
                dice_num_bonus = dice_num - base_dice_num;
                return dice_num;
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
                    f.processConditionApplication(actor, condition);
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
                                    List<EffectForm> effectForms,
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
        }
    }

}

