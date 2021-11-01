using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Patches
{
    class CharacterActionFreeFallPatcher
    {
        [HarmonyPatch(typeof(CharacterActionFreeFall), "HandleCharacterFall")]
        internal static class CharacterActionFreeFall_HandleCharacterFall_Patch
        {
            internal static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result,
                                                                    CharacterActionFreeFall __instance)
            {
                while (__result.MoveNext())
                {
                    yield return __result.Current;
                }
                var extra_events = Process(__instance);

                while (extra_events.MoveNext())
                {
                    yield return extra_events.Current;
                }
            }

            static RulesetUsablePower maybeGetReactionPowerToPreventFall(RulesetCharacter character)
            {
                var powers = Helpers.Accessors.extractPowers(character, p =>
                {
                    foreach (EffectForm effectForm in p.PowerDefinition.EffectDescription.EffectForms)
                    {
                        if (effectForm.FormType == EffectForm.EffectFormType.Condition && effectForm.ConditionForm.Operation == ConditionForm.ConditionOperation.Add && effectForm.ConditionForm.ConditionDefinition.IsSubtypeOf("ConditionFeatherFalling"))
                        {
                            return true;
                        }
                    }
                    return false;
                });

                return powers.FirstOrDefault();
            }


            internal static System.Collections.IEnumerator Process(CharacterActionFreeFall __instance)
            {
                var character = __instance.ActingCharacter;

                if (!character.RulesetCharacter.IsDeadOrDyingOrUnconscious 
                    && character.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction) == ActionDefinitions.ActionStatus.Available
                    )
                {
                    var usable_power = maybeGetReactionPowerToPreventFall(character.RulesetCharacter);
                    if (usable_power != null)
                    {
                        CharacterActionParams reactionParams = new CharacterActionParams(character, ActionDefinitions.Id.PowerReaction);
                        reactionParams.TargetCharacters.Add(character);
                        reactionParams.ActionModifiers.Add(new ActionModifier());
                        IRulesetImplementationService service2 = ServiceRepository.GetService<IRulesetImplementationService>();
                        reactionParams.RulesetEffect = (RulesetEffect)service2.InstantiateEffectPower(character.RulesetCharacter, usable_power, false);
                        reactionParams.StringParameter = usable_power.PowerDefinition.Name;
                        reactionParams.IsReactionEffect = true;
                        IGameLocationActionService actionService = ServiceRepository.GetService<IGameLocationActionService>();
                        int previousReactionCount = actionService.PendingReactionRequestGroups.Count;
                        actionService.ReactToUsePower(reactionParams, usable_power.PowerDefinition.Name);
                        while (previousReactionCount < actionService.PendingReactionRequestGroups.Count)
                            yield return (object)null;
                        actionService = (IGameLocationActionService)null;
                    }
                }
            }
        }
    }
}
