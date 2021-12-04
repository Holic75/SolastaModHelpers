using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class ActionData
    {
        private static Dictionary<BaseDefinition, List<NewFeatureDefinitions.IRestriction>> action_restirctions = new Dictionary<BaseDefinition, List<NewFeatureDefinitions.IRestriction>>();

        public static bool isActionForbidden(BaseDefinition feature, RulesetCharacter character)
        {
            if (!action_restirctions.ContainsKey(feature))
            {
                return false;
            }

            var prerequisites = action_restirctions[feature];
            foreach (var p in prerequisites)
            {
                if (p.isForbidden(character))
                {
                    return true;
                }
            }
            return false;
        }


        public static List<NewFeatureDefinitions.IRestriction> getActionPrerequisites(BaseDefinition feature)
        {
            if (action_restirctions.ContainsKey(feature))
            {
                return action_restirctions[feature];
            }
            else
            {
                return new List<IRestriction>();
            }
        }

        public static void addActionRestrictions(BaseDefinition feature, params IRestriction[] prerequisites)
        {
            if (action_restirctions.ContainsKey(feature))
            {
                throw new System.Exception(feature.name + " already has prerequisites");
            }
            action_restirctions[feature] = prerequisites.ToList();
        }
    }
}
