using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SolastaModHelpers.Patches
{
    class RestModuleHitDicePatcher
    {
        [HarmonyPatch(typeof(RestModuleHitDice), "Bind")]
        internal static class RestModuleHitDice_Bind_Patch
        {
            static public Dictionary<RulesetCharacterHero, Dictionary<string, RuleDefinitions.DieType>> extra_healing_dice_per_hero;

            internal static void Postfix(RestModuleHitDice __instance, RuleDefinitions.RestType restType,
                                         RestDefinitions.RestStage restStage,
                                         RestModule.RestModuleRefreshedHandler restModuleRefreshed)
            {
                extra_healing_dice_per_hero = new Dictionary<RulesetCharacterHero, Dictionary<string, RuleDefinitions.DieType>>();
                foreach (var h in __instance.Heroes)
                {
                    extra_healing_dice_per_hero[h] = new Dictionary<string, RuleDefinitions.DieType>();
                }
                foreach (var h in __instance.Heroes)
                {
                    h.EnumerateFeaturesToBrowse<NewFeatureDefinitions.FeatureDefinitionExtraHealingDieOnShortRest>(h.FeaturesToBrowse);

                    foreach (NewFeatureDefinitions.FeatureDefinitionExtraHealingDieOnShortRest f in h.FeaturesToBrowse)
                    {
                        Main.Logger.Log("Found Extra Healing Die feature: " + f.name + " on " + h.Name);
                        if (f.ApplyToParty)
                        {
                            foreach (var hh in __instance.Heroes)
                            {
                                if (!extra_healing_dice_per_hero[hh].ContainsKey(f.tag) || extra_healing_dice_per_hero[hh][f.tag] < f.DieType)
                                {
                                    extra_healing_dice_per_hero[hh][f.tag] = f.DieType;
                                }
                            }
                        }
                        else
                        {
                            if (!extra_healing_dice_per_hero[h].ContainsKey(f.tag) || extra_healing_dice_per_hero[h][f.tag] < f.DieType)
                            {
                                extra_healing_dice_per_hero[h][f.tag] = f.DieType;
                            }
                        }
                    }
                }
            }
        }

        class RulesetCharacterHeroRollHitDieOnRestPatcher
        {
            [HarmonyPatch(typeof(RulesetCharacterHero), "RollHitDie")]
            internal static class RestModuleHitDice_RollHitDie_Patch
            {
                internal static void Postfix(RulesetCharacterHero __instance)
                {
                    int damage = __instance.GetAttribute("HitPoints", false).CurrentValue - __instance.CurrentHitPoints;
                    if (damage <= 0)
                        return;

                    if (RestModuleHitDice_Bind_Patch.extra_healing_dice_per_hero.ContainsKey(__instance))
                    {
                        foreach (var kv in RestModuleHitDice_Bind_Patch.extra_healing_dice_per_hero[__instance])
                        {
                            int first_roll, second_roll;
                            var healing_amount = RuleDefinitions.RollDie(kv.Value, RuleDefinitions.AdvantageType.None, out first_roll, out second_roll, 0.0f);
                            __instance.ReceiveHealing(healing_amount, false, 0UL, RuleDefinitions.HealingCap.MaximumHitPoints);
                            Main.Logger.Log(__instance.Name + $" Received Extra Healing Die {kv.Value.ToString()}(={healing_amount}) due to " + kv.Key);
                        }
                        RestModuleHitDice_Bind_Patch.extra_healing_dice_per_hero.Remove(__instance);
                    }
                }
            }
        }
    }
}
