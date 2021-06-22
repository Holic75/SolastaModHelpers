using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IScalingArmorClassBonus
    {
        void apply(RulesetAttribute armor_class_attribute, RulesetCharacter character);
    }


    public class ArmorClassStatBonus : FeatureDefinition, IScalingArmorClassBonus
    {
        public string stat;
        public bool armorAllowed;
        public bool shieldAlowed;
        public List<ConditionDefinition> forbiddenConditions;
        public bool onlyPositive;

        public void apply(RulesetAttribute armor_class_attribute, RulesetCharacter character)
        {
            if (!armorAllowed && character.IsWearingArmor())
            {
                return;
            }

            if (!shieldAlowed && character.IsWearingShield())
            {
                return;
            }

            if (forbiddenConditions != null)
            {
                foreach (var c in forbiddenConditions)
                {
                    if (character.HasConditionOfType(c))
                    {
                        return;
                    }
                }
            }
            var stat_bonus = AttributeDefinitions.ComputeAbilityScoreModifier(character.GetAttribute(stat, false).CurrentValue);
            if (stat_bonus < 0 && onlyPositive)
            {
                return;
            }
            armor_class_attribute.ValueTrends.Add(new RuleDefinitions.TrendInfo(stat_bonus, RuleDefinitions.FeatureSourceType.AbilityScore, stat, character));
            armor_class_attribute.AddModifier(RulesetAttributeModifier.BuildAttributeModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive, (float)stat_bonus, "03Class"));
        }
    }
}
