using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class CharacterStageSpellSelectionPanelPatches
    {
        [HarmonyPatch(typeof(CharacterStageSpellSelectionPanel), "Refresh")]
        class CharacterStageSpellSelectionPanel_Refresh
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var known_cantrips = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("KnownCantrips"));

                codes[known_cantrips + 2] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                 new Func<List<int>, int, int>(getCantripsNumber).Method
                                                 );

                var classes_history = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("ClassesHistory"));
                codes.Insert(classes_history + 4, new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                                 new Func<bool, bool>(needHigherSpellLevels).Method
                                                                                ));


                return codes.AsEnumerable();
            }


            internal static bool needHigherSpellLevels(bool need)
            {
                if (need)
                {
                    return true;
                }

                var service = ServiceRepository.GetService<ICharacterBuildingService>();
                if (service == null || !service.PointPoolStacks.ContainsKey(HeroDefinitions.PointsPoolType.Spell))
                {
                    return false;
                }

                return service.PointPoolStacks[HeroDefinitions.PointsPoolType.Spell].activePools.Any(a => a.Value.maxPoints > 0);
            }


            internal static int getCantripsNumber(List<int> known_cantrips, int count)
            {
                if (known_cantrips[count] > 0)
                {
                    return known_cantrips[count];
                }
                var service = ServiceRepository.GetService<ICharacterBuildingService>();
                if (service == null || !service.PointPoolStacks.ContainsKey(HeroDefinitions.PointsPoolType.Cantrip))
                {
                    return known_cantrips[count];
                }

                return service.PointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip].activePools.Any(a => a.Value.maxPoints > 0) ? 1 : 0;
            }
        }
    }
}
