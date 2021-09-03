using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers
{
    public class Fixes
    {
        static internal void fixConjureAnimalDuration()
        {
            foreach (var s in DatabaseHelper.SpellDefinitions.ConjureAnimals.subspellsList)
            {
                s.effectDescription.durationType = RuleDefinitions.DurationType.Hour;
            }
        }


        static internal void fixVampiricTouch()
        {
            //fix vampiric touch to work indpendently of specificed attribute 
            var vampiric_touch_power = DatabaseHelper.FeatureDefinitionPowers.PowerVampiricTouchIntelligence;
            var vampiric_touch_power_custom = Helpers.GenericPowerBuilder<NewFeatureDefinitions.PowerWithContextFromCondition>
                                                            .createPower("VampiricTouchHitPower",
                                                                          "c960dbd5-0864-41a6-96f9-1745462ebaa6",
                                                                          vampiric_touch_power.GuiPresentation.title,
                                                                          vampiric_touch_power.GuiPresentation.description,
                                                                          vampiric_touch_power.GuiPresentation.spriteReference,
                                                                          vampiric_touch_power.effectDescription,
                                                                          vampiric_touch_power.activationTime,
                                                                          vampiric_touch_power.fixedUsesPerRecharge,
                                                                          vampiric_touch_power.UsesDetermination,
                                                                          vampiric_touch_power.rechargeRate,
                                                                          vampiric_touch_power.UsesAbilityScoreName,
                                                                          vampiric_touch_power.abilityScore,
                                                                          vampiric_touch_power.costPerUse,
                                                                          vampiric_touch_power.showCasting
                                                                          );
            vampiric_touch_power_custom.effectDescription.savingThrowDifficultyAbility = Helpers.Misc.createContextDeterminedAttribute(DatabaseHelper.ConditionDefinitions.ConditionVampiricTouchIntelligence);
            vampiric_touch_power_custom.minCustomEffectLevel = 4;
            for (int i = 4; i < 10; i++)
            {
                var effect = new EffectDescription();
                effect.Copy(vampiric_touch_power_custom.effectDescription);
                effect.effectForms.Clear();
                var damage = new EffectForm();
                damage.formType = EffectForm.EffectFormType.Damage;
                damage.createdByCharacter = true;
                damage.damageForm = new DamageForm();
                damage.damageForm.damageType = Helpers.DamageTypes.Necrotic;
                damage.damageForm.diceNumber = i;
                damage.damageForm.dieType = RuleDefinitions.DieType.D6;
                damage.damageForm.healFromInflictedDamage = RuleDefinitions.HealFromInflictedDamage.Half;
                effect.effectForms.Add(damage);

                vampiric_touch_power_custom.levelEffectList.Add((i, effect));
            }

            vampiric_touch_power_custom.attackHitComputation = vampiric_touch_power.AttackHitComputation;
            vampiric_touch_power_custom.abilityScoreBonusToAttack = vampiric_touch_power.AbilityScoreBonusToAttack;
            vampiric_touch_power_custom.proficiencyBonusToAttack = vampiric_touch_power.proficiencyBonusToAttack;
            vampiric_touch_power_custom.condition = DatabaseHelper.ConditionDefinitions.ConditionVampiricTouchIntelligence;
            DatabaseHelper.ActionDefinitions.VampiricTouchIntelligence.activatedPower = vampiric_touch_power_custom;
        }
    }
}
