using HarmonyLib;
using SolastaModHelpers.NewFeatureDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class RulesetCharacterHeroPatcher
    {
        //support for IScalingArmorClassBonus
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
                var features = Helpers.Accessors.extractFeaturesHierarchically<IScalingArmorClassBonus>(character);
                List<(IScalingArmorClassBonus, int)> exclusive_feature_value = new List<(IScalingArmorClassBonus, int)>();
                foreach (var f in features)
                {
                    if (!f.isExclusive())
                    {
                        f.apply(attribute, character, f.precomputeBonusValue(character));
                    }
                    else
                    {
                        exclusive_feature_value.Add((f, f.precomputeBonusValue(character)));
                    }
                }

                if (exclusive_feature_value.Empty())
                {
                    return;
                }

                var best_feature_value = exclusive_feature_value[0];
                for (int i = 1; i < exclusive_feature_value.Count(); i++)
                {
                    if (exclusive_feature_value[i].Item2 > best_feature_value.Item2)
                    {
                        best_feature_value = exclusive_feature_value[i];
                    }
                }
                best_feature_value.Item1.apply(attribute, character, best_feature_value.Item2);
            }
        }

        //support for ICanUSeDexterityWithWeapon
        [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshAttackMode")]
        class RulesetCharacterHero_RefreshAttackMode
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var finesse_string_load = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ldstr && x.operand.ToString().Contains("Finesse"));

                codes[finesse_string_load - 1] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0); //load this
                codes[finesse_string_load] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Func<WeaponDescription, RulesetCharacterHero, bool>(canUseDexterity).Method
                                                                 );
                codes.RemoveAt(finesse_string_load + 1);

                var ranged = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("Ranged"));
                codes.InsertRange(ranged + 1,
                                  new List<CodeInstruction>
                                  {
                                      new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0), //load this
                                      new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldloc_0), //attack mode
                                       new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Action<RulesetCharacterHero, RulesetAttackMode>(modifyAttackAbilityScore).Method)
                                  });
                return codes.AsEnumerable();
            }

            static bool canUseDexterity(WeaponDescription weapon_description, RulesetCharacterHero hero)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<ICanUSeDexterityWithWeapon>(hero);
                foreach (var f in features)
                {
                    if (f.worksOn(hero, weapon_description))
                    {
                        return true;
                    }
                }

                return weapon_description.WeaponTags.Contains("Finesse");
            }


            static void modifyAttackAbilityScore(RulesetCharacterHero hero, RulesetAttackMode attack_mode)
            {

                var features = Helpers.Accessors.extractFeaturesHierarchically<IAttackAbilityScoreModeModifier>(hero);
                foreach (var f in features)
                {
                    f.applyAbilityScoreModification(hero, attack_mode, attack_mode.SourceObject as RulesetItem);
                }
            }

            internal static void Postfix(RulesetCharacterHero __instance,
                                        ActionDefinitions.ActionType actionType,
                                        ItemDefinition itemDefinition,
                                        WeaponDescription weaponDescription,
                                        bool freeOffHand,
                                        bool canAddAbilityDamageBonus,
                                        string slotName,
                                        List<IAttackModificationProvider> attackModifiers,
                                        Dictionary<FeatureDefinition, RuleDefinitions.FeatureOrigin> featuresOrigin,
                                        RulesetItem weapon,
                                        ref RulesetAttackMode __result)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<IAttackModeModifier>(__instance);
                foreach (var f in features)
                {
                    f.apply(__instance, __result, weapon);
                }


                var features2 = Helpers.Accessors.extractFeaturesHierarchically<DoubleDamageOnSpecificWeaponTypes>(__instance);
                foreach (var f in features2)
                {
                    f.apply(__instance, __result, weapon);
                }
            }
        }

        //Apply certain actions to characters upon save load (to correct feats for example)
        [HarmonyPatch(typeof(RulesetCharacterHero), "PostLoad")]
        internal class RulesetCharacterHero_PostLoad
        {
            internal static void Postfix(RulesetCharacterHero __instance)
            {
                foreach (var a in Common.postload_actions)
                {
                    a(__instance);
                }
            }
        }

        //support for IAddExtraAttacks
        [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshAttackModes")]
        class RulesetCharacterHero_RefreshAttackModes
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var reference_stloc = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Stloc_2);
                var insert_point = reference_stloc + 2;
                
                codes.InsertRange(insert_point,
                              new HarmonyLib.CodeInstruction[]
                              {
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0), //load this == RulesetHeroCharacter
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Action<RulesetCharacterHero>(addExtraAttacks).Method
                                                                 )
                              }
                            );
                return codes.AsEnumerable();
            }

            static void addExtraAttacks(RulesetCharacterHero character)
            {
                var features = Helpers.Accessors.extractFeaturesHierarchically<IAddExtraAttacks>(character);
                foreach (var f in features)
                {
                    f.tryAddExtraAttack(character);
                }
            }
        }


        //Currently game will not detect feature origin if feature is given via FeatureDefinitionFeatureSet
        //This patch fixes this issue, still does not work for recursive FeatureDefinitionFeatureSet inclusion
        [HarmonyPatch(typeof(RulesetCharacterHero), "LookForFeatureOrigin")]
        internal class RulesetCharacterHero_LookForFeatureOrigin
        {
            internal static void Postfix(RulesetCharacterHero __instance,
                                        FeatureDefinition featureDefinition,
                                        ref CharacterRaceDefinition raceDefinition,
                                        ref CharacterClassDefinition classDefinition,
                                        ref FeatDefinition featDefinition)
            {
                if (featDefinition != null
                    || raceDefinition != null
                    || classDefinition != null)
                {
                    return;
                }
                foreach (FeatureUnlockByLevel featureUnlock in __instance.RaceDefinition.FeatureUnlocks)
                {
                    if (((featureUnlock.FeatureDefinition as FeatureDefinitionFeatureSet)?.FeatureSet.Contains(featureDefinition)).GetValueOrDefault())
                    {
                        raceDefinition = __instance.RaceDefinition;
                        return;
                    }
                }
                if ((BaseDefinition)__instance.SubRaceDefinition != (BaseDefinition)null)
                {
                    foreach (FeatureUnlockByLevel featureUnlock in __instance.SubRaceDefinition.FeatureUnlocks)
                    {
                        if (((featureUnlock.FeatureDefinition as FeatureDefinitionFeatureSet)?.FeatureSet.Contains(featureDefinition)).GetValueOrDefault())
                        {
                            raceDefinition = __instance.SubRaceDefinition;
                            return;
                        }
                    }
                }
                foreach (KeyValuePair<CharacterClassDefinition, int> classesAndLevel in __instance.ClassesAndLevels)
                {
                    foreach (FeatureUnlockByLevel featureUnlock in classesAndLevel.Key.FeatureUnlocks)
                    {
                        if (((featureUnlock.FeatureDefinition as FeatureDefinitionFeatureSet)?.FeatureSet.Contains(featureDefinition)).GetValueOrDefault())
                        {
                            classDefinition = classesAndLevel.Key;
                            return;
                        }
                    }
                    if (__instance.ClassesAndSubclasses.ContainsKey(classesAndLevel.Key) && (BaseDefinition)__instance.ClassesAndSubclasses[classesAndLevel.Key] != (BaseDefinition)null)
                    {
                        foreach (FeatureUnlockByLevel featureUnlock in __instance.ClassesAndSubclasses[classesAndLevel.Key].FeatureUnlocks)
                        {
                            if (((featureUnlock.FeatureDefinition as FeatureDefinitionFeatureSet)?.FeatureSet.Contains(featureDefinition)).GetValueOrDefault())
                            {
                                classDefinition = classesAndLevel.Key;
                                return;
                            }
                        }
                    }
                }
                foreach (FeatDefinition trainedFeat in __instance.TrainedFeats)
                {
                    foreach (var feature in trainedFeat.Features.OfType<FeatureDefinitionFeatureSet>())
                    {
                        if (feature.FeatureSet.Contains(featureDefinition))
                        {
                            featDefinition = trainedFeat;
                            return;
                        }
                    }
                }
            }
        }
    }
}
