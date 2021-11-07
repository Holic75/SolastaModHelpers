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
            }
        }

    }
}
