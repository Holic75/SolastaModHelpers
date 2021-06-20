using System;
using System.Collections.Generic;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IModifyDiceRollValue
    {
        int processDiceRoll(RuleDefinitions.RollContext context, int rolled_value, RulesetActor roller);
    }

    public class ModifyDiceRollValue : FeatureDefinition, IModifyDiceRollValue
    {
        public int numDice;
        public RuleDefinitions.DieType diceType;
        public List<RuleDefinitions.RollContext> contexts;

        public int processDiceRoll(RuleDefinitions.RollContext context, int rolled_value, RulesetActor roller)
        {
            if (!contexts.Contains(context))
            {
                return rolled_value;
            }

            int res = rolled_value;
            bool substract = numDice < 0;

            for (int i = 0; i < Math.Abs(numDice); i++)
            {
                int firstRoll;
                int secondRoll;
                int num2 = roller.RollDie(diceType, RuleDefinitions.RollContext.None, RuleDefinitions.AdvantageType.None, out firstRoll, out secondRoll, false, false);
                if (substract)
                {
                    res -= num2;
                }
                else
                {
                    res += num2;
                }
            }

            return Math.Max(0, res);
        }
    }
}
