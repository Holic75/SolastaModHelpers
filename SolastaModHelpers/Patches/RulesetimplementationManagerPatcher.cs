using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RulesetImplementationManagerPatcher
    {
        [HarmonyPatch(typeof(RulesetImplementationManager), "ApplyDamageForm")]
        class RulesetimplementationManager_ApplyDamageForm
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var dice_number_store_load = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("DiceNumber")) + 10;
                if  (codes[dice_number_store_load].opcode != System.Reflection.Emit.OpCodes.Stloc_S)
                {
                    throw new Exception("failed to patch RulesetimplementationManager_ApplyDamageForm");
                }

                codes.InsertRange(dice_number_store_load,
                              new HarmonyLib.CodeInstruction[]
                              {
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_2), //formParams
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<int, RulesetImplementationDefinitions.ApplyFormsParams, int>(processDiceNumber).Method
                                                                 )
                              }
                            );
                return codes.AsEnumerable();
            }

            static int processDiceNumber(int base_dice_num, RulesetImplementationDefinitions.ApplyFormsParams form_params)
            {
                var dice_num = base_dice_num;
                var character = (form_params.sourceCharacter as RulesetCharacterHero);
                if (character == null)
                {
                    return dice_num;
                }
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IDamageDiceIncrease>(character);
                foreach (var f in features)
                {
                    dice_num += f.extraDice(form_params);
                }
                return dice_num;
            }
        }
    }
}
