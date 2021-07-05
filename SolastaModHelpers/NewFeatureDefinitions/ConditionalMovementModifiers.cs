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


    public class MovementBonusWithRestrictions: FeatureDefinition, IConditionalMovementModifier
    {
        public List<IRestriction> restrictions = new List<IRestriction>();

        public List<FeatureDefinition> modifiers;
        public CharacterAction characterClass;

        public void tryAddConditionalMovementModfiers(RulesetCharacter character, List<FeatureDefinition> existingModifiers)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return;
                }
            }

            foreach (var m in modifiers)
            {
                existingModifiers.Add(m);
            }
        }
    }
}
