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
        public List<string> allowedWeaponTypes;
        public List<IRestriction> restrictions = new List<IRestriction>();

        public bool clearAllAttacks = false;
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

            if (rulesetInventorySlot2.EquipedItem != null && rulesetInventorySlot2.EquipedItem.ItemDefinition.IsWeapon)
            {
                //no extra attacks if already have and off-hand weapon
                return;
            }

            if (rulesetInventorySlot1.EquipedItem != null && rulesetInventorySlot1.EquipedItem.ItemDefinition.IsWeapon)
            {
                RulesetItem equipedItem = rulesetInventorySlot1.EquipedItem;
                ItemDefinition itemDefinition = rulesetInventorySlot1.EquipedItem.ItemDefinition;
                WeaponDescription weaponDescription = itemDefinition.WeaponDescription;

                if (!allowedWeaponTypes.Contains(weaponDescription.weaponType))
                {
                    return;
                }
            }

            if (rulesetInventorySlot1.EquipedItem == null && !allowedWeaponTypes.Contains(Helpers.WeaponProficiencies.Unarmed))
            {
                return;
            }

            ItemDefinition strikeDefinition = character.UnarmedStrikeDefinition;
            if (clearAllAttacks)
            {
                character.AttackModes.Clear();
            }
            character.AttackModes.Add(character.RefreshAttackMode(actionType, strikeDefinition,
                                                                  strikeDefinition.WeaponDescription, false, true,
                                                                  character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0].Name,
                                                                  character.attackModifiers, character.FeaturesOrigin, (RulesetItem)null));
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
            if (rulesetInventorySlot2.EquipedItem != null && rulesetInventorySlot2.EquipedItem.ItemDefinition.IsWeapon)
            {
                //no extra attacks if already have and off-hand weapon
                return;
            }
            if (rulesetInventorySlot1.EquipedItem == null || !rulesetInventorySlot1.EquipedItem.ItemDefinition.IsWeapon)
            {
                return;
            }
            character.AttackModes.Add(character.RefreshAttackMode(actionType, rulesetInventorySlot1.EquipedItem.ItemDefinition,
                                                                  rulesetInventorySlot1.EquipedItem.ItemDefinition.WeaponDescription, false, true,
                                                                  character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0].Name,
                                                                  character.attackModifiers, character.FeaturesOrigin, (RulesetItem)null));
        }
    }
}
