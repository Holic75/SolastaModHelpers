using SolastaModApi;
using SolastaModApi.Infrastructure;
using System.Collections.Generic;
using static RuleDefinitions;

namespace SolastaModHelpers
{
    public class Common
    {
        public static Dictionary<RitualCasting, FeatureDefinitionMagicAffinity> ritual_spellcastings_map = new Dictionary<RitualCasting, FeatureDefinitionMagicAffinity>();
        public static string common_condition_prefix = "Rules/&CommonConditioUnderEffectOfPrefix";
        public static string common_no_title = "Feature/&NoContentTitle";

        static public void initialize()
        {
            fillRitualSpellcastingMap();
        }


        static void fillRitualSpellcastingMap()
        {
            ritual_spellcastings_map[RitualCasting.Prepared] = DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinityClericRitualCasting;
            ritual_spellcastings_map[RitualCasting.Spellbook] = DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinityWizardRitualCasting;

            var spontaneous_ritual_spellcsting = Helpers.CopyFeatureBuilder<FeatureDefinitionMagicAffinity>.createFeatureCopy("MagicAffinitySpontaneousRitualCasting",
                                                                                                                              "efd3d247-d74f-47ac-b575-159fcad3608f",
                                                                                                                              "",
                                                                                                                              "",
                                                                                                                              null,
                                                                                                                              DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinityClericRitualCasting
                                                                                                                              );
            
            spontaneous_ritual_spellcsting.SetField("ritualCasting", (RitualCasting)ExtendedEnums.ExtraRitualCasting.Spontaneous);
            ritual_spellcastings_map[(RitualCasting)ExtendedEnums.ExtraRitualCasting.Spontaneous] = spontaneous_ritual_spellcsting;
        }
    }
}
