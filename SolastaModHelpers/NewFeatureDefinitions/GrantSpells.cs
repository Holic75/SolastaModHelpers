using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IGrantKnownSpellsOnLevelUp
    {
        void maybeGrantSpellsOnLevelUp(CharacterBuildingManager manager);
    }


    public class GrantSpells: FeatureDefinition, IGrantKnownSpellsOnLevelUp
    {
        public List<FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup> spellGroups = new List<FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup>();
        public CharacterClassDefinition spellcastingClass;
        public FeatureDefinitionCastSpell spellcastingFeature;

        public void maybeGrantSpellsOnLevelUp(CharacterBuildingManager manager)
        {
            CharacterClassDefinition current_class;
            int current_level;
            manager.GetLastAssignedClassAndLevel(out current_class, out current_level);

            HashSet<SpellDefinition> spells = new HashSet<SpellDefinition>();
            if (current_class != spellcastingClass)
            {
                return;
            }

            foreach (var sg in spellGroups)
            {
                if (sg.ClassLevel != current_level)
                {
                    continue;
                }

                foreach (var s in sg.SpellsList)
                {
                    spells.Add(s);
                }
            }

            var repertoire = manager.HeroCharacter.SpellRepertoires.FirstOrDefault(r => r.spellCastingClass == current_class
                                                                                   && (r.spellCastingFeature == spellcastingFeature || spellcastingFeature == null));
            if (repertoire == null)
            {
                return;
            }

            foreach (var s in repertoire.KnownSpells)
            {
                if (spells.Contains(s))
                {
                    spells.Remove(s);
                }
            }
            repertoire.KnownSpells.AddRange(spells);
        }
    }
}
