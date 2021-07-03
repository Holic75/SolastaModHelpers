using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IScalingArmorClassBonus
    {
        void apply(RulesetAttribute armor_class_attribute, RulesetCharacter character, int precomputedBonus);
        bool isExclusive();
        int precomputeBonusValue(RulesetCharacter character);
    }


    public class ArmorClassStatBonus : FeatureDefinition, IScalingArmorClassBonus
    {
        public string stat;
        public bool armorAllowed;
        public bool shieldAlowed;
        public List<ConditionDefinition> forbiddenConditions;
        public bool onlyPositive;
        public bool exclusive = false;

        public void apply(RulesetAttribute armor_class_attribute, RulesetCharacter character, int precomputedBonus)
        {
            armor_class_attribute.ValueTrends.Add(new RuleDefinitions.TrendInfo(precomputedBonus, RuleDefinitions.FeatureSourceType.AbilityScore, stat, character));
            armor_class_attribute.AddModifier(RulesetAttributeModifier.BuildAttributeModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive, (float)precomputedBonus, "03Class"));
        }

        public bool isExclusive()
        {
            return exclusive;
        }

        public int precomputeBonusValue(RulesetCharacter character)
        {
            if (!armorAllowed && character.IsWearingArmor())
            {
                return 0;
            }

            if (!shieldAlowed && character.IsWearingShield())
            {
                return 0;
            }

            if (forbiddenConditions != null)
            {
                foreach (var c in forbiddenConditions)
                {
                    if (character.HasConditionOfType(c))
                    {
                        return 0;
                    }
                }
            }
            var stat_bonus = AttributeDefinitions.ComputeAbilityScoreModifier(character.GetAttribute(stat, false).CurrentValue);
            if (stat_bonus < 0 && onlyPositive)
            {
                return 0 ;
            }
            return stat_bonus;
        }
    }
}
