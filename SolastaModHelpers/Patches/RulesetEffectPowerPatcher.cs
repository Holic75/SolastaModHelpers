using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    //add support for ICustomPowerEffectBasedOnCaster allowing to pick power effect depending on some caster properties
    class RulesetEffectPowerPatcher
    {
        [HarmonyPatch(typeof(RulesetEffectPower), "EffectDescription", MethodType.Getter)]
        class RulesetEffectPower_EffectDescription
        {
            static void Postfix(RulesetEffectPower __instance, ref EffectDescription __result)
            {
                var base_definition = __instance.PowerDefinition as NewFeatureDefinitions.ICustomPowerEffectBasedOnCaster;
                if (base_definition != null && __instance.User != null)
                {
                    __result = base_definition.getCustomEffect(__instance);
                }

                var power_definition = __instance.usablePower?.powerDefinition;
                if (power_definition == null 
                    || !(power_definition.activationTime == RuleDefinitions.ActivationTime.OnAttackHit
                         || power_definition.activationTime == RuleDefinitions.ActivationTime.OnAttackHitWithBow)
                    || !Patches.GameLocationBattleManagerPatcher.GameLocationBattleManagerHandleCharacterAttackDamagePatcher.GameLocationBattleManager_HandleCharacterAttackDamage_Patch.scored_critical)
                {
                    return;
                }
                __result = doubleCriticalHitDamage(__result);
            }
        }


        static public EffectDescription doubleCriticalHitDamage(EffectDescription current_effect)
        {
            var eff = new EffectDescription();
            eff.Copy(current_effect);
            eff.effectForms.Clear();
            foreach (var f in current_effect.effectForms)
            {
                if (f.FormType == EffectForm.EffectFormType.Damage)
                {
                    var new_f = new EffectForm();
                    new_f.Copy(f);
                    new_f.damageForm.diceNumber *= 2;
                    new_f.damageForm.bonusDamage *= 2;
                    eff.effectForms.Add(new_f);
                }
                else
                {
                    eff.effectForms.Add(f);
                }
            }
            return eff;
        }




    }
}
