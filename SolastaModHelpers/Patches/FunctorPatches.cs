using HarmonyLib;
using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class FunctorPatches
    {
        //select summoned units if all party is required (for example to teleport ot another lcoation)
        [HarmonyPatch(typeof(Functor), "SelectCharacters")]
        internal class Functor_SelectCharacters
        {
            static void Postfix(Functor __instance, FunctorParametersDescription functorParameters,
                                List<GameLocationCharacter> selectedCharacters,
                                List<GameLocationCharacter> exclusionList,
                                bool keepNullValues)
            {
                int index = 0;
                int max_index = functorParameters.playerPlacementMarkers.Length;
                if (functorParameters.characterLookUpMethod == FunctorDefinitions.CharacterLookUpMethod.AllPartyMembers)
                {
                    IGameLocationCharacterService service2 = ServiceRepository.GetService<IGameLocationCharacterService>();
                    foreach (var gc in service2.GuestCharacters)
                    {
                        var ruleset_character = gc.RulesetCharacter;
                        if (ruleset_character == null)
                        {
                            continue;
                        }
                        bool found = false;
                        foreach (var cc in ruleset_character.conditionsByCategory)
                        {
                            foreach (var c in cc.Value)
                            {
                                if ((c.conditionDefinition == DatabaseHelper.ConditionDefinitions.ConditionConjuredCreature)
                                    && service2.PartyCharacters.Any(p => p.RulesetCharacter.guid == c.sourceGuid))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (found)
                            {
                                selectedCharacters.Add(gc);
                                functorParameters.playerPlacementMarkers = functorParameters.playerPlacementMarkers.AddToArray(functorParameters.playerPlacementMarkers[index % max_index]);
                                index++;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
