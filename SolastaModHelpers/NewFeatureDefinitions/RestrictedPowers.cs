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


    public class NoArmorRestriction : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            return ((character as RulesetCharacter)?.IsWearingArmor()).GetValueOrDefault();
        }
    }


    public class NoShieldRestriction : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            return ((character as RulesetCharacter)?.IsWearingShield()).GetValueOrDefault();
        }
    }


    public class NoRangedWeaponRestriction : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            return ((character as RulesetCharacter)?.IsWieldingRangedWeapon()).GetValueOrDefault();
        }
    }


    public class FreeOffHandRestriciton : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            var ruleset_character = character as RulesetCharacter;
            if (ruleset_character == null)
            {
                return false;
            }

            RulesetInventorySlot rulesetInventorySlot2 = ruleset_character.CharacterInventory.InventorySlotsByType[EquipmentDefinitions.SlotTypeOffHand][0];
            return rulesetInventorySlot2.EquipedItem != null && rulesetInventorySlot2.EquipedItem.ItemDefinition.IsWeapon;
        }
    }


    public class UsedAllMainAttacksRestriction : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            var ruleset_character = character as RulesetCharacter;
            if (ruleset_character == null)
            {
                return false;
            }
           
            return ruleset_character.ExecutedAttacks < ruleset_character.GetAttribute("AttacksNumber", false).CurrentValue;
        }
    }


    public class AttackedRestriction : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            var ruleset_character = character as RulesetCharacter;
            if (ruleset_character == null)
            {
                return false;
            }

            return ruleset_character.ExecutedAttacks == 0;
        }
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


    public class HasFeatureRestriction : IRestriction 
    {
        private FeatureDefinition feature;

        public bool isForbidden(RulesetActor character)
        {
            return !Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinition>(character).Any(f => f == feature);
        }

        public HasFeatureRestriction(FeatureDefinition required_feature)
        {
            feature = required_feature;
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


    public class InverseRestriction : IRestriction
    {
        private IRestriction restriction;

        public bool isForbidden(RulesetActor character)
        {
            return !restriction.isForbidden(character);
        }

        public InverseRestriction(IRestriction base_restriction)
        {
            restriction = base_restriction;
        }
    }

    //holds if at least one of restrictions in the list holds
    public class OrRestriction : IRestriction
    {
        private List<IRestriction> restrictions;

        public bool isForbidden(RulesetActor character)
        {
            return restrictions.Any(r => r.isForbidden(character));
        }

        public OrRestriction(params IRestriction[] or_restriction)
        {
            restrictions = or_restriction.ToList();
        }
    }

    //holds only if all restrictions in the list hold
    public class AndRestriction : IRestriction
    {
        private List<IRestriction> restrictions;

        public bool isForbidden(RulesetActor character)
        {
            return restrictions.All(r => r.isForbidden(character));
        }

        public AndRestriction(params IRestriction[] and_restriciton)
        {
            restrictions = and_restriciton.ToList();
        }
    }


    public interface IPowerRestriction
    {
        bool isForbidden(RulesetActor character);
        bool isReactionForbidden(RulesetActor character);
    }


    public class PowerWithRestrictions : LinkedPower, IPowerRestriction 
    {
        public List<IRestriction> restrictions = new List<IRestriction>();
        public bool checkReaction = false;

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

        public bool isReactionForbidden(RulesetActor character)
        {
            return checkReaction ? isForbidden(character) : false;
        }
    }
}
