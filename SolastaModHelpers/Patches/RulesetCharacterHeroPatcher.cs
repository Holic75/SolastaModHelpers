using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RulesetCharacterHeroPatcher
    {
        [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshArmorClass")]
        class RulesetCharacterHero_RefreshArmorClass
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var dexterity_string_load = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ldstr && x.operand.ToString().Contains("Dexterity"));
                var insert_point = dexterity_string_load + 4;

                codes.InsertRange(insert_point,
                              new HarmonyLib.CodeInstruction[]
                              {
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldloc_0), //load attribute
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0), //load this == RulesetHeroCharacter
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Action<RulesetAttribute, RulesetCharacterHero>(applyArmorModifiers).Method
                                                                 )
                              }
                            );
                return codes.AsEnumerable();
            }

            static void applyArmorModifiers(RulesetAttribute attribute, RulesetCharacterHero character)
            {
                var features = character.ActiveFeatures.Values.Aggregate(new List<NewFeatureDefinitions.IScalingArmorClassBonus>(),
                                                                          (old, next) =>
                                                                          {
                                                                              old.AddRange(Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IScalingArmorClassBonus>(next));
                                                                              return old;
                                                                          }
                                                                          );
                foreach (var f in features)
                {
                    f.apply(attribute, character);
                }
            }
        }
    }
}
