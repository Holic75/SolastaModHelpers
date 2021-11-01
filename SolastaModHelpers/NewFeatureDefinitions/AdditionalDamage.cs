using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class MonsterAdditionalDamage: FeatureDefinition
    {
        public MonsterAdditionalDamageProxy provider;
    }

    public class MonsterAdditionalDamageProxy : FeatureDefinition, IAdditionalDamageProvider
    {
        public string notificationTag;
        public RuleDefinitions.FeatureLimitedUsage limitedUsage;
        public RuleDefinitions.AdditionalDamageTriggerCondition triggerCondition;
        public ConditionDefinition requiredTargetCondition;
        public string requiredTargetCreatureTag;
        public RuleDefinitions.AdditionalDamageValueDetermination damageValueDetermination;
        public RuleDefinitions.DieType damageDieType = RuleDefinitions.DieType.D6;
        public int damageDiceNumber;
        public RuleDefinitions.AdditionalDamageType additionalDamageType;
        public string specificDamageType = "Radiant";
        public RuleDefinitions.AdditionalDamageAdvancement damageAdvancement;
        public List<DiceByRank> diceByRankTable = new List<DiceByRank>();
        public List<string> familiesWithAdditionalDice = new List<string>();
        public int familiesDiceNumber = 1;
        public List<ConditionOperationDescription> conditionOperations = new List<ConditionOperationDescription>();
        public GameObject impactParticle;
        public FeatureDefinitionCastSpell spellcastingFeature;
        public List<IRestriction> restricitons = new List<IRestriction>();

        public string NotificationTag => this.notificationTag;

        public RuleDefinitions.FeatureLimitedUsage LimitedUsage => this.limitedUsage;

        public RuleDefinitions.AdditionalDamageTriggerCondition TriggerCondition => this.triggerCondition;

        public RuleDefinitions.AdditionalDamageRequiredProperty RequiredProperty => RuleDefinitions.AdditionalDamageRequiredProperty.None;

        public ConditionDefinition RequiredTargetCondition => this.requiredTargetCondition;

        public SenseMode.Type RequiredTargetSenseType => SenseMode.Type.None;

        public string RequiredTargetCreatureTag => this.requiredTargetCreatureTag;

        public CharacterFamilyDefinition RequiredCharacterFamily => null;

        public RuleDefinitions.AdditionalDamageValueDetermination DamageValueDetermination => this.damageValueDetermination;

        public RuleDefinitions.DieType DamageDieType => this.damageDieType;

        public int DamageDiceNumber => this.damageDiceNumber;

        public RuleDefinitions.AdditionalDamageType AdditionalDamageType => this.additionalDamageType;

        public string SpecificDamageType => this.specificDamageType;

        public RuleDefinitions.AdditionalDamageAdvancement DamageAdvancement => this.damageAdvancement;

        public List<DiceByRank> DiceByRankTable => this.diceByRankTable;

        public List<string> FamiliesWithAdditionalDice => this.familiesWithAdditionalDice;

        public int FamiliesDiceNumber => this.familiesDiceNumber;

        public bool HasSavingThrow => false;

        public string SavingThrowAbility => "Wisdom";

        public int SavingThrowDC => 10;

        public RuleDefinitions.EffectSavingThrowType DamageSaveAffinity => RuleDefinitions.EffectSavingThrowType.None;

        public List<ConditionOperationDescription> ConditionOperations => this.conditionOperations;

        public bool AddLightSource => false;

        public LightSourceForm LightSourceForm => null;

        public GameObject ImpactParticle => this.impactParticle;

        public bool AttackModeOnly => false;

        public bool IgnoreCriticalDoubleDice => false;

        public int GetDiceOfRank(int rank)
        {
            for (int index = 0; index < this.DiceByRankTable.Count; ++index)
            {
                if (this.DiceByRankTable[index].Rank == rank)
                    return this.DiceByRankTable[index].DiceNumber;
            }
            Trace.LogWarning(string.Format("Feature definition lacks an entry in its DiceByRankTable for rank={0} : {1}", (object)rank, (object)this.Name));
            return 0;
        }
    }
}
