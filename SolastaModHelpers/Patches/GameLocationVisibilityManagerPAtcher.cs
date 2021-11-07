using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolastaModHelpers.Patches
{
    class GameLocationVisibilityManagerPatcher
    {
        //support for IgnoreDynamicVisionImpairement feature, allowing to perceive enemies while in darkness
        [HarmonyPatch(typeof(GameLocationVisibilityManager), "IsPositionPerceivedByCharacter")]
        class GameLocationVisibilityManager_IsPositionPerceivedByCharacter
        {
            internal static void Postfix(GameLocationVisibilityManager __instance,
                                        Vector3 position,
                                        Vector3 origin,
                                        RulesetCharacter rulesetCharacter,
                                        List<SenseMode.Type> optionalRequiredSense,
                                        ref bool __result)
            {
                GridAccessor gridAccessor = GridAccessor.Default;
                bool dynamic_impairement = (uint)(gridAccessor.RuntimeFlags(__instance.gameLocationPositioningService.GetGridPositionFromWorldPosition(origin)) & CellFlags.Runtime.DynamicSightImpaired) > 0U;
                if (__result 
                    || rulesetCharacter.ImpairedSight 
                    || __instance.gameLocationPositioningService.RaycastGridSightBlocker(origin, position, __instance.GameLocationService) 
                    || !dynamic_impairement)
                {
                    return;
                }

                float magnitude = (__instance.gameLocationPositioningService.GetGridPositionFromWorldPosition(origin) - __instance.gameLocationPositioningService.GetGridPositionFromWorldPosition(position)).magnitude;

                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IgnoreDynamicVisionImpairement>(rulesetCharacter);

                __result = features.Any(f => f.canIgnoreDynamicVisionImpairement(rulesetCharacter, magnitude));
            }
        }
    }
}
