using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RulesetEffectSpellPatcher
    {
        [HarmonyPatch(typeof(RulesetEffectSpell), "EffectDescription", MethodType.Getter)]
        class RulesetEffectSpell_EffectDescription
        {
            static void Postfix(ref EffectDescription __result, RulesetEffectSpell __instance)
            {
                var base_definition = __instance.spellDefinition as NewFeatureDefinitions.ICustomMagicEffectBasedOnCaster;
                if (base_definition != null && __instance.Caster != null)
                {
                    __result = base_definition.getCustomEffect(__instance);
                }
            }
        }
    }
}
