using HarmonyLib;
using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class DoNotApplySpellOnSelfPatcher
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
                            break;
                        }

                    }
                    
                }

                if (__result && (tag & ExtendedEnums.ExtraTargetFilteringTag.NonCaster) != ExtendedEnums.ExtraTargetFilteringTag.No)
                {
                    __result = target != __instance.ActionParams.ActingCharacter;
                }
                if (__result && (tag & ExtendedEnums.ExtraTargetFilteringTag.NoHeavyArmor) != ExtendedEnums.ExtraTargetFilteringTag.No)
                {
                    var hero = target.RulesetCharacter as RulesetCharacterHero;
                    if (hero != null)
                    {
                        RulesetItem equipedItem = hero.characterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeTorso].EquipedItem;
                        bool is_heavy =  equipedItem != null 
                                         && equipedItem.ItemDefinition.IsArmor 
                                         && (DatabaseRepository.GetDatabase<ArmorCategoryDefinition>().GetElement(DatabaseRepository.GetDatabase<ArmorTypeDefinition>().GetElement(equipedItem.ItemDefinition.ArmorDescription.ArmorType, false).ArmorCategory, false) 
                                           == DatabaseHelper.ArmorCategoryDefinitions.HeavyArmorCategory);
                        __result = !is_heavy;
                    }
                }
                 

                if (!__result)
                {
                    var action_modifier = __instance.actionModifier;
                    action_modifier.FailureFlags.Add("Failure/&FailureFlagTargetIncorrectCreatureFamily");
                }
                return;
            }
        }
    }
}
