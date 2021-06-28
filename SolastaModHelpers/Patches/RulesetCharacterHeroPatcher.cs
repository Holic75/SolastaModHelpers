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
                foreach (var f in features)
                {
                    f.apply(attribute, character);
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshAttackMode")]
        class RulesetCharacterHero_RefreshAttackMode
        {
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
            }
        }


        [HarmonyPatch(typeof(RulesetCharacterHero), "PostLoad")]
        internal class RulesetCharacterHero_PostLoad
        {
            internal static void Postfix(RulesetCharacterHero __instance)
            {
                refreshMaxPowerUses(__instance);
            }


            internal static void refreshMaxPowerUses(RulesetCharacterHero hero)
            {
                if (hero == null)
                {
                    return;
                }
                var features = Helpers.Accessors.extractFeaturesHierarchically<NewFeatureDefinitions.IPowerNumberOfUsesIncrease>(hero);

                var usable_powers = hero.usablePowers;
                foreach (var p in usable_powers)
                {
                    if (p?.powerDefinition == null)
                    {
                        continue;
                    }

                    p.maxUses = p.PowerDefinition.fixedUsesPerRecharge;
                    foreach (var f in features)
                    {
                        f.apply(hero, p);
                    }
                }
            }
        }


        /*[HarmonyPatch(typeof(RulesetCharacterHero), "RefreshAttackModes")]
        class RulesetCharacterHero_RefreshAttackMods
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var reference_stloc = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Stloc_2);
                var insert_point = reference_stloc - 1;

                codes.InsertRange(insert_point,
                              new HarmonyLib.CodeInstruction[]
                              {
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0), //load this == RulesetHeroCharacter
                                  new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                 new Action<RulesetCharacterHero>(addExtraUnarmedAttacks).Method
                                                                 )
                              }
                            );
                return codes.AsEnumerable();
            }

            static void addExtraUnarmedAttacks(RulesetCharacterHero character)
            {
                ItemDefinition strikeDefinition = character.UnarmedStrikeDefinition;
                character.AttackModes.Add(character.RefreshAttackMode(ActionDefinitions.ActionType.Bonus, strikeDefinition, 
                                                                      strikeDefinition.WeaponDescription, false, true, 
                                                                      character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0].Name,
                                                                      character.attackModifiers, character.FeaturesOrigin, (RulesetItem)null));
            }
        }*/


    }
}
