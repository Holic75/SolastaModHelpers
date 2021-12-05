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
        SpellListDefinition getSpelllist(ICharacterBuildingService characterBuildingService, bool is_cantrip, int num_spells_from_feature, string spellTag);
    }


    public class FeatureDefinitionExtraSpellsKnownFromFeat : FeatureDefinition, IKnownSpellNumberIncrease
    {
        public int max_spells;
        public int max_cantrips = 0;

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
            CharacterClassDefinition class_origin;
            CharacterRaceDefinition race_origin;
            FeatDefinition feat_origin;
            hero.LookForFeatureOrigin(castSpellFeature, out race_origin, out class_origin, out feat_origin);

            int current_level;
            CharacterClassDefinition current_class;
            manager.GetLastAssignedClassAndLevel(out current_class, out current_level);

            if (class_origin != current_class)
            {
                return false;
            }

            //either we just acquired feature
            var tag = Helpers.Misc.getFeatTagForFeature(manager, this);
            if (tag != "")
            {
                return true;
            }

            //or we already had it
            return hero.FindFirstFeatHoldingFeature(this) != null;          
        }

        public int getKnownSpellsBonus(CharacterBuildingManager manager, RulesetCharacterHero hero, FeatureDefinitionCastSpell castSpellFeature)
        {
            if (!checkValidity(manager, hero, castSpellFeature))
            {
                return 0;
            }

            if (castSpellFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.Spellbook || castSpellFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.WholeList)
            {
                //for wizards we do not need to keep increasing their number of scribed spells after they learn an extra ones
                if (hero.FindFirstFeatHoldingFeature(this) != null)
                {
                    return 0;
                }
            }

            return max_spells;
        }
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

            if (castSpellFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.Spellbook || castSpellFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.WholeList)
            {
                //for wizards we do not need to keep increasing their number of scribed spells after they learn an extra one
                int current_level;
                CharacterClassDefinition current_class;
                manager.GetLastAssignedClassAndLevel(out current_class, out current_level);
                if (current_level > level)
                {
                    return 0;
                }
            }

            return max_spells;
        }
    }

    public class FeatureDefinitionExtraSpellSelection : FeatureDefinitionExtraSpellsKnown, IReplaceSpellList
    {
        public SpellListDefinition spell_list;
        public bool learnCantrips = false;

        public SpellListDefinition getSpelllist(ICharacterBuildingService characterBuildingService, bool is_cantrip, int num_spells_from_feature, string spellTag)
        {
            if (spellTag == "02Race")
            {
                //no extra spells for racial cantrips
                return null;
            }

            if (is_cantrip != learnCantrips)
            {
                return null;
            }

            CharacterClassDefinition current_class;
            int current_level;
            characterBuildingService.GetLastAssignedClassAndLevel(out current_class, out current_level);

            int allowed_spells_num = learnCantrips ? max_cantrips : max_spells;

            if (num_spells_from_feature >= allowed_spells_num)
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


    public class FeatureDefinitionExtraSpellSelectionFromFeat : FeatureDefinitionExtraSpellsKnownFromFeat, IReplaceSpellList
    {
        public SpellListDefinition spell_list;
        public bool learnCantrips = false;

        public SpellListDefinition getSpelllist(ICharacterBuildingService characterBuildingService, bool is_cantrip, int num_spells_from_feature, string spellTag)
        {
            if (spellTag == "02Race")
            {
                //no extra spells for racial cantrips
                return null;
            }

            if (is_cantrip != learnCantrips)
            {
                return null;
            }
            CharacterBuildingManager manager = characterBuildingService as CharacterBuildingManager;
            if (manager == null)
            {
                return null;
            }

            int allowed_spells_num = learnCantrips ? max_cantrips : max_spells;

            if (num_spells_from_feature >= allowed_spells_num)
            {
                return null;
            }

            var tag = Helpers.Misc.getFeatTagForFeature(manager, this);

            if (tag != "")
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
