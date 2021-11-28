using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class FeatureData
    {
        private static Dictionary<BaseDefinition, List<NewFeatureDefinitions.IPrerequisite>> feature_prerequisites = new Dictionary<BaseDefinition, List<NewFeatureDefinitions.IPrerequisite>>();

        public static bool isFeatureForbidden(BaseDefinition feature, RulesetCharacter character)
        {
            if (!feature_prerequisites.ContainsKey(feature))
            {
                return false;
            }

            var prerequisites = feature_prerequisites[feature];
            foreach (var p in prerequisites)
            {
                if (p.isForbidden(character))
                {
                    return true;
                }
            }
            return false;
        }


        public static List<NewFeatureDefinitions.IPrerequisite> getFeaturePrerequisites(BaseDefinition feature)
        {
            if (feature_prerequisites.ContainsKey(feature))
            {
                return feature_prerequisites[feature];
            }
            else
            {
                return new List<IPrerequisite>();
            }
        }

        public static void addFeatureRestrictions(BaseDefinition feature, params IPrerequisite[] prerequisites)
        {
            if (feature_prerequisites.ContainsKey(feature))
            {
                throw new System.Exception(feature.name + " already has prerequisites");
            }
            feature_prerequisites[feature] = prerequisites.ToList();
        }
    }
}
