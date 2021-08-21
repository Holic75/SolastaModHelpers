using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class AbilityCheckAffinityUnderRestriction: FeatureDefinition
    {
        public FeatureDefinitionAbilityCheckAffinity feature;
        public List<IRestriction> restrictions = new List<IRestriction>();

        public bool canBeUsed(RulesetCharacter character)
        {
            if (character == null)
            {
                return false;
            }

            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
