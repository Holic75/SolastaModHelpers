using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IForbidSpellcasting
    {
        bool isSpellcastingForbidden(RulesetActor character, SpellDefinition spellDefinition);
        bool shouldBreakConcentration(RulesetActor character);
    }


    public class SpellcastingForbidden : FeatureDefinition, IForbidSpellcasting
    {
        public List<FeatureDefinition> spellcastingExceptionFeatures = new List<FeatureDefinition>();
        public List<FeatureDefinition> concentrationExceptionFeatures = new List<FeatureDefinition>();
        public bool forbidConcentration = true;

        public bool isSpellcastingForbidden(RulesetActor character, SpellDefinition spellDefinition)
        {
            return !Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinition>(character).Any(f => spellcastingExceptionFeatures.Contains(f));
        }

        public bool shouldBreakConcentration(RulesetActor character)
        {
            return forbidConcentration ? !Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinition>(character).Any(f => concentrationExceptionFeatures.Contains(f)) : false;
        }
    }
}
