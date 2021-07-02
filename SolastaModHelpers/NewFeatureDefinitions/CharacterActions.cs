using SolastaModHelpers.NewFeatureDefinitions;
using SolastaModApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public enum ExtendedActionId
    {
        ModifyAttackRollViaPower = 128,
        DeflectMissileCustom = 129
    }
}


public class CharacterActionModifyAttackRollViaPower : CharacterActionUsePower
{
    static public ActionDefinition modifyAttackRollViaPowerActionDefinition;
    static public ReactionDefinition modifyAttackRollViaPowerReactionDefinition;

    static public void initialize()
    {
        modifyAttackRollViaPowerActionDefinition = SolastaModHelpers.Helpers.CopyFeatureBuilder<ActionDefinition>
            .createFeatureCopy("ModifyAttackRollViaPowerReaction", "2e980398-f18a-4c9a-a769-92d6603cea4d", "", "", null, DatabaseHelper.ActionDefinitions.PowerReaction);
        modifyAttackRollViaPowerActionDefinition.id = (ActionDefinitions.Id)ExtendedActionId.ModifyAttackRollViaPower;
        modifyAttackRollViaPowerActionDefinition.classNameOverride = "ModifyAttackRollViaPower";

        modifyAttackRollViaPowerReactionDefinition = SolastaModHelpers.Helpers.CopyFeatureBuilder<ReactionDefinition>
                    .createFeatureCopy("ModifyAttackRollViaPower", "0a5380a6-e3a8-4e59-8431-52c70f5073b7", "", "", null, DatabaseHelper.ReactionDefinitions.UsePower);
    }

    public CharacterActionModifyAttackRollViaPower(CharacterActionParams actionParams)
        : base(actionParams)
    {
    }

    public override IEnumerator ExecuteImpl()
    {
        CharacterActionModifyAttackRollViaPower actionModifyAttackRoll = this;
        GameLocationCharacter attacker = actionModifyAttackRoll.ActionParams.TargetCharacters[0];
        GameLocationCharacter defender = actionModifyAttackRoll.ActionParams.TargetCharacters[1];
        var action_modifier = actionModifyAttackRoll.ActionParams.ActionModifiers[0];
        var power = actionParams.UsablePower.PowerDefinition;
        foreach (var e in power.EffectDescription.effectForms)
        {
            var condition = e.ConditionForm?.conditionDefinition;
            if (condition == null)
            {
                continue;
            }

            foreach (var f in condition.Features.OfType<FeatureDefinitionCombatAffinity>())
            {
                f.ComputeAttackModifier(attacker.RulesetCharacter, defender.RulesetCharacter, actionModifyAttackRoll.actionParams.AttackMode,
                                        actionModifyAttackRoll.ActionParams.ActionModifiers[0],
                                        new RuleDefinitions.FeatureOrigin(RuleDefinitions.FeatureSourceType.Condition, condition.Name, condition, "")
                                        );
            }
        }
        actionModifyAttackRoll.ActionParams.TargetCharacters.RemoveAt(1); //remove defender to avoid it being affected
        var res = base.ExecuteImpl();
        while (res.MoveNext())
        {
            yield return res.Current;
        }
    }
}



public class CharacterActionDeflectMissileCustom : CharacterAction
{
    static public ActionDefinition monkDeflectMissilesActionDefinition;
    static public ReactionDefinition monkDeflectMissilesPowerReactionDefinition;


    static public void initialize()
    {
        monkDeflectMissilesActionDefinition = SolastaModHelpers.Helpers.CopyFeatureBuilder<ActionDefinition>
            .createFeatureCopy("DeflectMissileCustomAction", "0309ed2c-39ea-4e23-89cf-42db212448ee", "", "", null, DatabaseHelper.ActionDefinitions.DeflectMissile);
        monkDeflectMissilesActionDefinition.id = (ActionDefinitions.Id)ExtendedActionId.DeflectMissileCustom;
        monkDeflectMissilesActionDefinition.classNameOverride = "DeflectMissileCustom";

        monkDeflectMissilesPowerReactionDefinition = SolastaModHelpers.Helpers.CopyFeatureBuilder<ReactionDefinition>
                    .createFeatureCopy("DeflectMissileCustom", "2fa0624e-a8d2-4e18-862a-4649f40c10b1", "", "Reaction/&DeflectMissileDescriptionCustom", null, DatabaseHelper.ReactionDefinitions.DeflectMissile);
    }

    public CharacterActionDeflectMissileCustom(CharacterActionParams actionParams)
      : base(actionParams)
    {
        if (this.ActionParams.ActionModifiers.Count != 0)
            return;
        this.ActionParams.ActionModifiers.Add(new ActionModifier());
    }

    public override IEnumerator ExecuteImpl()
    {
        CharacterActionDeflectMissileCustom actionDeflectMissile = this;
        GameLocationCharacter target = actionDeflectMissile.ActionParams.TargetCharacters[0];
        actionDeflectMissile.ActingCharacter.TurnTowards(target, false);
        yield return (object)actionDeflectMissile.ActingCharacter.EventSystem.UpdateMotionsAndWaitForEvent(GameLocationCharacterEventSystem.Event.RotationEnd);
        actionDeflectMissile.ActingCharacter.DeflectAttack(actionDeflectMissile.ActionParams.TargetCharacters[0]);

       
        var features = SolastaModHelpers.Helpers.Accessors.extractFeaturesHierarchically< SolastaModHelpers.NewFeatureDefinitions.DeflectMissileCustom>(actionDeflectMissile.ActingCharacter.RulesetCharacter);
        int max_bonus = 0;
        FeatureDefinition deflect_missile_feature = null;
        foreach (var f in features)
        {
            int new_bonus = f.getDeflectMissileBonus(actionDeflectMissile.ActingCharacter.RulesetCharacter);
            if (new_bonus > max_bonus)
            {
                max_bonus = new_bonus;
                deflect_missile_feature = f;
            }
        }

        int reductionAmount = RuleDefinitions.RollDie(actionDeflectMissile.ActionDefinition.DieType, RuleDefinitions.AdvantageType.None, out int _, out int _) + max_bonus;
        actionDeflectMissile.ActionParams.ActionModifiers[0].DamageRollReduction += reductionAmount;

        if (deflect_missile_feature != (BaseDefinition)null)
        {
            RulesetCharacter rulesetCharacter = actionDeflectMissile.ActingCharacter.RulesetCharacter;
            if (rulesetCharacter != null)
                rulesetCharacter.DamageReduced((RulesetActor)actionDeflectMissile.ActingCharacter.RulesetCharacter, deflect_missile_feature, reductionAmount);
        }
        yield return (object)actionDeflectMissile.ActingCharacter.EventSystem.WaitForEvent(GameLocationCharacterEventSystem.Event.HitAnimationEnd);
        actionDeflectMissile.ActingCharacter.TurnTowards(target);
        yield return (object)actionDeflectMissile.ActingCharacter.EventSystem.UpdateMotionsAndWaitForEvent(GameLocationCharacterEventSystem.Event.RotationEnd);


        //perform attack
    }
}


public class ReactionRequestDeflectMissileCustom : ReactionRequest
{
    public const string Name = "DeflectMissileCustom";

    public ReactionRequestDeflectMissileCustom(CharacterActionParams reactionParams)
      : base("DeflectMissileCustom", reactionParams)
    {
    }

    public override string FormatDescription()
    {
        GuiCharacter guiCharacter1 = new GuiCharacter(this.ReactionParams.ActingCharacter);
        GuiCharacter guiCharacter2 = new GuiCharacter(this.ReactionParams.TargetCharacters[0]);

        var features = SolastaModHelpers.Helpers.Accessors.extractFeaturesHierarchically<SolastaModHelpers.NewFeatureDefinitions.DeflectMissileCustom>(this.ReactionParams.ActingCharacter.RulesetCharacter);
        int max_bonus = 0;
        DeflectMissileCustom deflect_missile_feature = null;
        foreach (var f in features)
        {
            int new_bonus = f.getDeflectMissileBonus(this.ReactionParams.ActingCharacter.RulesetCharacter);
            if (new_bonus > max_bonus)
            {
                max_bonus = new_bonus;
                deflect_missile_feature = f;
            }
        }

        return string.Format(base.FormatDescription(), (object)guiCharacter2.Name, (object)guiCharacter1.Name, deflect_missile_feature.characterStat, deflect_missile_feature.characterClass.Name);
    }
}


