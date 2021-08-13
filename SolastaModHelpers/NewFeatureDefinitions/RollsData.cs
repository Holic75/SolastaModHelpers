using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class AttackRollInfo
    {
        public int roll_value;
        public RuleDefinitions.RollOutcome outcome;

        public AttackRollInfo(int attack_roll = - 1, RuleDefinitions.RollOutcome attack_outcome = RuleDefinitions.RollOutcome.Neutral)
        {
            roll_value = attack_roll;
            outcome = attack_outcome;
        }
    }


    public class SavingthrowRollInfo
    {
        public int total_roll_value;
        public int dc_value;
        public RuleDefinitions.RollOutcome outcome;

        public SavingthrowRollInfo(int total_roll = -1, int dc = 0,  RuleDefinitions.RollOutcome save_outcome = RuleDefinitions.RollOutcome.Neutral)
        {
            total_roll_value = total_roll;
            dc_value = dc;
            outcome = save_outcome;
        }
    }


    public class AttackRollsData
    {
        static Dictionary<GameLocationCharacter, AttackRollInfo> prerolled_attacks = new Dictionary<GameLocationCharacter, AttackRollInfo>();

        static public AttackRollInfo getPrerolledData(GameLocationCharacter attacker)
        {
            if (prerolled_attacks.ContainsKey(attacker))
            {
                return prerolled_attacks[attacker];
            }
            else
            {
                return new AttackRollInfo();
            }
        }


        static public void storePrerolledData(GameLocationCharacter attacker, AttackRollInfo value)
        {
            prerolled_attacks[attacker] = value;
        }


        static public void removePrerolledData(GameLocationCharacter attacker)
        {
            prerolled_attacks.Remove(attacker);
        }
    }



    public class SavingthrowRollsData
    {
        static Dictionary<GameLocationCharacter, SavingthrowRollInfo> prerolled_saves = new Dictionary<GameLocationCharacter, SavingthrowRollInfo>();

        static public SavingthrowRollInfo getPrerolledData(GameLocationCharacter roller)
        {
            if (prerolled_saves.ContainsKey(roller))
            {
                return prerolled_saves[roller];
            }
            else
            {
                return new SavingthrowRollInfo();
            }
        }


        static public void storePrerolledData(GameLocationCharacter roller, SavingthrowRollInfo value)
        {
            prerolled_saves[roller] = value;
        }


        static public void removePrerolledData(GameLocationCharacter roller)
        {
            prerolled_saves.Remove(roller);
        }
    }

}
