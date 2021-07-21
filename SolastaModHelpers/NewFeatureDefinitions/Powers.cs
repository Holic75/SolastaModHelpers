using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    //power that will additionaly consume resource from another power linked to it,
    //thus number of times it can be used = min(number_of_remaining_power_uses, number_of_remaining_linked_power_uses)
    public class LinkedPower : FeatureDefinitionPower
    {
        public FeatureDefinition linkedPower;

        public RulesetUsablePower getBasePower(RulesetCharacter character)
        {
            if (linkedPower == null)
            {
                return null;
            }
            return character?.usablePowers?.FirstOrDefault(p => p.PowerDefinition == linkedPower);
        }
    }

    public interface IPowerRestriction
    {
        bool isForbidden(RulesetActor character);
        bool isReactionForbidden(RulesetActor character);
    }


    public interface ICustomPowerAbilityScore
    {
        string getPowerAbilityScore(RulesetCharacter character);
    }


    public class HighestAbilityScorePower: FeatureDefinitionPower, ICustomPowerAbilityScore
    {
        public List<string> validAbilityScores = new List<string>();

        public string getPowerAbilityScore(RulesetCharacter character)
        {
            int max_value = 0;
            string max_stat = "";

            foreach (var a in validAbilityScores)
            {
                var val = character.GetAttribute(a).currentValue;
                if (val > max_value)
                {
                    max_value = val;
                    max_stat = a;
                }
            }
            Main.Logger.Log("Found max stat: " + max_stat);
            return max_stat;
        }
    }


    public class PowerWithRestrictions : LinkedPower, IPowerRestriction
    {
        public List<IRestriction> restrictions = new List<IRestriction>();
        public bool checkReaction = false;

        public bool isForbidden(RulesetActor character)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return true;
                }
            }
            return false;
        }

        public bool isReactionForbidden(RulesetActor character)
        {
            return checkReaction ? isForbidden(character) : false;
        }
    }


    public class PowerWithRestrictionsAndCasterLevelDependentEffect : PowerWithRestrictions, ICustomEffectBasedOnCaster
    {
        public List<(int, EffectDescription)> levelEffectList = new List<(int, EffectDescription)>();
        public int minCustomEffectLevel = 100;

        public EffectDescription getCustomEffect(RulesetImplementationDefinitions.ApplyFormsParams formsParams)
        {
            int caster_level = formsParams.classLevel;
            if (caster_level < minCustomEffectLevel)
            {
                return this.effectDescription;
            }

            foreach (var e in levelEffectList)
            {
                if (caster_level <= e.Item1)
                {
                    return e.Item2;
                }
            }

            return this.effectDescription;
        }
    }


    public class HiddenPower: PowerWithRestrictions
    { 
    }

    public class RerollFailedSavePower : HiddenPower
    {
    }
}
