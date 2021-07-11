using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    //power that will additionaly consume resource from another power linked to it,
    //thus number of times it can be used = min(number_of_remaining_power_uses, number_of_remaining_linked_power_uses)
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
