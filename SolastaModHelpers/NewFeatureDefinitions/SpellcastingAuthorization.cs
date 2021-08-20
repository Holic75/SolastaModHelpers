using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IForbidSpellcasting
    {
        bool isSpellcastingForbidden(RulesetActor character, SpellDefinition spellDefinition);
        bool shouldBreakConcentration(RulesetActor character);
    }


    public interface ISomaticComponentIgnore
    {
        bool canIgnoreSomaticComponent(RulesetCharacter character, SpellDefinition spellDefinition);
    }


    public class AllowToUseWeaponWithFeatureAsSpellFocus : FeatureDefinition, ISomaticComponentIgnore
    {
        public FeatureDefinition weaponFeature;

        public bool canIgnoreSomaticComponent(RulesetCharacter character, SpellDefinition spellDefinition)
        {
            if (spellDefinition.MaterialComponentType != RuleDefinitions.MaterialComponentType.Mundane)
            {
                return false;
            }
            RulesetItem equipedItem1 = character.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;
            RulesetItem equipedItem2 = character.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem;

            return (equipedItem1 != null && Helpers.Misc.itemHasFeature(equipedItem1, weaponFeature))
                    || (equipedItem2 != null && Helpers.Misc.itemHasFeature(equipedItem2, weaponFeature));
        }
    }


    public class SpellcastingForbidden : FeatureDefinition, IForbidSpellcasting
    {
        public List<FeatureDefinition> spellcastingExceptionFeatures = new List<FeatureDefinition>();
        public List<FeatureDefinition> concentrationExceptionFeatures = new List<FeatureDefinition>();
        public List<SpellDefinition> exceptionSpells = new List<SpellDefinition>();
        public bool forbidConcentration = true;

        public bool isSpellcastingForbidden(RulesetActor character, SpellDefinition spellDefinition)
        {
            if (exceptionSpells.Contains(spellDefinition))
            {
                return false;
            }
            return spellcastingExceptionFeatures.Empty() || !Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinition>(character).Any(f => spellcastingExceptionFeatures.Contains(f));
        }

        public bool shouldBreakConcentration(RulesetActor character)
        {
            return forbidConcentration ? concentrationExceptionFeatures.Empty() || !Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinition>(character).Any(f => concentrationExceptionFeatures.Contains(f)) : false;
        }
    }
}
