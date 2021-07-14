using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{

    public interface IKnownSpellNumberIncrease
    {
        int getKnownSpellsBonus(CharacterBuildingManager manager, RulesetCharacterHero hero, FeatureDefinitionCastSpell castSpellFeature);
        int getKnownCantripsBonus(CharacterBuildingManager manager, RulesetCharacterHero hero, FeatureDefinitionCastSpell castSpellFeature);
    }


    public interface IReplaceSpellList
    {
        SpellListDefinition getSpelllist(ICharacterBuildingService characterBuildingService, bool is_cantrip);
    }


    public class FeatureDefinitionExtraSpellsKnown : FeatureDefinition, IKnownSpellNumberIncrease
    {
        public CharacterClassDefinition caster_class;
        public int max_spells;
        public int max_cantrips = 0;
        public int level;

        public int getKnownCantripsBonus(CharacterBuildingManager manager, RulesetCharacterHero hero, FeatureDefinitionCastSpell castSpellFeature)
        {
            if (!checkValidity(manager, hero, castSpellFeature))
            {
                return 0;
            }

            return max_cantrips;
        }


        bool checkValidity(CharacterBuildingManager manager, RulesetCharacterHero hero, FeatureDefinitionCastSpell castSpellFeature)
        {
            int current_level;
            CharacterClassDefinition current_class;
            manager.GetLastAssignedClassAndLevel(out current_class, out current_level);

            if (caster_class != current_class)
            {
                return false;
            }

            if (hero == null)
            {
                return false;
            }

            if (!hero.ClassesAndLevels.ContainsKey(caster_class))
            {
                return false;
            }

            CharacterClassDefinition class_origin;
            CharacterRaceDefinition race_origin;
            FeatDefinition feat_origin;

            hero.LookForFeatureOrigin(castSpellFeature, out race_origin, out class_origin, out feat_origin);
            if (class_origin != caster_class)
            {
                return false;
            }
            return hero.ClassesAndLevels[caster_class] >= level;
        }

        public int getKnownSpellsBonus(CharacterBuildingManager manager, RulesetCharacterHero hero, FeatureDefinitionCastSpell castSpellFeature)
        {
            if (!checkValidity(manager, hero, castSpellFeature))
            {
                return 0;
            }

            return max_spells;
        }
    }

    public class FeatureDefinitionExtraSpellSelection : FeatureDefinitionExtraSpellsKnown, IReplaceSpellList
    {
        public SpellListDefinition spell_list;
        public bool learnCantrips = false;

        public SpellListDefinition getSpelllist(ICharacterBuildingService characterBuildingService, bool is_cantrip)
        {
            if (is_cantrip && !learnCantrips)
            {
                return null;
            }
            CharacterClassDefinition current_class;
            int current_level;
            characterBuildingService.GetLastAssignedClassAndLevel(out current_class, out current_level);

            int acquired_spells_num = 0;
            int allowed_spells_num = learnCantrips ? max_cantrips : max_spells;
            if (learnCantrips)
            {
                CharacterBuildingManager manager = characterBuildingService as CharacterBuildingManager;
                if (manager == null)
                {
                    return null;
                }
                acquired_spells_num = manager.acquiredCantrips.Aggregate(0, (num, a) => num += a.Value.Count());
            }
            else
            {
                acquired_spells_num = characterBuildingService.AcquiredSpells.Aggregate(0, (num, a) => num += a.Value.Count());
            }
            if (acquired_spells_num >= allowed_spells_num)
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
