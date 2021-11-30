using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IIgnoreDamageAffinity
    {
        bool canIgnoreDamageAffinity(IDamageAffinityProvider provider, string damageType);
    }


    public class IgnoreDamageResistance : FeatureDefinition, IIgnoreDamageAffinity
    {
        public List<string> damageTypes = new List<string>();

        public bool canIgnoreDamageAffinity(IDamageAffinityProvider provider, string damageType)
        {
            if (provider.DamageAffinityType != RuleDefinitions.DamageAffinityType.Resistance)
            {
                return false;
            }

            return damageTypes.Contains(damageType);
        }
    }
}
