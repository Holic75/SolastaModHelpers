using HarmonyLib;
using SolastaModHelpers.NewFeatureDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RulesetCharacterMonsterPatcher
    {
        [HarmonyPatch(typeof(RulesetCharacterMonster), "BuildAttackMode")]
        class RulesetCharacterMonster_BuildAttackMode
        {
            internal static void Postfix(RulesetCharacter __instance,
                                        MonsterAttackDefinition monsterAttackDefinition,
                                        ref RulesetAttackMode __result)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<IAttackModeModifier>(__instance);
                foreach (var f in features)
                {
                    f.apply(__instance, __result, null);
                }

                var attack_modifiers = Helpers.Accessors.extractFeaturesHierarchically<IAttackModificationProvider>(__instance);
                foreach (var f in attack_modifiers)
                {
                    __result.ToHitBonus += f.AttackRollModifier;
                    __result.ToHitBonusTrends.Add(new RuleDefinitions.TrendInfo(f.AttackRollModifier, RuleDefinitions.FeatureSourceType.MonsterFeature, (f as FeatureDefinition)?.Name, (f as FeatureDefinition)));
                    DamageForm firstDamageForm = __result.EffectDescription.FindFirstDamageForm();
                    firstDamageForm.BonusDamage += f.DamageRollModifier;
                }

            }
        }
    }
}
