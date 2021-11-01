using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class FeatureDescriptionItemPatcher
    {
        [HarmonyPatch(typeof(FeatureDescriptionItem), "Bind")]
        class FeatureDescriptionItem_Bind
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var contains_check = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt 
                                                          && x.operand.ToString().Contains("Contains") 
                                                          && !x.operand.ToString().Contains("Key"));

                codes[contains_check] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<List<FeatureDefinition>, FeatureDefinition, bool>(isFeatureForbidden).Method
                                                                 );

                return codes.AsEnumerable();
            }
        
            internal static bool isFeatureForbidden(List<FeatureDefinition> allActiveFeatures, FeatureDefinition feature2)
            {
                if (allActiveFeatures.Contains(feature2))
                {
                    return true;
                }

                var character  = ServiceRepository.GetService<ICharacterBuildingService>().HeroCharacter;
                if (character == null)
                {
                    return false;
                }

                return NewFeatureDefinitions.FeatureData.isFeatureForbidden(feature2, character);
            }
        }


        [HarmonyPatch(typeof(FeatureDescriptionItem), "GetCurrentFeature")]
        class FeatureDescriptionItem_GetCurrentFeature
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var contains_check = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("Contains"));

                codes[contains_check] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<List<FeatureDefinition>, FeatureDefinition, bool>(FeatureDescriptionItem_Bind.isFeatureForbidden).Method
                                                                 );

                return codes.AsEnumerable();
            }
        }
    }
}
