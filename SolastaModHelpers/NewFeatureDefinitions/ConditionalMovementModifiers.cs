using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IConditionalMovementModifier
    {

        void tryAddConditionalMovementModfiers(RulesetCharacter character, List<FeatureDefinition> existingModifiers);
    }


    public class MovementBonusBasedOnEquipment: FeatureDefinition, IConditionalMovementModifier
    {
        public bool allowArmor;
        public bool allowShield;

        public List<FeatureDefinition> modifiers;
        public CharacterAction characterClass;

        public void tryAddConditionalMovementModfiers(RulesetCharacter character, List<FeatureDefinition> existingModifiers)
        {
            if (character.IsWearingArmor() && !allowArmor)
            {
                return;
            }

            if (character.IsWearingShield() && !allowShield)
            {
                return;
            }

            foreach (var m in modifiers)
            {
                existingModifiers.Add(m);
            }
        }
    }
}
