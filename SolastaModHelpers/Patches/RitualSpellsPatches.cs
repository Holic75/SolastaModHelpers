using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class SpontaneousRitualSpellsPatcher
    {
        //Support for spontaneous caster ritual casting (for bard for example)
        [HarmonyPatch(typeof(RulesetCharacterHero), "EnumerateUsableRitualSpells")]
        internal static class RestModuleHitDice_EnumerateUsableRitualSpells_Patch
        {
            internal static void Postfix(RulesetCharacterHero __instance, RuleDefinitions.RitualCasting ritualType, List<SpellDefinition> ritualSpells)
            {
                var extended_ritual_type = (ExtendedEnums.ExtraRitualCasting)ritualType;
                switch (extended_ritual_type)
                {
                    case ExtendedEnums.ExtraRitualCasting.Spontaneous:
                        RulesetSpellRepertoire rulesetSpellRepertoire1 = (RulesetSpellRepertoire)null;
                        foreach (RulesetSpellRepertoire spellRepertoire in __instance.SpellRepertoires)
                        {
                            if (spellRepertoire.SpellCastingFeature.SpellReadyness == RuleDefinitions.SpellReadyness.AllKnown && spellRepertoire.SpellCastingFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.Selection)
                            {
                                rulesetSpellRepertoire1 = spellRepertoire;
                                break;
                            }
                        }
                        if (rulesetSpellRepertoire1 == null)
                            break;
                        using (List<SpellDefinition>.Enumerator enumerator = rulesetSpellRepertoire1.KnownSpells.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                SpellDefinition current = enumerator.Current;
                                if (current.Ritual && rulesetSpellRepertoire1.MaxSpellLevelOfSpellCastingLevel >= current.SpellLevel)
                                    ritualSpells.Add(current);
                            }
                            break;
                        }
                }
            }
        }
    }

}
