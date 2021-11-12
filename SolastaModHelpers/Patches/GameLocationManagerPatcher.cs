using HarmonyLib;
using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class GameLocationManagerPatcher
    {
        
        [HarmonyPatch(typeof(GameLocationManager), "StopCharacterEffectsIfRelevant")]
        class GameLocationManager_StopCharacterEffectsIfRelevant
        {

            static bool Prefix(GameLocationManager __instance, bool willEnterChainedLocation)
            {
                //removed summoned monsters upon entering new locations, since the game somehow removes corresponding summoned (and all other conditions)
                //from them and thus they are no longer linked to the caster
                IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>() as GameLocationCharacterManager;
                foreach (var gc in service1.GuestCharacters.ToArray())
                {
                    if (gc.RulesetCharacter.conditionsByCategory.Any(c => c.Value.Any(cc => cc.conditionDefinition == DatabaseHelper.ConditionDefinitions.ConditionConjuredCreature))
                        && !((gc.RulesetCharacter as RulesetCharacterMonster)?.MonsterDefinition?.creatureTags.Contains("KindredSpirit")).GetValueOrDefault())
                    {
                        Main.Logger.Log("Removed Summoned unit on location leave: " + gc.Name);
                        service1.DestroyCharacterBody(gc);
                    }

                }
                return true;
            }

            //prevent summoned monsters removal when entering into chained locaiton
            /*static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var remove_effects = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("Terminate"));
                codes[remove_effects] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Action<RulesetEffect, bool>(maybeTerminate).Method);

                return codes.AsEnumerable();
            }


            static void maybeTerminate(RulesetEffect effect, bool self)
            {
                var spell_effect = effect as RulesetEffectSpell;
                if (spell_effect != null)
                {
                    Main.Logger.Log("Removing: " + spell_effect.spellDefinition.name);
                }
                if (RuleDefinitions.MatchesMagicType(effect.EffectDescription, RuleDefinitions.MagicType.SummonsCreature))
                {
                    Main.Logger.Log("Prevented removal");
                    return;
                }
                effect.Terminate(self);
            }*/
        }
    }
}
