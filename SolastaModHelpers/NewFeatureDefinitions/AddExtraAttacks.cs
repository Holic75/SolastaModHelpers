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
        public bool allowShield;
        public bool allowArmor;

        public bool clearAllAttacks = false;
        public ActionDefinitions.ActionType actionType;

        public void tryAddExtraAttack(RulesetCharacterHero character)
        {
            if (character.IsWearingArmor() && !allowArmor)
            {
                return;
            }

            if (character.IsWearingShield() && !allowShield)
            {
                return;
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
}
