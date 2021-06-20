using System.Collections.Generic;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class GrantSpells : FeatureDefinition
    {
        public List<FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup> spellGroups = new List<FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup>();
        public CharacterClassDefinition spellcastingClass;
        public FeatureDefinitionCastSpell spellcastingFeature;
    }
}
