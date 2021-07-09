using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class SavingthrowAffinityUnderRestriction : FeatureDefinition, ISavingThrowAffinityProvider
    {
        public List<IRestriction> restrictions = new List<IRestriction>();

        public List<FeatureDefinitionSavingThrowAffinity.SavingThrowAffinityGroup> affinityGroups;

        public int IndomitableSavingThrows => 0;

        public string PriorityAbilityScore => throw new NotImplementedException();

        public int ComputePermanentSavingThrowBonus(string abilityType, int sourceAbilityBonus)
        {
            return 0;
        }

        public void ComputeSavingThrowModifier(RulesetActor saver, string abilityType, EffectForm.EffectFormType formType, string schoolOfMagic, string damageType, string conditionType, int sourceAbilityBonus, ActionModifier actionModifier, RuleDefinitions.FeatureOrigin featureOrigin, int contextField)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(saver))
                {
                    return;
                }
            }
            foreach (FeatureDefinitionSavingThrowAffinity.SavingThrowAffinityGroup affinityGroup in this.affinityGroups)
            {
                if (affinityGroup.abilityScoreName == abilityType)
                {
                    if (((affinityGroup.restrictedForms.Count == 0 ? 1 : (affinityGroup.restrictedForms.Contains(formType) ? 1 : 0)) & (affinityGroup.restrictedSchools.Count == 0 ? (true ? 1 : 0) : (affinityGroup.restrictedSchools.Contains(schoolOfMagic) ? 1 : 0))) == 0 || affinityGroup.savingThrowContext != RuleDefinitions.SavingThrowContext.None && (affinityGroup.savingThrowContext & (RuleDefinitions.SavingThrowContext)contextField) == RuleDefinitions.SavingThrowContext.None)
                        break;
                    if (affinityGroup.affinity == RuleDefinitions.CharacterSavingThrowAffinity.Advantage)
                        actionModifier.SavingThrowAdvantageTrends.Add(new RuleDefinitions.TrendInfo(1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
                    else if (affinityGroup.affinity == RuleDefinitions.CharacterSavingThrowAffinity.Disadvantage)
                        actionModifier.SavingThrowAdvantageTrends.Add(new RuleDefinitions.TrendInfo(-1, featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
                    string additionalDetails = string.Empty;
                    if (!string.IsNullOrEmpty(featureOrigin.tags))
                        additionalDetails = Gui.Format("({0})", featureOrigin.tags);
                    if (affinityGroup.savingThrowModifierType == FeatureDefinitionSavingThrowAffinity.ModifierType.AddDice || affinityGroup.savingThrowModifierType == FeatureDefinitionSavingThrowAffinity.ModifierType.RemoveDice)
                    {
                        int num = (affinityGroup.savingThrowModifierType == FeatureDefinitionSavingThrowAffinity.ModifierType.AddDice ? 1 : -1) * RuleDefinitions.RollStaticDiceAndSum(affinityGroup.savingThrowModifierDiceNumber, affinityGroup.savingThrowModifierDieType, (List<int>)null);
                        actionModifier.SavingThrowModifier = actionModifier.GetRawSavingThrowModifier() + num;
                        actionModifier.SavingThrowModifierTrends.Add(new RuleDefinitions.TrendInfo(num, featureOrigin.sourceType, featureOrigin.sourceName, (object)null, additionalDetails));
                        break;
                    }
                    if (affinityGroup.savingThrowModifierType != FeatureDefinitionSavingThrowAffinity.ModifierType.SourceAbility)
                        break;
                    actionModifier.SavingThrowModifier = actionModifier.GetRawSavingThrowModifier() + sourceAbilityBonus;
                    actionModifier.SavingThrowModifierTrends.Add(new RuleDefinitions.TrendInfo(sourceAbilityBonus, featureOrigin.sourceType, featureOrigin.sourceName, (object)null, additionalDetails));
                    break;
                }
            }
        }

        public void ComputeSavingThrowModifier(RulesetActor saver, string abilityType, EffectForm.EffectFormType formType, string schoolOfMagic, string damageType, string conditionType, int sourceAbilityBonus, ActionModifier attackModifier, RuleDefinitions.FeatureOrigin featureOrigin, int contextField, string ancestryDamageType)
        {
            throw new NotImplementedException();
        }
    }
}
