using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA;
using UnityEngine;

namespace SolastaModHelpers.Patches
{
    class GameLocationTargetingManagerPatcher
    {
        /*[HarmonyPatch(typeof(GameLocationTargetingManager), "ComputeTargetsInBox")]
        class GameLocationTargetingManager_ComputeTargetsInBox
        {
            internal static void Postfix(BoxInt boxTargetArea,
                                        bool considerShape,
                                        RuleDefinitions.Side observingSide,
                                        EffectDescription effectDescription,
                                        List<GameLocationCharacter> affectedCharacters,
                                        Vector3 origin,
                                        Vector3 direction,
                                        GameLocationCharacter sourceCharacter,
                                        ICollection<int3> coveredPositions,
                                        ICollection<int3> coveredFloorPositions,
                                        ICollection<int3> affectedPositions,
                                        ICollection<int3> affectedFloorPositions)
            {
                Main.Logger.Log("Running Effect: " + effectDescription.TargetType.ToString());
                Main.Logger.Log("Running Exclude: " + effectDescription.TargetExcludeCaster.ToString());
                Main.Logger.Log("Running Targeting: " + sourceCharacter?.RulesetCharacter?.Name);
                foreach (var c in affectedCharacters)
                {
                    Main.Logger.Log("Affected: " + c.Name);
                }
            }
        }*/
    }
}
