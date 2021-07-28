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
        public static ConditionDefinition wildshaped_unit_condition = DatabaseHelper.ConditionDefinitions.ConditionConjuredCreature;

        static public void initialize()
        {
            fillRitualSpellcastingMap();
            createPolymorphMergeCondition();
        }

        static void createPolymorphMergeCondition()
        {
            polymorph_merge_condition = Helpers.ConditionBuilder.createCondition("PolymorphRemoveFromGameCondition",
                                                                                        "9898e483-2ab3-4044-afa3-b4d463724192",
                                                                                        "Rules/&PolymorphMergeConditionTitle",
                                                                                        Common.common_no_title,
                                                                                        null,
                                                                                        DatabaseHelper.ConditionDefinitions.ConditionMagicallyArmored
                                                                                        );
            polymorph_merge_condition.removedFromTheGame = true;
            polymorph_merge_condition.conditionTags.Clear();
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
