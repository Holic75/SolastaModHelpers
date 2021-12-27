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
            var ruleset_character_hero = character as RulesetCharacterHero;
            if (ruleset_character_hero == null)
            {
                return false;
            }

            RulesetItem equipedItem = ruleset_character_hero.characterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;
            if (equipedItem == null || !equipedItem.ItemDefinition.IsWeapon)
                return false;
            return DatabaseRepository.GetDatabase<WeaponTypeDefinition>().GetElement(equipedItem.ItemDefinition.WeaponDescription.WeaponType).WeaponProximity == RuleDefinitions.AttackProximity.Range;
            //return ((character as RulesetCharacter)?.IsWieldingRangedWeapon()).GetValueOrDefault();
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


    public class UsedCantrip : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            var ruleset_character = character as RulesetCharacter;
            if (ruleset_character == null)
            {
                return true;
            }

            var game_location_character = Helpers.Misc.findGameLocationCharacter(ruleset_character);
            if (game_location_character == null)
            {
                return true;
            }
            return !game_location_character.UsedMainCantrip;
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


    public class SpecificWeaponInMainHandRestriction : IRestriction
    {
        private List<string> weaponTypes = new List<string>();

        public bool isForbidden(RulesetActor character)
        {
            var hero = character as RulesetCharacterHero;
            if (hero == null)
            {
                return true;
            }
            RulesetItem equipedItem = hero?.characterInventory?.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand]?.EquipedItem;
            if (equipedItem?.itemDefinition == null || !equipedItem.ItemDefinition.IsWeapon)
            {
                return true;
            }
            
            return !weaponTypes.Contains(equipedItem.ItemDefinition.weaponDefinition.WeaponType);
        }


        public SpecificWeaponInMainHandRestriction(List<string> weapon_types)
        {
            weaponTypes = weapon_types;
        }
    }


    public class HasWeaponInMainHandWithoutFeature : IRestriction
    {
        FeatureDefinition weaponFeature;
        public bool isForbidden(RulesetActor character)
        {
            var hero = character as RulesetCharacterHero;
            if (hero == null)
            {
                return true;
            }
            RulesetItem equipedItem = hero?.characterInventory?.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand]?.EquipedItem;
            if (equipedItem?.itemDefinition == null || !equipedItem.ItemDefinition.IsWeapon)
            {
                return true;
            }
            return Helpers.Misc.itemHasFeature(equipedItem, weaponFeature);
        }


        public HasWeaponInMainHandWithoutFeature(FeatureDefinition weapon_feature)
        {
            weaponFeature = weapon_feature;
        }
    }


    public class WearingArmorWithoutFeature: IRestriction
    {
        FeatureDefinition armorFeature;
        public bool isForbidden(RulesetActor character)
        {
            var hero = character as RulesetCharacterHero;
            if (hero == null)
            {
                return true;
            }

            if (!hero.IsWearingArmor())
            {
                return true;
            }

            RulesetItem equipedItem = hero?.characterInventory?.InventorySlotsByName[EquipmentDefinitions.SlotTypeTorso]?.EquipedItem;

            return Helpers.Misc.itemHasFeature(equipedItem, armorFeature);
        }


        public WearingArmorWithoutFeature(FeatureDefinition armor_feature)
        {
            armorFeature = armor_feature;
        }
    }


    public class WearingArmorWithFeature : IRestriction
    {
        FeatureDefinition armorFeature;
        public bool isForbidden(RulesetActor character)
        {
            var hero = character as RulesetCharacterHero;
            if (hero == null)
            {
                return true;
            }

            if (!hero.IsWearingArmor())
            {
                return true;
            }

            RulesetItem equipedItem = hero?.characterInventory?.InventorySlotsByName[EquipmentDefinitions.SlotTypeTorso]?.EquipedItem;

            return !Helpers.Misc.itemHasFeature(equipedItem, armorFeature);
        }


        public WearingArmorWithFeature(FeatureDefinition armor_feature)
        {
            armorFeature = armor_feature;
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


    public class DownedAnEnemy : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            var game_location_character = Helpers.Misc.findGameLocationCharacter(character as RulesetCharacter);
            if (game_location_character == null)
            {
                return true;
            }
            return game_location_character.EnemiesDownedByAttack <= 0;
        }
    }


    public class HasConditionRestriction : IRestriction
    {
        private ConditionDefinition condition;

        public bool isForbidden(RulesetActor character)
        {
            return !character.HasConditionOfType(condition.Name);
        }

        public HasConditionRestriction(ConditionDefinition allowed_condition)
        {
            condition = allowed_condition;
        }
    }


    public class MinClassLevelRestriction : IRestriction
    {
        private CharacterClassDefinition character_class;
        private int level;

        public bool isForbidden(RulesetActor character)
        {
            var hero = character as RulesetCharacterHero;
            if (hero == null)
            {
                return true;
            }

            if (!hero.ClassesAndLevels.ContainsKey(character_class))
            {
                return true;
            }

            return hero.ClassesAndLevels[character_class] < level;
        }

        public MinClassLevelRestriction(CharacterClassDefinition required_class, int required_level)
        {
            character_class = required_class;
            level = required_level;
        }
    }


    public class HasFeatureRestriction : IRestriction 
    {
        private FeatureDefinition feature;

        public bool isForbidden(RulesetActor character)
        {
            return !Helpers.Misc.characterHasFeature(character, feature);
        }

        public HasFeatureRestriction(FeatureDefinition required_feature)
        {
            feature = required_feature;
        }
    }


    public class HasAnyFeatureFromListRestriction : IRestriction
    {
        private List<FeatureDefinition> features;

        public bool isForbidden(RulesetActor character)
        {
            foreach (var ff in features)
            {
                if (Helpers.Misc.characterHasFeature(character, ff))
                {
                    return false;
                }
            }
            return true;
        }

        public HasAnyFeatureFromListRestriction(params FeatureDefinition[] required_features)
        {
            features = required_features.ToList();
        }
    }


    public class CanCastSpellRestriction : IRestriction
    {
        private SpellDefinition spell;
        private bool checkSlot;

        public bool isForbidden(RulesetActor character)
        {
            var ruleset_character = character as RulesetCharacter;
            if (ruleset_character == null)
            {
                return true;
            }
            RulesetSpellRepertoire repertoire = null;
            return !ruleset_character.CanCastSpell(spell, checkSlot, out repertoire);
        }

        public CanCastSpellRestriction(SpellDefinition spell_to_check, bool check_slot = true)
        {
            spell = spell_to_check;
            checkSlot = check_slot;
        }
    }



    public class HasAtLeastOneConditionFromListRestriction : IRestriction
    {
        public List<ConditionDefinition> conditions;

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
}
