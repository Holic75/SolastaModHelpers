using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class FeatureData
    {
        private static Dictionary<FeatureDefinition, List<NewFeatureDefinitions.IRestriction>> feature_prerequisites = new Dictionary<FeatureDefinition, List<NewFeatureDefinitions.IRestriction>>();

        public static bool isFeatureForbidden(FeatureDefinition feature, RulesetCharacter character)
        {
            if (!feature_prerequisites.ContainsKey(feature))
            {
                return false;
            }

            var restrictions = feature_prerequisites[feature];
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return true;
                }
            }
            return false;
        }

        public static void addFeatureRestrictions(FeatureDefinition feature, params IRestriction[] restrictions)
        {
            if (feature_prerequisites.ContainsKey(feature))
            {
                throw new System.Exception(feature.name + " already has prerequisites");
            }
            feature_prerequisites[feature] = restrictions.ToList();
        }
    }
}
