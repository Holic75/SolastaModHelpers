using HarmonyLib;
using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class CursorLocationSelectTargetPatcher
    {
        [HarmonyPatch(typeof(CursorLocationSelectTarget), "IsFilteringValid")]
        internal static class CursorLocationSelectTarget_IsFilteringValid_Patch
        {
            internal static void Postfix(CursorLocationSelectTarget __instance, GameLocationCharacter target, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                var effect = __instance.effectDescription;
                var tag = (ExtendedEnums.ExtraTargetFilteringTag)effect.TargetFilteringTag;
                if (effect == null)
                {
                    return;
                }

                foreach (var s in effect.ImmuneCreatureFamilies)
                {
                    var immune_condition = Helpers.Misc.extractImmuneCondition(s);
                    if (immune_condition != null)
                    {
                        if (target.RulesetCharacter != null && target.RulesetCharacter.HasConditionOfType(immune_condition))
                        {
                            __result = false;
                            var action_modifier = __instance.actionModifier;
                            action_modifier.FailureFlags.Add("Failure/&FailureFlagTargetIncorrectCreatureFamily");
                            return;
                        }
                    }

                    var immune_no_condition = Helpers.Misc.extractImmuneIfHasNoCondition(s);
                    if (immune_no_condition != null)
                    {
                        if (target.RulesetCharacter != null && !target.RulesetCharacter.HasConditionOfType(immune_no_condition))
                        {
                            __result = false;
                            var action_modifier = __instance.actionModifier;
                            action_modifier.FailureFlags.Add("Failure/&FailureFlagTargetIncorrectCreatureFamily");
                            return;
                        }
                    }
                }

                if ((tag & ExtendedEnums.ExtraTargetFilteringTag.NonCaster) != ExtendedEnums.ExtraTargetFilteringTag.No
                    && target == __instance.ActionParams.ActingCharacter)
                {
                    __result = false;
                    var action_modifier = __instance.actionModifier;
                    action_modifier.FailureFlags.Add("Failure/&FailureFlagTargetIncorrectCreatureFamily");
                    return;
                }
                if ((tag & ExtendedEnums.ExtraTargetFilteringTag.MetalArmor) != ExtendedEnums.ExtraTargetFilteringTag.No)
                {
                    if (!target.RulesetCharacter.Tags.Contains("MetalArmor"))
                    {
                        __result = false;
                        var action_modifier = __instance.actionModifier;
                        action_modifier.FailureFlags.Add("Failure/&FailureFlagTargetIncorrectArmor");
                        return;
                    }
                }

                if ((tag & ExtendedEnums.ExtraTargetFilteringTag.NoHeavyArmor) != ExtendedEnums.ExtraTargetFilteringTag.No)
                {
                    var hero = target.RulesetCharacter as RulesetCharacterHero;
                    if (hero != null)
                    {
                        RulesetItem equipedItem = hero.characterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeTorso].EquipedItem;
                        bool is_heavy =  equipedItem != null 
                                         && equipedItem.ItemDefinition.IsArmor 
                                         && (DatabaseRepository.GetDatabase<ArmorCategoryDefinition>().GetElement(DatabaseRepository.GetDatabase<ArmorTypeDefinition>().GetElement(equipedItem.ItemDefinition.ArmorDescription.ArmorType, false).ArmorCategory, false) 
                                           == DatabaseHelper.ArmorCategoryDefinitions.HeavyArmorCategory);
                        if (is_heavy)
                        {
                            __result = false;
                            var action_modifier = __instance.actionModifier;
                            action_modifier.FailureFlags.Add("Failure/&FailureFlagTargetIncorrectArmor");
                            return;
                        }
                    }
                }

                var spell_with_attack = __instance.actionParams?.RulesetEffect?.SourceDefinition as NewFeatureDefinitions.IPerformAttackAfterMagicEffectUse;
                if (spell_with_attack != null && !spell_with_attack.canUse(__instance.actionParams.actingCharacter, target))
                {
                    __result = false;
                    var action_modifier = __instance.actionModifier;
                    action_modifier.FailureFlags.Add("Failure/&FailureFlagTargetIncorrectWeapon");
                    return;
                }
                 
                return;
            }
        }
    }
}
