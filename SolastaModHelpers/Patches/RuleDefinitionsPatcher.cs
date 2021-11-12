using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    //check for spelleffect that grant weapon enchants when checking for buffs
    //To prevent termination of these kind of effects on map change (otherwise they will be removed - is it intentional ???)
    [HarmonyPatch(typeof(RuleDefinitions), "MatchesMagicType")]
    internal static class AssetReference_RuntimeKeyIsValid
    {
        internal static bool Prefix(EffectDescription effectDescription, RuleDefinitions.MagicType validType, ref bool __result)
        {
            if ((validType & RuleDefinitions.MagicType.Buff) != 0
                && (effectDescription.targetType == RuleDefinitions.TargetType.Item 
                    || effectDescription.itemSelectionType == ActionDefinitions.ItemSelectionType.Weapon
                    || effectDescription.itemSelectionType == ActionDefinitions.ItemSelectionType.WeaponNonMagical
                    || effectDescription.itemSelectionType == ActionDefinitions.ItemSelectionType.Equiped)
               )
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
