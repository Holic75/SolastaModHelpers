using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA;

namespace SolastaModHelpers.Patches
{
    class GameLocationCharacterManagerPatcher
    {
        //Add support for custom features triggering effects on proxy summon
        [HarmonyPatch(typeof(GameLocationCharacterManager), "CreateAndBindEffectProxy")]
        internal static class GameLocationCharacterManager_CreateAndBindEffectProxy_Patch
        {
            internal static void Prefix(GameLocationCharacterManager __instance,
                                         RulesetActor rulesetEntity,
                                         RulesetEffect rulesetEffect,
                                         int3 position,
                                         EffectProxyDefinition effectProxyDefinition)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IApplyEffectOnProxySummon>(rulesetEntity);
                foreach (var f in features)
                {
                    f.processProxySummon(rulesetEntity, rulesetEffect, position, effectProxyDefinition);
                }
            }
        }
    }
}
