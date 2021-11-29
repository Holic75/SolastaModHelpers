using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class GameLocationActionManagerPatches
    {
        [HarmonyPatch(typeof(GameLocationActionManager), "ReactForOpportunityAttack")]
        internal static class GameLocationActionManager_ReactForOpportunityAttack
        {
            internal static bool Prefix(GameLocationActionManager __instance, CharacterActionParams reactionParams)
            {
                if (Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.Warcaster>(reactionParams.actingCharacter.RulesetCharacter).Any())
                {
                    __instance.AddInterruptRequest((ReactionRequest)new ReactionRequestWarcaster(reactionParams));
                    return false;
                }

                return true;
            }
        }
    }
}
