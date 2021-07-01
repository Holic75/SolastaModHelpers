using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class DeflectMissileCustom: FeatureDefinition
    {
        public CharacterClassDefinition characterClass;
        public string characterStat;

        public int getDeflectMissileBonus(RulesetCharacter character)
        {
            var bonus = 0;
            bonus += AttributeDefinitions.ComputeAbilityScoreModifier(character.GetAttribute(characterStat).CurrentValue);

            var hero = character as RulesetCharacterHero;
            if (hero == null || !hero.classesAndLevels.ContainsKey(characterClass))
            {
                return bonus;
            }

            bonus += hero.classesAndLevels[characterClass];
            return bonus;
        }
    }
}
