using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class SpellRepertoirePatches
    {
        class SpellsByLevelGroupBindLearningPatcher
        {
            [HarmonyPatch(typeof(SpellsByLevelGroup), "BindLearning")]
            internal static class SpellsByLevelGroup_BindLearning_Patch
            {
                internal static bool Prefix(ICharacterBuildingService characterBuildingService,
                                            ref SpellListDefinition spellListDefinition,
                                            List<string> restrictedSchools,
                                            int spellLevel,
                                            SpellBox.SpellBoxChangedHandler spellBoxChanged,
                                            ref List<SpellDefinition> knownSpells,
                                            List<SpellDefinition> unlearnedSpells,
                                            string spellTag,
                                            bool canAcquireSpells,
                                            bool unlearn)
                {
                    if (unlearn)
                    {
                        var spell_set = spellListDefinition.SpellsByLevel.Aggregate(new HashSet<SpellDefinition>(), (old, next) => 
                        {
                           foreach (var ss in next.spells)
                           {
                                old.Add(ss);
                           }
                           return old;
                        }
                        );
                        knownSpells = knownSpells.Where(s => spell_set.Contains(s)).ToList();
                        return true;
                    }

                    var hero = characterBuildingService.HeroCharacter;
                    if (hero == null)
                    {
                        return true;
                    }

                    var extra_spell_list = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IReplaceSpellList>(hero)
                                                                    .Select(rs => rs.getSpelllist(characterBuildingService)).FirstOrDefault(s => s != null);

                    if (extra_spell_list == null)
                    {
                        return true;
                    }

                    spellListDefinition = extra_spell_list;
                    return true;
                }
            }
        }
    }
}
