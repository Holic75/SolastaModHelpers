using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IAddExtraAttacks
    {
        void tryAddExtraAttack(RulesetCharacterHero character);
    }


    public class ExtraUnarmedAttack : FeatureDefinition, IAddExtraAttacks
    {        
        public List<IRestriction> restrictions = new List<IRestriction>();

        public bool clearAllAttacks = false;
        public ActionDefinitions.ActionType actionType;

        public List<string> allowedWeaponTypesIfHasRequiredFeature = new List<string>();
        public FeatureDefinition requiredFeature;

        public void tryAddExtraAttack(RulesetCharacterHero character)
        {
            if (clearAllAttacks)
            {
                character.AttackModes.Clear();
            }

            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return;
                }
            }

            RulesetInventorySlot rulesetInventorySlot1 = character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0];
            RulesetInventorySlot rulesetInventorySlot2 = character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeOffHand][0];

            if (rulesetInventorySlot2.EquipedItem != null && rulesetInventorySlot2.EquipedItem.ItemDefinition.IsWeapon)
            {
                //no extra attacks if already have an off-hand weapon
                return;
            }


            if (requiredFeature != null && Helpers.Misc.characterHasFeature(character, requiredFeature))
            {
                if (rulesetInventorySlot1.EquipedItem != null && rulesetInventorySlot1.EquipedItem.ItemDefinition.IsWeapon)
                {
                    RulesetItem equipedItem = rulesetInventorySlot1.EquipedItem;
                    ItemDefinition itemDefinition = rulesetInventorySlot1.EquipedItem.ItemDefinition;
                    WeaponDescription weaponDescription = itemDefinition.WeaponDescription;

                    if (allowedWeaponTypesIfHasRequiredFeature.Contains(weaponDescription.weaponType) || allowedWeaponTypesIfHasRequiredFeature.Empty())
                    {
                        character.AttackModes.Add(character.RefreshAttackMode(actionType, rulesetInventorySlot1.EquipedItem.ItemDefinition,
                                                      rulesetInventorySlot1.EquipedItem.ItemDefinition.WeaponDescription, true, true,
                                                      character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0].Name,
                                                      character.attackModifiers, character.FeaturesOrigin, (RulesetItem)null));
                    }
                }
            }
            else
            {
                ItemDefinition strikeDefinition = character.UnarmedStrikeDefinition;

                character.AttackModes.Add(character.RefreshAttackMode(actionType, strikeDefinition,
                                                                      strikeDefinition.WeaponDescription, false, true,
                                                                      character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0].Name,
                                                                      character.attackModifiers, character.FeaturesOrigin, (RulesetItem)null));
            }
        }
    }


    public class ExtraMainWeaponAttack : FeatureDefinition, IAddExtraAttacks
    {
        public List<IRestriction> restrictions = new List<IRestriction>();

        public ActionDefinitions.ActionType actionType;

        public void tryAddExtraAttack(RulesetCharacterHero character)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return;
                }
            }

            RulesetInventorySlot rulesetInventorySlot1 = character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0];
            RulesetInventorySlot rulesetInventorySlot2 = character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeOffHand][0];
            if (rulesetInventorySlot2.EquipedItem != null && rulesetInventorySlot2.EquipedItem.ItemDefinition.IsWeapon && actionType == ActionDefinitions.ActionType.Bonus)
            {
                //no extra attacks if already have an off-hand weapon
                return;
            }
            if (rulesetInventorySlot1.EquipedItem == null || !rulesetInventorySlot1.EquipedItem.ItemDefinition.IsWeapon)
            {
                return;
            }

            var free_off_hand = rulesetInventorySlot2.EquipedItem == null;

            character.AttackModes.Add(character.RefreshAttackMode(actionType, rulesetInventorySlot1.EquipedItem.ItemDefinition,
                                                                  rulesetInventorySlot1.EquipedItem.ItemDefinition.WeaponDescription, free_off_hand, true,
                                                                  character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0].Name,
                                                                  character.attackModifiers, character.FeaturesOrigin, (RulesetItem)null));
        }
    }


    public class AddExtraMainWeaponAttackModified : FeatureDefinition, IAddExtraAttacks
    {
        public List<IRestriction> restrictions = new List<IRestriction>();

        public ActionDefinitions.ActionType actionType;

        public RuleDefinitions.DieType new_die_type;
        public int new_dice_number = 1;
        public string damage_type = "";

        public void tryAddExtraAttack(RulesetCharacterHero character)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return;
                }
            }

            RulesetInventorySlot rulesetInventorySlot1 = character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0];
            RulesetInventorySlot rulesetInventorySlot2 = character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeOffHand][0];
            if (rulesetInventorySlot2.EquipedItem != null && rulesetInventorySlot2.EquipedItem.ItemDefinition.IsWeapon && actionType == ActionDefinitions.ActionType.Bonus)
            {
                //no extra attacks if already have an off-hand weapon
                return;
            }
            if (rulesetInventorySlot1.EquipedItem == null || !rulesetInventorySlot1.EquipedItem.ItemDefinition.IsWeapon)
            {
                return;
            }

            RulesetAttackMode rulesetAttackMode = character.RefreshAttackMode(actionType,
                                                                              rulesetInventorySlot1.EquipedItem.ItemDefinition,
                                                                              rulesetInventorySlot1.EquipedItem.ItemDefinition.WeaponDescription,
                                                                              false,
                                                                              true,
                                                                              character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0].Name,
                                                                              character.attackModifiers, character.FeaturesOrigin, (RulesetItem)null);

            DamageForm copy = DamageForm.GetCopy(rulesetAttackMode.EffectDescription.FindFirstDamageForm());
            rulesetAttackMode.EffectDescription.Clear();
            EffectForm effectForm = EffectForm.Get();
            copy.DieType = new_die_type;
            copy.versatile = false;
            copy.DiceNumber = 1;
            if (damage_type != "")
            {
                copy.damageType = damage_type;
            }
            effectForm.FormType = EffectForm.EffectFormType.Damage;
            effectForm.DamageForm = copy;
            rulesetAttackMode.EffectDescription.EffectForms.Add(effectForm);
            character.AttackModes.Add(rulesetAttackMode);
        }
    }
}
