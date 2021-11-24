using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IIgnoreAooImmunity
    {
        bool canIgnore(RulesetCharacter attacker, RulesetCharacter defender);
    }

    public class CanAlwaysMakeAoo: FeatureDefinition, IIgnoreAooImmunity
    {
        public bool canIgnore(RulesetCharacter attacker, RulesetCharacter defender)
        {
            return true;
        }
    }



    public class AooIfAllyIsAttacked: FeatureDefinition
    {

    }


}
