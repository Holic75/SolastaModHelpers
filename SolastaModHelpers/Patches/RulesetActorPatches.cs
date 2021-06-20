using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RuleActorPatcher
    {
        [HarmonyPatch(typeof(RulesetActor), "RollDiceAndSum")]
        class RulesetActor_RollDiceAndSum
        {
            internal static void Postfix(RulesetActor __instance,
                                        RuleDefinitions.DieType diceType,
                                        RuleDefinitions.RollContext context,
                                        int diceNumber,
                                        List<int> rolledValues,
                                        bool canRerollDice,
                                        ref int __result)
            {
                List<FeatureDefinition> features = new List<FeatureDefinition>();
                __instance.EnumerateFeaturesToBrowse<NewFeatureDefinitions.IModifyDiceRollValue>(features, (Dictionary<FeatureDefinition, RuleDefinitions.FeatureOrigin>)null);
                foreach (NewFeatureDefinitions.IModifyDiceRollValue f in features)
                {
                    __result = f.processDiceRoll(context, __result, __instance);
                }
                if (context == RuleDefinitions.RollContext.DamageValueRoll)
                {
                    __instance.ProcessConditionsMatchingInterruption((RuleDefinitions.ConditionInterruption)ExtendedEnums.ExtraConditionInterruption.RollsForDamage, 0);
                }
            }
        }
    }
}
