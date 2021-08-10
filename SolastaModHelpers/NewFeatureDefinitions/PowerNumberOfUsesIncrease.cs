using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    interface IPowerNumberOfUsesIncrease
    {
        void apply(RulesetCharacter character, RulesetUsablePower usable_power);
    }


    public class IncreaseNumberOfPowerUsesPerClassLevel : FeatureDefinition, IPowerNumberOfUsesIncrease
    {
        public List<FeatureDefinitionPower> powers = new List<FeatureDefinitionPower>();
        public CharacterClassDefinition characterClass;
        public List<(int, int)> levelIncreaseList = new List<(int, int)>();

        public void apply(RulesetCharacter character, RulesetUsablePower usable_power)
        {
            var hero = character as RulesetCharacterHero;

            if (hero == null)
            {
                return;
            }

            if (!powers.Contains(usable_power.PowerDefinition))
            {
                return;
            }

            if (!hero.ClassesAndLevels.ContainsKey(characterClass))
            {
                return;
            }

            var lvl = hero.ClassesAndLevels[characterClass];

            var bonus_uses = levelIncreaseList.Aggregate(0, (old, next) =>
                                                        {
                                                            if (next.Item1 <= lvl)
                                                            {
                                                                return old + next.Item2;
                                                            }
                                                            return old;
                                                        }
                                                        );
            usable_power.maxUses += bonus_uses;
            usable_power.Recharge();
        }
    }


    public class IncreaseNumberOfPowerUses : FeatureDefinition, IPowerNumberOfUsesIncrease
    {
        public List<FeatureDefinitionPower> powers = new List<FeatureDefinitionPower>();
        public int value;

        public void apply(RulesetCharacter character, RulesetUsablePower usable_power)
        {
            int bonus_uses = value;

            if (powers.Contains(usable_power.PowerDefinition))
            {
                usable_power.maxUses += bonus_uses;
                usable_power.Recharge();
            }
        }

    }
}
