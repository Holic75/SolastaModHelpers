using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using static RuleDefinitions;

namespace SolastaModHelpers
{
    public class Common
    {
        public static Dictionary<RuleDefinitions.RitualCasting, FeatureDefinitionMagicAffinity> ritual_spellcastings_map = new Dictionary<RitualCasting, FeatureDefinitionMagicAffinity>();
        public static string common_condition_prefix = "Rules/&CommonConditioUnderEffectOfPrefix";
        public static string common_no_title = "Feature/&NoContentTitle";
        public static AssetReferenceSprite common_no_icon = DatabaseHelper.FeatureDefinitionPointPools.PointPoolRangerSkillPoints.GuiPresentation.SpriteReference;

        public static ConditionDefinition polymorph_merge_condition;
        public static ConditionDefinition wildshaped_unit_condition;

        public static FeatureDefinitionPower cancel_polymorph_power;
        public static NewFeatureDefinitions.CancelPolymorphFeature cancel_polymorph_feature;
        public static NewFeatureDefinitions.SpellcastingForbidden polymorph_spellcasting_forbidden;

        public static FeatureDefinitionPower switch_attack;
        public static NewFeatureDefinitions.AllowToUseWeaponCategoryAsSpellFocus staff_focus;

        public static List<Action<RulesetCharacterHero>> postload_actions = new List<Action<RulesetCharacterHero>>();

        static public void initialize()
        {
            fillRitualSpellcastingMap();
            createPolymorphFeatures();
            createStaffFocus();
        }

        static void createStaffFocus()
        {
            staff_focus = Helpers.FeatureBuilder<NewFeatureDefinitions.AllowToUseWeaponCategoryAsSpellFocus>.createFeature("UseStaffAsSpellcastingFocus",
                                                                                                                   "67785d07-4320-419e-8c70-634a67c74e4c",
                                                                                                                   Common.common_no_title,
                                                                                                                   Common.common_no_title,
                                                                                                                   Common.common_no_icon,
                                                                                                                   a =>
                                                                                                                   {
                                                                                                                       a.weaponTypes = new List<string> { Helpers.WeaponProficiencies.QuarterStaff };
                                                                                                                   }
                                                                                                                   );
            staff_focus.guiPresentation.hidden = true;
            DatabaseHelper.CharacterClassDefinitions.Wizard.FeatureUnlocks.Insert(0, new FeatureUnlockByLevel(staff_focus, 1));
            DatabaseHelper.CharacterClassDefinitions.Sorcerer.FeatureUnlocks.Insert(0, new FeatureUnlockByLevel(staff_focus, 1));

            Action<RulesetCharacterHero> fix_action = c =>
            {
                if (c.activeFeatures.Any(cc => cc.Value.Contains(staff_focus)))
                {
                    return;
                }

                if (c.classesAndLevels.ContainsKey(DatabaseHelper.CharacterClassDefinitions.Wizard))
                {
                    c.activeFeatures[AttributeDefinitions.GetClassTag(DatabaseHelper.CharacterClassDefinitions.Wizard, 1)].Add(staff_focus);
                }

                if (c.classesAndLevels.ContainsKey(DatabaseHelper.CharacterClassDefinitions.Sorcerer))
                {
                    c.activeFeatures[AttributeDefinitions.GetClassTag(DatabaseHelper.CharacterClassDefinitions.Sorcerer, 1)].Add(staff_focus);
                }
            };

            Common.postload_actions.Add(fix_action);
        }


        static void createPolymorphFeatures()
        {
            var negate_fall_damage = Helpers.CopyFeatureBuilder<FeatureDefinitionMovementAffinity>.createFeatureCopy("PolymorphNegateFallDamage",
                                                                                                                 "a631d566-dc98-428d-8cea-801479020cb8",
                                                                                                                 Common.common_no_title,
                                                                                                                 Common.common_no_title,
                                                                                                                 null,
                                                                                                                 DatabaseHelper.FeatureDefinitionMovementAffinitys.MovementAffinityCatsGrace,
                                                                                                                 a =>
                                                                                                                 {
                                                                                                                     a.additionalFallThreshold = 10;
                                                                                                                 }
                                                                                                                 );

            polymorph_merge_condition = Helpers.ConditionBuilder.createCondition("PolymorphRemoveFromGameCondition",
                                                                                    "9898e483-2ab3-4044-afa3-b4d463724192",
                                                                                    Common.common_no_title,
                                                                                    Common.common_no_title,
                                                                                    null,
                                                                                    DatabaseHelper.ConditionDefinitions.ConditionDummy,
                                                                                    DatabaseHelper.FeatureDefinitionMoveModes.MoveModeFly12,
                                                                                    negate_fall_damage,
                                                                                    DatabaseHelper.FeatureDefinitionActionAffinitys.ActionAffinityConditionLethargic
                                                                                    );
            polymorph_merge_condition.removedFromTheGame = true;
            polymorph_merge_condition.conditionTags.Clear();
            
            wildshaped_unit_condition = Helpers.CopyFeatureBuilder<ConditionDefinition>.createFeatureCopy("PolymorphPolymorphedUnitMarkCondition",
                                                                                                          "a85f25e8-30ac-4c4c-ae64-32eac0f03381",
                                                                                                          "Rules/&WildshapedConditionTitle",
                                                                                                          "Rules/&WildshapedConditionDescription",
                                                                                                          DatabaseHelper.ConditionDefinitions.ConditionSpiderClimb.guiPresentation.SpriteReference,
                                                                                                          DatabaseHelper.ConditionDefinitions.ConditionConjuredCreature,
                                                                                                          a =>
                                                                                                          {
                                                                                                              a.parentCondition = DatabaseHelper.ConditionDefinitions.ConditionConjuredCreature;
                                                                                                          }
                                                                                                          );
                                                             



            var effect = new EffectDescription();
            effect.Copy(DatabaseHelper.SpellDefinitions.Banishment.effectDescription);
            effect.effectForms.Clear();
            effect.hasSavingThrow = false;
            effect.rangeParameter = 1;
            effect.targetSide = Side.Ally;
            effect.targetType = TargetType.Self;
            effect.rangeType = RangeType.Self;

            cancel_polymorph_power = Helpers.PowerBuilder.createPower("CancelPolymorphSelfPower",
                                                                      "11e21ac5-58d5-4968-ad83-5903cb09e43b",
                                                                      "Feature/&CancelPolymorphSelfPowerTitle",
                                                                      "Feature/&CancelPolymorphSelfPowerDescription",
                                                                      null,
                                                                      DatabaseHelper.FeatureDefinitionPowers.PowerTraditionShockArcanistArcaneFury,
                                                                      effect,
                                                                      ActivationTime.BonusAction,
                                                                      1,
                                                                      UsesDetermination.Fixed,
                                                                      RechargeRate.LongRest);

            cancel_polymorph_feature = Helpers.FeatureBuilder<NewFeatureDefinitions.CancelPolymorphFeature>.createFeature("CancelPolymorphWatcher",
                                                                                                                          "5c3616f8-f1c0-4649-8814-6e6d68c87d13",
                                                                                                                          Common.common_no_title,
                                                                                                                          Common.common_no_title,
                                                                                                                          Common.common_no_icon,
                                                                                                                          a =>
                                                                                                                          {
                                                                                                                              a.effectSource = cancel_polymorph_power;
                                                                                                                          }
                                                                                                                          );

            polymorph_spellcasting_forbidden = Helpers.FeatureBuilder<NewFeatureDefinitions.SpellcastingForbidden>.createFeature("PolymorphSpellcstingForbidden",
                                                                                                                                  "5bb54b18-efb5-4b38-9d52-7fd9e56ccfc2",
                                                                                                                                  Common.common_no_title,
                                                                                                                                  Common.common_no_title,
                                                                                                                                  Common.common_no_icon,
                                                                                                                                  a =>
                                                                                                                                  {
                                                                                                                                      a.forbidConcentration = false;
                                                                                                                                  }
                                                                                                                                  );
        }


        public static MonsterDefinition createPolymoprhUnit(MonsterDefinition base_unit, string name, string guid, string title, string description)
        {
            var unit = Helpers.CopyFeatureBuilder<MonsterDefinition>.createFeatureCopy(name,
                                                                                       guid,
                                                                                       title,
                                                                                       description,
                                                                                       null,
                                                                                       base_unit,
                                                                                       a =>
                                                                                       {
                                                                                           a.defaultFaction = DatabaseHelper.FactionDefinitions.Party.Name;
                                                                                           a.fullyControlledWhenAllied = true;
                                                                                           a.features = new List<FeatureDefinition>();
                                                                                           a.features.AddRange(base_unit.features);
                                                                                           a.features.Add(cancel_polymorph_power);
                                                                                           a.features.Add(cancel_polymorph_feature);
                                                                                           a.droppedLootDefinition = null;
                                                                                           a.bestiaryEntry = BestiaryDefinitions.BestiaryEntry.None;
                                                                                       });
            return unit;
        }


        static void fillRitualSpellcastingMap()
        {
            ritual_spellcastings_map[RuleDefinitions.RitualCasting.Prepared] = DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinityClericRitualCasting;
            ritual_spellcastings_map[RuleDefinitions.RitualCasting.Spellbook] = DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinityWizardRitualCasting;

            var spontaneous_ritual_spellcsting = Helpers.CopyFeatureBuilder<FeatureDefinitionMagicAffinity>.createFeatureCopy("MagicAffinitySpontaneousRitualCasting",
                                                                                                                              "efd3d247-d74f-47ac-b575-159fcad3608f",
                                                                                                                              "",
                                                                                                                              "",
                                                                                                                              null,
                                                                                                                              DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinityClericRitualCasting
                                                                                                                              );
            Helpers.Accessors.SetField(spontaneous_ritual_spellcsting, "ritualCasting", (RuleDefinitions.RitualCasting)ExtendedEnums.ExtraRitualCasting.Spontaneous);
            ritual_spellcastings_map[(RuleDefinitions.RitualCasting)ExtendedEnums.ExtraRitualCasting.Spontaneous] = spontaneous_ritual_spellcsting;
        }
    }
}
