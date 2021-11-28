using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IPrerequisite
    {
        bool isForbidden(RulesetActor character);
        string getDescription();
    }


    public class CanCastSpellPrerequisite : IPrerequisite
    {
        private SpellDefinition spell;

        public bool isForbidden(RulesetActor character)
        {
            var ruleset_character = character as RulesetCharacter;
            if (ruleset_character == null)
            {
                return true;
            }
            RulesetSpellRepertoire repertoire = null;
            return !ruleset_character.CanCastSpell(spell, false, out repertoire);
        }

        public string getDescription()
        {
            return String.Format(Gui.Localize("Tooltip/&CustomPrerequisiteAbilityToCastSpellTitle"), Gui.Localize(spell.guiPresentation.title));
        }

        public CanCastSpellPrerequisite(SpellDefinition spell_to_check)
        {
            spell = spell_to_check;
        }
    }


    public class HasFeaturePrerequisite : IPrerequisite
    {
        private FeatureDefinition feature;

        public bool isForbidden(RulesetActor character)
        {
            return !Helpers.Misc.characterHasFeature(character, feature);
        }

        public HasFeaturePrerequisite(FeatureDefinition required_feature)
        {
            feature = required_feature;
        }

        public string getDescription()
        {
            return String.Format(Gui.Localize("Tooltip/&CustomPrerequisiteHasFeatureTitle"), Gui.Localize(feature.guiPresentation.title));
        }
    }


    public class HasAnyFeatureFromListPrerequisite : IPrerequisite
    {
        private List<FeatureDefinition> features;

        public bool isForbidden(RulesetActor character)
        {
            foreach (var ff in features)
            {
                if (Helpers.Misc.characterHasFeature(character, ff))
                {
                    return false;
                }
            }
            return true;
        }

        public HasAnyFeatureFromListPrerequisite(params FeatureDefinition[] required_features)
        {
            features = required_features.ToList();
        }


        public string getDescription()
        {
            string features_string = "";

            for (int i = 0; i < features.Count; i++)
            {
                if (i > 0)
                {
                    features_string += ", ";
                }
                features_string += Gui.Localize(features[i].guiPresentation.title);
            }

            return String.Format(Gui.Localize("Tooltip/&CustomPrerequisiteHasFeaturesFromListTitle"), features_string);
        }
    }


    public class MinClassLevelPrerequisite : IPrerequisite
    {
        private CharacterClassDefinition character_class;
        private int level;

        public bool isForbidden(RulesetActor character)
        {
            var hero = character as RulesetCharacterHero;
            if (hero == null)
            {
                return true;
            }

            if (!hero.ClassesAndLevels.ContainsKey(character_class))
            {
                return true;
            }

            return hero.ClassesAndLevels[character_class] < level;
        }

        public MinClassLevelPrerequisite(CharacterClassDefinition required_class, int required_level)
        {
            character_class = required_class;
            level = required_level;
        }


        public string getDescription()
        {
            return String.Format(Gui.Localize("Tooltip/&CustomPrerequisiteMinClassLevelListTitle"), Gui.Localize(character_class.guiPresentation.title), level.ToString());
        }
    }



    public class CanCastSpellOfSpecifiedLevelPrerequisite : IPrerequisite
    {
        private int spell_level;

        public bool isForbidden(RulesetActor character)
        {
            var ruleset_character = character as RulesetCharacter;
            if (ruleset_character == null)
            {
                return true;
            }

            if (spell_level == 1)
            {
                return !Helpers.Accessors.extractFeaturesHierarchically<FeatureDefinitionCastSpell>(ruleset_character).Any(f => f.spellCastingOrigin != FeatureDefinitionCastSpell.CastingOrigin.Race);
            }

            return !ruleset_character.SpellRepertoires.Any(sr => sr.MaxSpellLevelOfSpellCastingLevel >= spell_level);
        }

        public string getDescription()
        {
            return String.Format(Gui.Localize("Tooltip/&CustomPrerequisiteAbilityToCastSpellOfLevelTitle"), spell_level.ToString());
        }

        public CanCastSpellOfSpecifiedLevelPrerequisite(int level)
        {
            spell_level = level;
        }
    }
}
