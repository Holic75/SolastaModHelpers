using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class CharacterActionPanelPatcher
    {
        //remove restricted or hidden powers from power selection panel
        [HarmonyPatch(typeof(PowerSelectionPanel), "Bind")]
        class PowerSelectionPanel_Bind
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var power_canceled_handler = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Call && x.operand.ToString().Contains("PowerCancelled"));

                codes.InsertRange(power_canceled_handler + 1,
                              new List<CodeInstruction> { new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0),
                                                          new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1),
                                                          new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Action<PowerSelectionPanel, RulesetCharacter>(removeForbiddenPowers).Method)
                                                        }
                            );
                return codes.AsEnumerable();
            }

            static void removeForbiddenPowers(PowerSelectionPanel panel, RulesetCharacter character)
            {
                foreach (var p in panel.relevantPowers.ToArray())
                {
                    //Main.Logger.Log("Processing Power: " + p.PowerDefinition.name);
                    if (((p.PowerDefinition as NewFeatureDefinitions.IPowerRestriction)?.isForbidden(character)).GetValueOrDefault()
                        || ((p.PowerDefinition as NewFeatureDefinitions.IHiddenAbility)?.isHidden()).GetValueOrDefault())
                    {
                        panel.relevantPowers.Remove(p);
                        //Main.Logger.Log("Removing Power: " + p.PowerDefinition.name);
                    }
                }
            }
        }
    }
}
