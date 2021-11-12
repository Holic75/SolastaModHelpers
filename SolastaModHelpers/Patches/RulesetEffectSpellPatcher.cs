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
        //add support for ICustomMagicEffectBasedOnCaster allowing to pick spell effect depending on some caster properties
        //and IModifySpellEffect which modifies existing effect (changing elemental damage type for example)
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

                var caster = __instance.Caster;
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IModifySpellEffect>(caster);
                foreach (var f in features)
                {
                    __result = f.modifyEffect(__instance, __result);
                }
                
            }
        }

        //fix to make ICustomMagicEffectBasedOnCaster work properly on racial spells
        [HarmonyPatch(typeof(RulesetEffectSpell), "GetClassLevel")]
        class RulesetEffectSpell_GetClassLevel
        {
            static void Postfix(RulesetEffectSpell __instance, RulesetCharacter character, ref int __result)
            {
                if (__result == 0 
                    && (__instance?.spellRepertoire?.spellCastingMonster != null || __instance?.spellRepertoire?.spellCastingRace != null))
                {
                    __result = character.GetSpellcastingLevel(__instance.spellRepertoire);
                }

            }
        }
    }
}
