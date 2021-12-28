using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class CharacterReactionItemPatcher
    {
        [HarmonyPatch(typeof(CharacterReactionItem), "Bind")]
        class CharacterReactionItem_Bind
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var initialize_spellrepertoire = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Stloc_0);


                codes[initialize_spellrepertoire - 1] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1); //load reactionRequest
                codes.Insert(initialize_spellrepertoire,
                              new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<ReactionRequest, RulesetSpellRepertoire>(initializeSpellRepertoire).Method));
                return codes.AsEnumerable();
            }

            static RulesetSpellRepertoire initializeSpellRepertoire(ReactionRequest reactionRequest)
            {
                return reactionRequest?.reactionParams?.spellRepertoire;
            }
        }
    }
}
