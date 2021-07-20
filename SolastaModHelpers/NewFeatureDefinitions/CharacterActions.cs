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
        DeflectMissileCustom = 129,
        ConsumePowerUse = 130
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


public class CharacterActionConsumePowerUse : CharacterAction
{
    static public ActionDefinition consumePowerUseActionDefinition;
    static public ReactionDefinition consumePowerUseReactionDefinition;

    static public void initialize()
    {
        consumePowerUseActionDefinition = SolastaModHelpers.Helpers.CopyFeatureBuilder<ActionDefinition>
            .createFeatureCopy("ConsumePowerUsection", "a243323c-9c33-4554-9581-51a00e132f7a", "", "", null, DatabaseHelper.ActionDefinitions.SpendPower);
        consumePowerUseActionDefinition.id = (ActionDefinitions.Id)ExtendedActionId.ConsumePowerUse;
        consumePowerUseActionDefinition.classNameOverride = "ConsumePowerUse";

        consumePowerUseReactionDefinition = SolastaModHelpers.Helpers.CopyFeatureBuilder<ReactionDefinition>
                    .createFeatureCopy("ConsumePowerUse", "d95c9f4b-5b6d-4f71-a0ec-ed6015ce3b32", "Reaction/&ConsumePowerUse{0}Title", "Reaction/&ConsumePowerUse{0}Description", null, DatabaseHelper.ReactionDefinitions.SpendPower);

        consumePowerUseReactionDefinition.reactTitle = "Reaction/&ConsumePowerUse{0}ReactTitle";
        consumePowerUseReactionDefinition.reactDescription = "Reaction/&ConsumePowerUse{0}ReactDescription";
    }


    public CharacterActionConsumePowerUse(CharacterActionParams actionParams)
      : base(actionParams)
    {
    }

    public override IEnumerator ExecuteImpl()
    {
        actionParams.ActingCharacter?.RulesetCharacter?.UsePower(actionParams.usablePower);
        yield return null;
    }
}


public class ReactionRequestConsumePowerUse : ReactionRequest
{
    public const string Name = "ConsumePowerUse";

    public ReactionRequestConsumePowerUse(CharacterActionParams reactionParams)
      : base("ConsumePowerUse", reactionParams)
    {
    }

    public override string FormatDescription()
    {
        GuiCharacter guiCharacter = new GuiCharacter(this.ReactionParams.ActingCharacter);
        string empty = string.Empty;
        RulesetEffect rulesetEffect = this.ReactionParams.RulesetEffect;
        string effect_name = !(rulesetEffect is RulesetEffectSpell) ? Gui.Localize((rulesetEffect as RulesetEffectPower).PowerDefinition.GuiPresentation.Title) : Gui.Localize((rulesetEffect as RulesetEffectSpell).SpellDefinition.GuiPresentation.Title);

        var tr_string =  string.Format(DatabaseRepository.GetDatabase<ReactionDefinition>().GetElement(this.DefinitionName).GuiPresentation.Description, 
                             this.ReactionParams.UsablePower.PowerDefinition.name
                             );
        return string.Format(Gui.Localize(tr_string), effect_name);
    }


    public override string FormatTitle()
    {
        GuiCharacter guiCharacter = new GuiCharacter(this.ReactionParams.ActingCharacter);
        string empty = string.Empty;
        return string.Format(DatabaseRepository.GetDatabase<ReactionDefinition>().GetElement(this.DefinitionName).GuiPresentation.Title, this.ReactionParams.UsablePower.PowerDefinition.name);
    }


    public override string FormatReactTitle()
    {
        GuiCharacter guiCharacter = new GuiCharacter(this.ReactionParams.ActingCharacter);
        string empty = string.Empty;
        return string.Format(DatabaseRepository.GetDatabase<ReactionDefinition>().GetElement(this.DefinitionName).ReactTitle, this.ReactionParams.UsablePower.PowerDefinition.name);
    }


    public override string FormatReactDescription()
    {
        GuiCharacter guiCharacter = new GuiCharacter(this.ReactionParams.ActingCharacter);
        string empty = string.Empty;
        return string.Format(DatabaseRepository.GetDatabase<ReactionDefinition>().GetElement(this.DefinitionName).ReactDescription, this.ReactionParams.UsablePower.PowerDefinition.name);
    }
}


public class ReactionRequestCastSpellInResponseToAttack : ReactionRequestCastSpell
{
    static public ReactionDefinition castSpellInResponseToAttackReactionDefinition;
    static public void initialize()
    {
        castSpellInResponseToAttackReactionDefinition = SolastaModHelpers.Helpers.CopyFeatureBuilder<ReactionDefinition>
                    .createFeatureCopy("CastSpellInResponseToAttack", "ead1ad34-ec8b-4416-9746-b1ecdc50adf5", "Reaction/&CastSpellInResponseToAttackTitle", "Reaction/&CastSpellInResponseToAttackDescription", null, DatabaseHelper.ReactionDefinitions.AlterAttackSpell);

        castSpellInResponseToAttackReactionDefinition.reactTitle = "Reaction/&CastSpellInResponseToAttackReactTitle";
        castSpellInResponseToAttackReactionDefinition.reactDescription = "Reaction/&CastSpellInResponseToAttackReactDescription";
    }

    public ReactionRequestCastSpellInResponseToAttack(
      CharacterActionParams actionParams)
      : base("CastSpellInResponseToAttack", actionParams)
    {
        this.BuildSlotSubOptions();
    }

    public override string FormatDescription()
    {
        SpellDefinition spellDefinition = (this.ReactionParams.RulesetEffect as RulesetEffectSpell).SpellDefinition;
        return Gui.Format(base.FormatDescription(), this.reactionParams.targetCharacters[0].RulesetActor.Name, this.Character.Name, spellDefinition.GuiPresentation.Title);
    }

    public override string FormatReactDescription()
    {
        SpellDefinition spellDefinition = (this.ReactionParams.RulesetEffect as RulesetEffectSpell).SpellDefinition;
        return Gui.Format(base.FormatReactDescription(), (this.ReactionParams.RulesetEffect as RulesetEffectSpell).SpellDefinition.GuiPresentation.Title);
    }
}


