using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{

    public interface IKnownSpellNumberIncrease
    {
        int getKnownSpellsBonus(RulesetCharacterHero hero);
    }


    public interface IReplaceSpellList
    {

        SpellListDefinition getSpelllist(ICharacterBuildingService characterBuildingService);
    }


    public class FeatureDefinitionExtraSpellsKnown : FeatureDefinition, IKnownSpellNumberIncrease
    {
        public CharacterClassDefinition caster_class;
        public int max_spells;
        public int level;

        public int getKnownSpellsBonus(RulesetCharacterHero hero)
        {
            if (hero == null)
            {
                return 0;
            }

            if (!hero.ClassesAndLevels.ContainsKey(caster_class))
            {
                return 0;
            }

            if (hero.ClassesAndLevels[caster_class] >= level)
            {
                return max_spells;
            }

            return 0;
        }
    }

    public class FeatureDefinitionExtraSpellSelection : FeatureDefinition, IReplaceSpellList, IKnownSpellNumberIncrease
    {
        public SpellListDefinition spell_list;
        public CharacterClassDefinition caster_class;
        public int max_spells;
        public int level;

        public int getKnownSpellsBonus(RulesetCharacterHero hero)
        {
            if (hero == null)
            {
                return 0;
            }

            if (!hero.ClassesAndLevels.ContainsKey(caster_class))
            {
                return 0;
            }

            if (hero.ClassesAndLevels[caster_class] >= level)
            {
                return max_spells;
            }

            return 0;
        }

        public SpellListDefinition getSpelllist(ICharacterBuildingService characterBuildingService)
        {
            CharacterClassDefinition current_class;
            int current_level;
            characterBuildingService.GetLastAssignedClassAndLevel(out current_class, out current_level);

            int acquired_spells_num = characterBuildingService.AcquiredSpells.Aggregate(0, (num, a) => num += a.Value.Count());
            if (acquired_spells_num >= max_spells)
            {
                return null;
            }
            if (current_class == caster_class && current_level == level)
            {
                return spell_list;
            }
            else
            {
                return null;
            }
        }
    }
}
