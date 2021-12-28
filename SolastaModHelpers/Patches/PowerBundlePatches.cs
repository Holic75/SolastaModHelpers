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

                //add info about remaining spell slots if powers consume them
                var usable_power = caster.GetPowerFromDefinition(power);
                if (usable_power != null && power.rechargeRate == RuleDefinitions.RechargeRate.SpellSlot)
                {
                    var power_info = Helpers.Accessors.getNumberOfSpellsFromRepertoireOfSpecificSlotLevelAndFeature(power.costPerUse, caster, power.spellcastingFeature);
                    __instance.spellTitle.Text += $"   [{power_info.remains}/{power_info.total}]";
                }
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



        
        [HarmonyPatch(typeof(CharacterReactionSubitem), "Bind")]
        internal static class CharacterReactionSubitem_Bind
        {
            internal const int SLOT_LEVEL_FOR_POWER_BUNDLE = -37;
            internal const int SLOT_LEVEL_FOR_WARCSTER = -38;
            internal static bool Prefix(CharacterReactionSubitem __instance, RulesetSpellRepertoire spellRepertoire,
                                        int slotLevel,
                                        string text,
                                        bool interactable,
                                        CharacterReactionSubitem.SubitemSelectedHandler subitemSelected
                                        )
            {
                __instance.toggle.transform.localScale = new Vector3(1, 1, 1);
                __instance.label.transform.localScale = new Vector3(1, 1, 1);
                int SLOTS_FOR_TOOLTIP = 5;
                string title = "";
                string description = "";

                switch (slotLevel)
                {
                    case SLOT_LEVEL_FOR_POWER_BUNDLE:
                        {
                            var subpower = DatabaseRepository.GetDatabase<FeatureDefinitionPower>().GetElement(text);
                            title = subpower.guiPresentation.title;
                            description = subpower.guiPresentation.description;
                            break;
                        }
                    case SLOT_LEVEL_FOR_WARCSTER:
                        {
                            if (text == "OpportunityAttack")
                            {
                                title = "Reaction/&WarcasterAttackTitle";
                                description = "Reaction/&WarcasterAttackDescription";
                            }
                            else
                            {
                                var spell = DatabaseRepository.GetDatabase<SpellDefinition>().GetElement(text);
                                title = spell.guiPresentation.title;
                                description = spell.guiPresentation.description;
                            }
                            break;
                        }
                    default:
                        return true;
                }
       
                __instance.label.Text = Gui.Localize(title);
                __instance.toggle.interactable = interactable;
                __instance.canvasGroup.interactable = interactable;
                __instance.SubitemSelected = subitemSelected;

                while (__instance.slotStatusTable.childCount < SLOTS_FOR_TOOLTIP)
                    Gui.GetPrefabFromPool(__instance.slotStatusPrefab, (Transform)__instance.slotStatusTable);
                for (int index = 0; index < __instance.slotStatusTable.childCount; ++index)
                {
                    var child = __instance.slotStatusTable.GetChild(index);
                    child.gameObject.SetActive(true);
                    SlotStatus component = child.GetComponent<SlotStatus>();
                    component.Used.gameObject.SetActive(false);
                    component.Available.gameObject.SetActive(false);
                }
                for (int index = SLOTS_FOR_TOOLTIP; index < __instance.slotStatusTable.childCount; ++index)
                {
                    __instance.slotStatusTable.GetChild(index).gameObject.SetActive(false);
                }
                __instance.slotStatusTable.GetComponent<GuiTooltip>().Content = description;


                float scale = 5.0f;
                var old_scale = __instance.toggle.transform.localScale;
                old_scale.x *= scale;
                __instance.toggle.transform.localScale = old_scale;
                old_scale = __instance.label.transform.localScale;
                old_scale.x /= scale;
                __instance.label.transform.localScale = old_scale;
                return false;
            }
        }


        [HarmonyPatch(typeof(CharacterReactionItem), "Bind")]
        internal static class CharacterReactionItem_Bind
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var load_spell_repertoire = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ldloc_0);
                //next ldloc.3 -> load index
                //     ldc.i4.1 -> load 1
                //     add      -> add
                codes[load_spell_repertoire + 2] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1); //load reactionRequest
                codes[load_spell_repertoire + 3] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                                  new Func<int, ReactionRequest, int>(getSuboptionIndex).Method
                                                                                 );

                return codes.AsEnumerable();
            }

            static int getSuboptionIndex(int index, ReactionRequest reactionRequest)
            {
                var power_bundle = reactionRequest.reactionParams?.UsablePower?.PowerDefinition as NewFeatureDefinitions.IPowerBundle;
                if (power_bundle != null)
                {
                    return CharacterReactionSubitem_Bind.SLOT_LEVEL_FOR_POWER_BUNDLE;
                }

                if (reactionRequest.reactionParams.StringParameter2 == "Warcaster")
                {
                    return CharacterReactionSubitem_Bind.SLOT_LEVEL_FOR_WARCSTER;
                }
                return index + 1;
            }
        }


        [HarmonyPatch(typeof(GameLocationActionManager), "ReactToSpendPower")]
        internal static class GameLocationActionManager_ReactToSpendPower
        {
            internal static bool Prefix(GameLocationActionManager __instance, CharacterActionParams reactionParams)
            {
                var usable_power = (reactionParams?.RulesetEffect as RulesetEffectPower)?.usablePower;
                var power_bundle = usable_power?.powerDefinition as NewFeatureDefinitions.IPowerBundle;
                if (power_bundle == null)
                {
                    return true;
                }

                reactionParams.usablePower = usable_power;
                __instance.AddInterruptRequest((ReactionRequest)new ReactionRequestSpendPowerFromBundle(reactionParams));
                return false;
            }            
        }
    }
}
