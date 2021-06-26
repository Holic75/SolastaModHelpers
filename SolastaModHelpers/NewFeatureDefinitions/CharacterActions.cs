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
        ModifyAttackRollViaPower = 128
    }
}


public class CharacterActionModifyAttackRollViaPower : CharacterActionUsePower
{
    static public ActionDefinition modifyAttackRollViaPowerActionDefinition;
    static public ReactionDefinition modifyAttackRollViaPowerReactionDefinition;

    static public ActionDefinition mainAttackAsBonusAction;

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


