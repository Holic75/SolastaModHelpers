using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    //power that will use resource from another power
    public class LinkedPower : FeatureDefinitionPower
    {
        public FeatureDefinition linkedPower;

        public RulesetUsablePower getBasePower(RulesetCharacter character)
        {
            if (linkedPower == null)
            {
                return null;
            }
            return character?.usablePowers?.FirstOrDefault(p => p.PowerDefinition == linkedPower);
        }
    }
}
