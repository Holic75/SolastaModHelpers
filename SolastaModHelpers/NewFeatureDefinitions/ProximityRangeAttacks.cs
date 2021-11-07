using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IIgnoreRangeProximityPenalty
    {
        bool canIgnoreRangeProximityPenalty(RulesetCharacter character, BattleDefinitions.AttackEvaluationParams attack_params);
    }


    public class IgnorePhysicalRangeProximityPenaltyWithWeaponCategory : FeatureDefinition, IIgnoreRangeProximityPenalty
    {
        public List<string> weaponCategories = new List<string>();
        public bool only_for_close_range_attacks = false;

        public bool canIgnoreRangeProximityPenalty(RulesetCharacter character, BattleDefinitions.AttackEvaluationParams attack_params)
        {
            if (attack_params.attackProximity != BattleDefinitions.AttackProximity.PhysicalRange)
            {
                return false;
            }

            if (only_for_close_range_attacks)
            {
                var defender = attack_params.defender;
                var attacker = attack_params.attacker;
                var battle_service = ServiceRepository.GetService<IGameLocationBattleService>();
                if (attacker == null || defender == null || battle_service == null)
                {
                    return false;
                }
                if (!ServiceRepository.GetService<IGameLocationBattleService>().IsWithinXCells(attacker, defender, 1))
                {
                    return false;
                }
            }
            //(attack_params.attackMode?.sourceDefinition as ItemDefinition) ?
            RulesetInventorySlot rulesetInventorySlot1 = character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeMainHand][0];
            if (rulesetInventorySlot1.EquipedItem != null && rulesetInventorySlot1.EquipedItem.ItemDefinition.IsWeapon)
            {
                RulesetItem equipedItem = rulesetInventorySlot1.EquipedItem;
                ItemDefinition itemDefinition = rulesetInventorySlot1.EquipedItem.ItemDefinition;
                WeaponDescription weaponDescription = itemDefinition.WeaponDescription;

                if (weaponCategories.Contains(weaponDescription.weaponType))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
