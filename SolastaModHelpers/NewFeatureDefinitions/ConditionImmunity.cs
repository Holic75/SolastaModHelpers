using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    interface IConditionImmunity
    {
        bool isImmune(RulesetActor actor, ConditionDefinition condition);
    }


    public class ImmunityToCondtionIfHasSpecificConditions : FeatureDefinition, IConditionImmunity
    {
        public List<ConditionDefinition> immuneCondtions;
        public List<ConditionDefinition> requiredConditions = new List<ConditionDefinition>();

        public bool isImmune(RulesetActor actor, ConditionDefinition condition)
        {
            bool is_ok = requiredConditions.Empty();

            foreach (var c in requiredConditions)
            {
                if (actor.HasConditionOfType(c))
                {
                    is_ok = true;
                    break;
                }
            }

            if (!is_ok)
            {
                return false ;
            }

            return immuneCondtions.Contains(condition);
        }
    }
}
