using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class IgnoreDynamicVisionImpairement: FeatureDefinition
    {
        public float max_range;
        public List<FeatureDefinition> required_features = new List<FeatureDefinition>();
        public List<FeatureDefinition> forbidden_features = new List<FeatureDefinition>();

        public bool canIgnoreDynamicVisionImpairement(RulesetCharacter character, float range)
        {
            if (range > max_range)
            {
                return false;
            }

            foreach (var f in required_features)
            {
                if (!Helpers.Misc.characterHasFeature(character, f))
                {
                    return false;
                }
            }

            foreach (var f in forbidden_features)
            {
                if (Helpers.Misc.characterHasFeature(character, f))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
