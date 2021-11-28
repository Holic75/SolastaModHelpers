using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class GuiFeatDefinitionPatcher
    {
        [HarmonyPatch(typeof(GuiFeatDefinition), "IsFeatMacthingPrerequisites")]
        internal static class GuiFeatDefinition_IsFeatMacthingPrerequisites
        {
            internal static void Postfix(GuiFeatDefinition __instance, 
                                        FeatDefinition feat,
                                        RulesetCharacterHero hero,
                                        ref string prerequisiteOutput,
                                        ref bool __result)
            {
                var prerequsites = NewFeatureDefinitions.FeatureData.getFeaturePrerequisites(feat);

                foreach (var p in prerequsites)
                {
                    var description = p.getDescription();
                    if (p.isForbidden(hero))
                    {
                        prerequisiteOutput += "\n" + Gui.Colorize(description, "EA7171");
                        __result = false;
                    }
                    else
                    {
                        prerequisiteOutput += "\n" + description;
                    }
                }
            }
        }
    }
}
