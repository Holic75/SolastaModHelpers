using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class CharacterBuildingManagerPatcher
    {
        class CharacterBuildingManagerSetPointPoolPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "SetPointPool")]
            internal static class CharacterBuildingManager_SetPointPool_Patch
            {
                internal static bool Prefix(CharacterBuildingManager __instance, HeroDefinitions.PointsPoolType pointPoolType, string tag, ref int maxNumber)
                {
                    if (pointPoolType != HeroDefinitions.PointsPoolType.Spell)
                    {
                        return true;
                    }
                    var hero = __instance.HeroCharacter;
                    if (hero == null)
                    {
                        return true;
                    }
                    int bonus_known_spells = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IKnownSpellNumberIncrease>(__instance.HeroCharacter)
                                                                         .Aggregate(0, (old, next) => old += next.getKnownSpellsBonus(__instance, hero));

                    maxNumber = maxNumber + bonus_known_spells;
                    return true;
                }
            }
        }


        class CharacterBuildingManagerBrowseGrantedFeaturesHierarchicallyPatcher
        {
            [HarmonyPatch(typeof(CharacterBuildingManager), "BrowseGrantedFeaturesHierarchically")]
            internal static class CharacterBuildingManager_BrowseGrantedFeaturesHierarchically_Patch
            {
                internal static bool Prefix(CharacterBuildingManager __instance, List<FeatureDefinition> grantedFeatures, string tag)
                {
                    grantSpells(__instance, grantedFeatures);
                    return true;
                }

                static void grantSpells(CharacterBuildingManager __instance, List<FeatureDefinition> grantedFeatures)
                {
                    var features = grantedFeatures.OfType<NewFeatureDefinitions.IGrantKnownSpellsOnLevelUp>().ToList();
                    features.AddRange(Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IGrantKnownSpellsOnLevelUp>(__instance.HeroCharacter));

                    CharacterClassDefinition current_class;
                    int current_level;
                    __instance.GetLastAssignedClassAndLevel(out current_class, out current_level);

                    HashSet<SpellDefinition> spells = new HashSet<SpellDefinition>();
                    foreach (var f in features)
                    {
                        f.maybeGrantSpellsOnLevelUp(__instance);

                    }

                    return;
                }
            }
        }
    }
}
