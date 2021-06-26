using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IForbidSpellcasting
    {
        bool isSpellcastingForbidden(RulesetCharacter character);
    }


    public class SpellcastingForbidden : FeatureDefinition, IForbidSpellcasting
    {
        public List<FeatureDefinition> exceptionFeatures = new List<FeatureDefinition>();

        public bool isSpellcastingForbidden(RulesetCharacter character)
        {
            var hero = character as RulesetCharacterHero;
            if (hero == null)
            {
                return false;
            }

            return !Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinition>(hero).Any(f => exceptionFeatures.Contains(f));
        }
    }
}
