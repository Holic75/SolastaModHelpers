using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.ExtendedEnums
{
    [Flags]
    public enum ExtraTargetFilteringTag : byte
    {
        No = 0,
        Unarmored = 1,
        MetalArmor = 32,
        NoHeavyArmor = 64,
        NonCaster = 128
    }


    public enum ExtraRitualCasting
    {
        None = RuleDefinitions.RitualCasting.None,
        Prepared = RuleDefinitions.RitualCasting.Prepared,
        Spellbook = RuleDefinitions.RitualCasting.Spellbook,
        Spontaneous = 10
    }


    public enum ExtraConditionInterruption
    {
        RollsForDamage = 128
    }

    public enum AdditionalDamageTriggerCondition
    {
        CantripDamage = 126,
        MagicalAttacksOnTargetWithConditionFromMe = 127,
        RadiantOrFireSpellDamage = 128,      
    }
}
