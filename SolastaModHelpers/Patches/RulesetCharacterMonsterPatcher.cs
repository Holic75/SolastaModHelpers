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
            internal static void Postfix(RulesetCharacterMonster __instance,
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


        [HarmonyPatch(typeof(RulesetCharacterMonster), "RefreshAttackModes")]
        class RulesetCharacterMonster_RefreshAttackModes
        {
            internal static void Postfix(RulesetCharacterMonster __instance)
            {
                var game_location_character = Helpers.Misc.findGameLocationCharacter(__instance);

                if (__instance.monsterDefinition.GroupAttacks || __instance.AttackModes.Count() <= 2
                    || __instance.AttackModes[0].actionType != __instance.AttackModes[2].actionType
                    || __instance.AttackModes[0].ranged == __instance.AttackModes[2].ranged
                    || __instance.side == RuleDefinitions.Side.Enemy
                    || game_location_character == null)
                {
                    return;
                }

                IGameLocationBattleService battle_service = ServiceRepository.GetService<IGameLocationBattleService>();
                if (battle_service?.Battle == null || battle_service.Battle.activeContender != game_location_character)
                {
                    return;
                }
             
                var enemies = battle_service.Battle.enemyContenders;
                if (enemies.Count() == 0)
                {
                    return;
                }

                foreach (var e in enemies)
                {
                    if (battle_service.IsWithinXCells(game_location_character, e, __instance.AttackModes[0].ReachRange))
                    {
                        return;
                    }
                }
                                
                var attack0 = __instance.AttackModes[0];
                var attack1 = __instance.AttackModes[2];
                __instance.AttackModes[0] = attack1;
                __instance.AttackModes[2] = attack0;
            }
        }
    }
}
