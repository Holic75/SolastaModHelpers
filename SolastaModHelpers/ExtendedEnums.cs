using System;

namespace SolastaModHelpers.ExtendedEnums
{
    [Flags]
    public enum ExtraTargetFilteringTag : byte
    {
        No = 0,
        Unarmored = 1,
        NonCaster = 128
    }


    public enum ExtraRitualCasting
    {
        None,
        Prepared,
        Spellbook,
        Spontaneous
    }


    public enum ExtraConditionInterruption
    {
        RollsForDamage = 128
    }
}
