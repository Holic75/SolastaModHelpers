using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IRestriction
    {
        bool isForbidden(RulesetActor character);
    }


    public class ArmorTypeRestriction : IRestriction
    {
        private ArmorCategoryDefinition armorCategory;
        private bool not; 

        public bool isForbidden(RulesetActor character)
        {
            RulesetItem equipedItem = (character as RulesetCharacterHero)?.characterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeTorso].EquipedItem;
            bool has_armor_type = equipedItem != null
                                  && equipedItem.ItemDefinition.IsArmor
                                  && (DatabaseRepository.GetDatabase<ArmorCategoryDefinition>().GetElement(DatabaseRepository.GetDatabase<ArmorTypeDefinition>().GetElement(equipedItem.ItemDefinition.ArmorDescription.ArmorType, false).ArmorCategory, false)
                                   == armorCategory);
            return has_armor_type == not;
        }

        public ArmorTypeRestriction(ArmorCategoryDefinition armor_category, bool inverted = false)
        {
            armorCategory = armor_category;
            not = inverted;
        }
    }


    public class InBattleRestriction : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            return ServiceRepository.GetService<IGameLocationBattleService>()?.Battle == null;
        }
    }


    public class NoConditionRestriction : IRestriction
    {
        private ConditionDefinition condition;

        public bool isForbidden(RulesetActor character)
        {
            return character.HasConditionOfType(condition.Name);
        }

        public NoConditionRestriction(ConditionDefinition forbidden_condition)
        {
            condition = forbidden_condition;
        }
    }


    public class HasAtLeastOneConditionFromListRestriction : IRestriction
    {
        private List<ConditionDefinition> conditions;

        public bool isForbidden(RulesetActor character)
        {
            foreach (var c in conditions)
            {
                if (character.HasConditionOfType(c.Name))
                {
                    return false;
                }
            }
            return true;
        }

        public HasAtLeastOneConditionFromListRestriction(params ConditionDefinition[] required_conditions)
        {
            conditions = required_conditions.ToList();
        }
    }


    public interface IPowerRestriction
    {
        bool isForbidden(RulesetActor character);
    }


    public class PowerWithRestrictions : LinkedPower, IPowerRestriction 
    {
        public List<IRestriction> restrictions = new List<IRestriction>();

        public bool isForbidden(RulesetActor character)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
