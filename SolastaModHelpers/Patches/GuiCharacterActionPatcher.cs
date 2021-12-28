using HarmonyLib;
using SolastaModHelpers.NewFeatureDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolastaModHelpers.Patches
{
    class GuiCharacterActionPatcher
    {
        [HarmonyPatch(typeof(GuiCharacterAction), "SetupUseSlots")]
        internal static class GuiCharacterAction_SetupUseSlots
        {
            static void  Postfix(GuiCharacterAction __instance, RectTransform useSlotsTable, GameObject slotStatusPrefab)
            {
                if (useSlotsTable.gameObject.activeSelf)
                    return;

                int num1 = 0;
                int num2 = 0;
                if ((ExtendedActionId)__instance.actionDefinition.Id == ExtendedActionId.ElementalForm)
                {
                    RulesetUsablePower powerFromDefinition = __instance.actingCharacter.RulesetCharacter.GetPowerFromDefinition(__instance.actionDefinition.ActivatedPower);
                    if (powerFromDefinition != null)
                    {
                        num1 = powerFromDefinition.MaxUses;
                        num2 = powerFromDefinition.RemainingUses;
                    }
                }
                useSlotsTable.gameObject.SetActive(num1 > 0);
                if (!useSlotsTable.gameObject.activeSelf)
                    return;
                while (useSlotsTable.childCount < num1)
                    Gui.GetPrefabFromPool(slotStatusPrefab, (Transform)useSlotsTable);
                for (int index = 0; index < useSlotsTable.childCount; ++index)
                {
                    Transform child = useSlotsTable.GetChild(index);
                    if (index < num1)
                    {
                        child.gameObject.SetActive(true);
                        SlotStatus component = child.GetComponent<SlotStatus>();
                        component.Used.gameObject.SetActive(index >= num2);
                        component.Available.gameObject.SetActive(index < num2);
                    }
                    else
                        child.gameObject.SetActive(false);
                }
            }



        }
    }
}
