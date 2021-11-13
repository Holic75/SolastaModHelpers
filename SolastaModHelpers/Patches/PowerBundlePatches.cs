using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SolastaModHelpers.Patches.PowerBundlePatches
{
    //set of patches to allow Power Bundle support
    //Since we can not easily add new unity assets to the game, we are going to reuse existing ones supporting SpellBundles
    //(used in SubspellSelectionModal and SubspellItem). 
    //Since they only support spells, we create a set of fake spells mapped to the powers we want to use (master power and subpowers)
    //and interface only through these fake spells, remapping them back to subpowers only when invoking PowerEngaged handler
    class SubspellSelectionModalPatches
    {
        [HarmonyPatch(typeof(SubspellSelectionModal))]
        [HarmonyPatch("Bind", new Type[] {typeof(SpellDefinition), typeof(RulesetCharacter), typeof(RulesetSpellRepertoire), typeof(SpellsByLevelBox.SpellCastEngagedHandler), typeof(int), typeof(RectTransform)})]
        internal static class SubspellSelectionModal_Bind
        {
            internal static bool Prefix(SubspellSelectionModal __instance, SpellDefinition masterSpell,
                                                                           RulesetCharacter caster,
                                                                           RulesetSpellRepertoire spellRepertoire,
                                                                           SpellsByLevelBox.SpellCastEngagedHandler spellCastEngaged,
                                                                           int slotLevel,
                                                                           RectTransform masterSpellBox)
            {
                var master_power = NewFeatureDefinitions.PowerBundleData.findPowerFromSpell(masterSpell) as NewFeatureDefinitions.IPowerBundle;

                if (master_power == null)
                {
                    return true;
                }
                //Main.Logger.Log("Creating Power Bundle");
                bool activeSelf1 = __instance.gameObject.activeSelf;
                bool activeSelf2 = __instance.gameObject.activeSelf;
                __instance.gameObject.SetActive(true);
                __instance.mainPanel.gameObject.SetActive(true);
                __instance.masterSpell = masterSpell;
                __instance.spellRepertoire = new RulesetSpellRepertoire();//will be used to store spells mapped to subpowers
                __instance.spellCastEngaged = spellCastEngaged;
                __instance.deviceFunctionEngaged = (UsableDeviceFunctionBox.DeviceFunctionEngagedHandler)null;
                __instance.slotLevel = slotLevel;

                //stored spells mapped to subpowers in knownSpells list
                var subpowers = master_power.getAvailablePowers(caster);
                __instance.spellRepertoire.knownSpells = new List<SpellDefinition>();
                foreach (var p in subpowers)
                {
                    __instance.spellRepertoire.knownSpells.Add(NewFeatureDefinitions.PowerBundleData.findSpellFromPower(p.PowerDefinition));
                }
                //Main.Logger.Log($"Found {subpowers.Count} subpowers");
                while (__instance.subspellsTable.childCount < subpowers.Count)
                    Gui.GetPrefabFromPool(__instance.subspellItemPrefab, (Transform)__instance.subspellsTable);
                for (int index = 0; index < __instance.subspellsTable.childCount; ++index)
                {
                    Transform child = __instance.subspellsTable.GetChild(index);
                    SubspellItem component = child.GetComponent<SubspellItem>();
                    if (index < subpowers.Count)
                    {
                        child.gameObject.SetActive(true);
                        component.Bind(caster, __instance.spellRepertoire.knownSpells[index],
                                               index, new SubspellItem.OnActivateHandler(__instance.OnActivate));
                    }
                    else
                    {
                        child.gameObject.SetActive(false);
                        component.Unbind();
                    }
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(__instance.mainPanel.RectTransform);
                Vector3[] fourCornersArray = new Vector3[4];
                masterSpellBox.GetWorldCorners(fourCornersArray);
                __instance.mainPanel.RectTransform.position = 0.5f * (fourCornersArray[1] + fourCornersArray[2]);
                __instance.gameObject.SetActive(activeSelf1);
                __instance.mainPanel.gameObject.SetActive(activeSelf2);

                return false;
            }
        }


        [HarmonyPatch(typeof(SubspellSelectionModal), "OnActivate")]
        internal static class SubspellSelectionModal_OnActivate
        {
            internal static bool Prefix(SubspellSelectionModal __instance, int index)
            {
                var master_power = NewFeatureDefinitions.PowerBundleData.findPowerFromSpell(__instance.masterSpell) as NewFeatureDefinitions.IPowerBundle;

                if (master_power == null)
                {
                    return true;
                }

                if (__instance.spellCastEngaged != null)
                    __instance.spellCastEngaged(__instance.spellRepertoire, __instance.spellRepertoire.knownSpells[index], __instance.slotLevel);
                else if (__instance.deviceFunctionEngaged != null)
                    __instance.deviceFunctionEngaged(__instance.guiCharacter, __instance.rulesetItemDevice, __instance.rulesetDeviceFunction, 0, index);
                __instance.Hide();
                return false;
            }
        }



        [HarmonyPatch(typeof(SubspellItem), "Bind")]
        internal static class SubspellItem_OnActivate
        {
            internal static bool Prefix(SubspellItem __instance, RulesetCharacter caster,
                                                                           SpellDefinition spellDefinition,
                                                                           int index,
                                                                           SubspellItem.OnActivateHandler onActivate)
            {
                var power = NewFeatureDefinitions.PowerBundleData.findPowerFromSpell(spellDefinition);

                if (power == null)
                {
                    return true;
                }

                __instance.index = index;
                GuiPowerDefinition guiPowerDefinition = ServiceRepository.GetService<IGuiWrapperService>().GetGuiPowerDefinition(power.Name);
                __instance.spellTitle.Text = guiPowerDefinition.Title;
                __instance.tooltip.TooltipClass = guiPowerDefinition.TooltipClass;
                __instance.tooltip.Content = guiPowerDefinition.Name;
                __instance.tooltip.DataProvider = (object)guiPowerDefinition;
                __instance.tooltip.Context = (object)caster;
                __instance.onActivate = onActivate;
                return false;
            }
        }



        [HarmonyPatch(typeof(UsablePowerBox), "OnActivateCb")]
        internal static class UsablePowerBox_OnActivateCb
        {
            internal static bool Prefix(UsablePowerBox __instance)
            {
                var power_bundle = __instance.usablePower.powerDefinition as NewFeatureDefinitions.IPowerBundle;

                if (power_bundle == null)
                {
                    return true;
                }

                if (__instance.powerEngaged == null)
                    return true;

                var master_spell = NewFeatureDefinitions.PowerBundleData.findSpellFromPower(__instance.usablePower.PowerDefinition);

                SubspellSelectionModal screen = Gui.GuiService.GetScreen<SubspellSelectionModal>();
                var handler = new SpellsByLevelBox.SpellCastEngagedHandler((spellRepertoire, spell, slotLevel) => powerEngagedHandler(__instance, spellRepertoire, spell, slotLevel));
                screen.Bind(master_spell, __instance.activator, null, handler, 0, __instance.RectTransform);
                screen.Show();
              
                return false;
            }

            static void powerEngagedHandler(UsablePowerBox box, RulesetSpellRepertoire spellRepertoire, SpellDefinition spell, int slotLevel)
            {
                var power = NewFeatureDefinitions.PowerBundleData.findPowerFromSpell(spell);
                box.powerEngaged(box.activator.usablePowers.First(u => u.powerDefinition == power));
            }
        }


        //allow termination of all powers from bundle if they are unique
        [HarmonyPatch(typeof(RulesetCharacter), "TerminateMatchingUniquePower")]
        internal static class RulesetCharacter_TerminateMatchingUniquePower
        {
            internal static void Postfix(RulesetCharacter __instance, FeatureDefinitionPower powerDefinition)
            {
                var all_power_bundles = DatabaseRepository.GetDatabase<FeatureDefinitionPower>().GetAllElements().OfType<NewFeatureDefinitions.IPowerBundle>();
                HashSet<FeatureDefinitionPower> powers_to_remove = new HashSet<FeatureDefinitionPower>();

                foreach (var pb in all_power_bundles)
                {
                    if (pb.containsSubpower(powerDefinition))
                    {
                        var powers = pb.getAllPowers();
                        foreach (var p in powers)
                        {
                            if (p != powerDefinition)
                            {
                                powers_to_remove.Add(p as FeatureDefinitionPower);
                            }
                        }
                    }
                }

                __instance.powersToTerminate.Clear();
                foreach (RulesetEffectPower rulesetEffectPower in __instance.powersUsedByMe)
                {
                    if (powers_to_remove.Contains(rulesetEffectPower.PowerDefinition))
                    {
                        __instance.powersToTerminate.Add(rulesetEffectPower);
                    }
                }
                foreach (RulesetEffectPower activePower in __instance.powersToTerminate)
                    __instance.TerminatePower(activePower);
            }
        }
    }
}
