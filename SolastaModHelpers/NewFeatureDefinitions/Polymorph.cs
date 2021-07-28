﻿using HarmonyLib;
using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA;
using TA.AI;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class Polymorph : FeatureDefinition, IApplyEffectOnConditionApplication
    {
        static public string tagWildshapeOriginal = "255TagWildshapeOriginal";
        static public string tagWildshapePolymorphed = "255TagWildshapePolymorphed";
        static public string tagWildshapeMerge = "255TagMerge";


        static public List<ConditionDefinition> unstransferableConditions = new List<ConditionDefinition> {DatabaseHelper.ConditionDefinitions.ConditionHealthy,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionEncumbered,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionHeavilyEncumbered,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionHeavyArmorOverload,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionHeavilyObscured,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionDead,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionSeverelyWounded
                                                                                                          };
        public MonsterDefinition monster;
        public bool transfer_features;

        private List<GameLocationCharacter> dummyCharacterList = new List<GameLocationCharacter>();
        private List<RulesetActor.SizeParameters> dummySizesList = new List<RulesetActor.SizeParameters>();
        private List<int3> placementPositions = new List<int3>();
        private List<int3> emptyFormationPositions = new List<int3>();

        public void processConditionApplication(RulesetActor actor, ConditionDefinition applied_condition, RulesetImplementationDefinitions.ApplyFormsParams formParams)
        {
            var condition = (actor as RulesetCharacter)?.FindFirstConditionHoldingFeature(this)?.conditionDefinition;
            var target = actor as RulesetCharacter;
            if (applied_condition != condition)
            {
                return;
            }
            var game_location_character = Helpers.Misc.findGameLocationCharacter(actor as RulesetCharacter);

            IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>();
            RulesetCharacterMonster characterMonster = new RulesetCharacterMonster(monster, 0, new RuleDefinitions.SpawnOverrides(), GadgetDefinitions.CreatureSex.Random);
            RulesetCondition condition2 = characterMonster.InflictCondition(Common.wildshaped_unit_condition.Name, RuleDefinitions.DurationType.Permanent, formParams.activeEffect.RemainingRounds, RuleDefinitions.TurnOccurenceType.EndOfTurn, tagWildshapePolymorphed, actor.Guid, (actor as RulesetCharacter).CurrentFaction.Name , 1, string.Empty, 0, 5);
            GameLocationBehaviourPackage behaviourPackage = new GameLocationBehaviourPackage();
            behaviourPackage.NodePositions = new List<int3>();
            behaviourPackage.NodeOrientations = new List<LocationDefinitions.Orientation>();
            behaviourPackage.NodeDecisionPackages = new List<DecisionPackageDefinition>();
            behaviourPackage.EncounterId = service1.GenerateEncounterId((GameGadget)null); ;
            behaviourPackage.FormationDefinition = (FormationDefinition)null;
            behaviourPackage.BattleStartBehavior = GameLocationBehaviourPackage.BattleStartBehaviorType.RaisesAlarm;
            behaviourPackage.IsLeader = false;
            behaviourPackage.DecisionPackageDefinition = DatabaseHelper.DecisionPackageDefinitions.Idle;
            IGameFactionService service2 = ServiceRepository.GetService<IGameFactionService>();
            bool battleInProgress = ServiceRepository.GetService<IGameLocationBattleService>().IsBattleInProgress;
            FactionDefinition currentFaction = characterMonster.CurrentFaction;
            int num = battleInProgress ? 1 : 0;
            RuleDefinitions.Side side = service2.ComputeSide(currentFaction, num != 0);
            characterMonster.ConjuredByParty = side == RuleDefinitions.Side.Ally;
            GameLocationCharacter character = service1.CreateCharacter(8, (RulesetCharacter)characterMonster, side, behaviourPackage);
            formParams.activeEffect.TrackCondition(formParams.sourceCharacter, formParams.sourceCharacter.Guid, (RulesetActor)characterMonster, character.Guid, condition2, tagWildshapeOriginal);

            var target_character = Helpers.Misc.findGameLocationCharacter(target);

            List<FeatureDefinition> features_to_transfer = new List<FeatureDefinition>();
            if (transfer_features && (target is RulesetCharacterHero))
            {
                var hero = target as RulesetCharacterHero;
                foreach (var f in hero.activeFeatures)
                {
                    features_to_transfer.AddRange(f.Value);
                }
            }
            transferContextToWildshapedUnit(target_character, character, formParams.activeEffect, features_to_transfer);
            Main.Logger.Log("Finished context transfer");
            //remove from the game
            RulesetCondition condition3 = target.InflictCondition(Common.polymorph_merge_condition.name, RuleDefinitions.DurationType.Permanent, formParams.activeEffect.RemainingRounds, RuleDefinitions.TurnOccurenceType.EndOfTurn, tagWildshapeMerge, target.Guid, target.CurrentFaction.Name, 1, string.Empty, 0, 5);
            formParams.activeEffect.TrackCondition(formParams.sourceCharacter, formParams.sourceCharacter.Guid, target, character.Guid, condition3, tagWildshapeMerge);

            IGameLocationPositioningService service3 = ServiceRepository.GetService<IGameLocationPositioningService>();
            int3 position = game_location_character.locationPosition;
            LocationDefinitions.Orientation orientation = LocationDefinitions.Orientation.North;
            this.dummyCharacterList.Clear();
            this.dummyCharacterList.Add(character);
            this.dummySizesList.Clear();
            this.dummySizesList.Add(characterMonster.SizeParams);
            this.placementPositions.Clear();

            service3.ComputeFormationPlacementPositions(this.dummyCharacterList, position, orientation, this.emptyFormationPositions, CellHelpers.PlacementMode.Station, this.placementPositions, this.dummySizesList);

            service3.PlaceCharacter(character, this.placementPositions[0], orientation);
            character.RefreshActionPerformances();
            service1.RevealCharacter(character);
            fixSelectedCharacters(target_character, character);
            makeReadyForBattle(target_character, character, formParams.activeEffect);
            Main.Logger.Log("Finished Polymorph Transformation");
        }


        static void fixSelectedCharacters(GameLocationCharacter original, GameLocationCharacter wildshaped)
        {
            var battle =  ServiceRepository.GetService<IGameLocationBattleService>()?.Battle;
            if (battle != null)
            {
                return;
            }
            var service = ServiceRepository.GetService<IGameLocationSelectionService>();
            if (service != null && service.SelectedCharacters.Contains(original))
            {
                var prev_characters = service.SelectedCharacters.ToArray().ToList();
                prev_characters.Remove(original);
                prev_characters.Add(wildshaped);
                PolymorphPatcher.GameLocationSelectionManager_IsCharacterSelectable.ignore_selection_constraints = true;
                service.SelectMultipleCharacters(prev_characters, true);
                PolymorphPatcher.GameLocationSelectionManager_IsCharacterSelectable.ignore_selection_constraints = false;
            }
        }


        static void makeReadyForBattle(GameLocationCharacter original, GameLocationCharacter wildshaped, RulesetEffect effect)
        {
            //remove from battle contenders and initiative order
            var battle = ServiceRepository.GetService<IGameLocationBattleService>()?.Battle;
            if (battle != null)
            {
                battle.playerContenders?.Remove(original);
                battle.enemyContenders?.Remove(original);
                
                battle.initiativeSortedContenders.Remove(wildshaped);
                var idx = battle.initiativeSortedContenders.IndexOf(original);
                if (idx >= 0)
                {
                    Main.Logger.Log("Active contender: " + battle.ActiveContender.Name);
                    if (battle.ActiveContender != original)
                    {
                        battle.initiativeSortedContenders.Remove(wildshaped);
                        battle.initiativeSortedContenders.Insert(idx + 1, wildshaped);
                        battle.initiativeSortedContenders.Remove(original);
                    }
                    else
                    {
                        Main.Logger.Log("Adding active contender");
                        battle.initiativeSortedContenders.Remove(wildshaped);
                        battle.initiativeSortedContenders.Insert(idx + 1, wildshaped);
                        battle.NextTurn();
                        battle.initiativeSortedContenders.Remove(original);
                        battle.activeContenderIndex = idx;
                        battle.ActiveContender = wildshaped;
                        foreach (var kv in original.currentActionRankByType)
                        {
                            wildshaped.currentActionRankByType[kv.Key] = kv.Value;
                        }
                        if (effect != null)
                        {
                            wildshaped.CurrentActionRankByType[effect.ActionType]++;
                        }
                        wildshaped.usedBonusSpell = original.usedBonusSpell;
                        wildshaped.usedMainSpell = original.usedMainSpell;
                        wildshaped.sustainedAttacks = original.sustainedAttacks;
                        wildshaped.usedMainAttacks = original.usedMainAttacks;
                        wildshaped.RefreshActionPerformances();
                        wildshaped.ActionsRefreshed?.Invoke(wildshaped);
                        wildshaped.RulesetCharacter.RefreshAll();
                        wildshaped.UsedTacticalMoves = Math.Min(original.UsedTacticalMoves, wildshaped.MaxTacticalMoves);
                    }
                }
            }
        }


        static void transferContextToWildshapedUnit(GameLocationCharacter original, GameLocationCharacter wildshaped, 
                                                   RulesetEffect effect, List<FeatureDefinition> features_to_add)
        {
            var monster = wildshaped.RulesetCharacter;
            var character = original.RulesetCharacter;
            if (monster == null || character == null)
            {
                return;
            }

            foreach (var ff in features_to_add)
            {
                (monster as RulesetCharacterMonster)?.activeFeatures.Add(ff);
                Main.Logger.Log("Transferred Feature: " + ff.name);
            }

            monster.concentratedSpell = character.concentratedSpell;
            character.concentratedSpell = null;
            foreach (var s in character.spellsCastByMe.ToArray())
            {
                if (s == effect)
                {
                    continue;
                }
                s.caster = monster;
                s.casterId = monster.guid;
                monster.SpellsCastByMe.Add(s);
                s.TerminatedSelf -= new RulesetEffect.TerminatedSelfHandler(character.ActiveSpellTerminatedSelf);
                s.TerminatedSelf += new RulesetEffect.TerminatedSelfHandler(monster.ActiveSpellTerminatedSelf);
                foreach (var c_guid in s.trackedConditionGuids)
                {
                    RulesetCondition local = null;
                    if (RulesetEntity.TryGetEntity<RulesetCondition>(c_guid, out local))
                    {
                        local.sourceGuid = wildshaped.Guid;
                    }
                    character.spellsCastByMe.Remove(s);
                }
            }

            foreach (var s in character.PowersUsedByMe.ToArray())
            {
                if (s == effect)
                {
                    continue;
                }

                s.user = monster;
                s.userId = monster.guid;
                monster.PowersUsedByMe.Add(s);
                foreach (var c_guid in s.trackedConditionGuids)
                {
                    RulesetCondition local = null;
                    if (RulesetEntity.TryGetEntity<RulesetCondition>(c_guid, out local))
                    {
                        local.sourceGuid = wildshaped.Guid;
                    }
                }
                character.PowersUsedByMe.Remove(s);
            }


            //transfer conditions
            foreach (var c in character.conditionsByCategory)
            {
                foreach (var cc in c.Value.ToArray())
                {
                    if (Polymorph.unstransferableConditions.Contains(cc.conditionDefinition))
                    {
                        continue;
                    }
                    if (effect.trackedConditionGuids.Contains(cc.guid))
                    {
                        continue;
                    }
                    cc.targetGuid = monster.guid;

                    monster.AddConditionCategoryAsNeeded(c.Key);
                    monster.conditionsByCategory[c.Key].Add(cc);
                    Main.Logger.Log("Transferred Condition: " + cc.Name);
                    var caster = RulesetEntity.GetEntity<RulesetCharacter>(cc.SourceGuid);
                    if (caster == null)
                    {
                        continue;
                    }
                    
                    foreach (var e in caster.EnumerateActiveEffectsActivatedByMe())
                    {
                        foreach (var c_guid in e.trackedConditionGuids)
                        {
                            if (cc.guid == c_guid)
                            {
                                monster.ConditionRemoved += new RulesetActor.ConditionRemovedHandler(e.ConditionRemoved);
                                monster.ConditionOccurenceReached += new RulesetActor.ConditionOccurenceReachedHandler(e.ConditionOccurenceReached);
                                monster.ConditionSaveRerollRequested += new RulesetActor.ConditionSaveRerollRequestedHandler(e.ConditionSaveRerollRequested);
                                character.ConditionRemoved -= new RulesetActor.ConditionRemovedHandler(e.ConditionRemoved);
                                character.ConditionOccurenceReached -= new RulesetActor.ConditionOccurenceReachedHandler(e.ConditionOccurenceReached);
                                character.ConditionSaveRerollRequested -= new RulesetActor.ConditionSaveRerollRequestedHandler(e.ConditionSaveRerollRequested);
                                break;
                            }
                        }
                    }
                    c.Value.Remove(cc);
                }
            }
        }



        static public void undoPolymorph(GameLocationCharacter original, GameLocationCharacter wildshaped, RulesetEffect effect)
        {
            if (original == null || wildshaped == null || effect == null)
            {
                return;
            }

            transferContextToWildshapedUnit(wildshaped, original, effect, new List<FeatureDefinition>());
            fixSelectedCharacters(wildshaped, original);
            var battle = ServiceRepository.GetService<IGameLocationBattleService>()?.Battle;
            battle?.IntroduceNewContender(original);
            makeReadyForBattle(wildshaped, original, null);
        }

        public static RulesetCharacter extractOriginalFromWildshaped(RulesetCharacter character)
        {
            if (character.conditionsByCategory.ContainsKey(Polymorph.tagWildshapePolymorphed)
                && character.conditionsByCategory[Polymorph.tagWildshapePolymorphed].Count == 1)
            {
                var condition = character.conditionsByCategory[Polymorph.tagWildshapePolymorphed][0];
                RulesetCharacter caster = (RulesetCharacter)null;
                if (RulesetEntity.TryGetEntity<RulesetCharacter>(condition.sourceGuid, out caster))
                {
                    var eff = caster.EnumerateActiveEffectsActivatedByMe().Where(e => e.TrackedConditionGuids.Contains(condition.guid)).FirstOrDefault();
                    if (eff != null)
                    {
                        RulesetCharacter original = null;
                        RulesetCharacter wildshaped = null;
                        if (!Polymorph.extractOriginalAndWildshapeFromEffect(eff, out original, out wildshaped))
                        {
                            return null;
                        }

                        return original;
                    }
                }
            }
            return null;
        }


        public static bool extractOriginalAndWildshapeFromEffect(RulesetEffect effect, out RulesetCharacter original, out RulesetCharacter wildshaped)
        {
            original = null;
            wildshaped = null;

            RulesetCondition original_condition = null;
            RulesetCondition wildshaped_condition = null;
            foreach (var c_guid in effect.trackedConditionGuids)
            {
                RulesetCondition local = null;
                if (RulesetEntity.TryGetEntity<RulesetCondition>(c_guid, out local))
                {
                    if (local.conditionDefinition == Common.polymorph_merge_condition)
                    {
                        original_condition = local;
                    }
                    else if (local.conditionDefinition == Common.wildshaped_unit_condition)
                    {
                        wildshaped_condition = local;
                    }
                }
            }

            if (wildshaped_condition == null || original_condition == null || wildshaped_condition.sourceGuid != original_condition.sourceGuid)
            {
                return false;
            }

            if (!RulesetEntity.TryGetEntity<RulesetCharacter>(original_condition.targetGuid, out original)
                || !RulesetEntity.TryGetEntity<RulesetCharacter>(wildshaped_condition.targetGuid, out wildshaped))
            {

                return false;
            }
            return true;
        }


        static public void maybeProcessPolymorphedDeath(RulesetCharacter character)
        {
            if (character.conditionsByCategory.ContainsKey(tagWildshapePolymorphed)
                && character.conditionsByCategory[tagWildshapePolymorphed].Count == 1)
            {
                var condition = character.conditionsByCategory[tagWildshapePolymorphed][0];

                RulesetCharacter caster = (RulesetCharacter)null;
                if (RulesetEntity.TryGetEntity<RulesetCharacter>(condition.sourceGuid, out caster))
                {
                    var eff = caster.EnumerateActiveEffectsActivatedByMe().Where(e => e.TrackedConditionGuids.Contains(condition.guid)).FirstOrDefault();
                    if (eff != null)
                    {
                        Main.Logger.Log("Terminating Polymorph");
                        if (eff is RulesetEffectPower)
                        {
                            caster.TerminatePower(eff as RulesetEffectPower);
                        }
                        else if (eff is RulesetEffectSpell)
                        {
                            caster.TerminateSpell(eff as RulesetEffectSpell);
                        }
                    }
                }
            }

        }
    }

    class PolymorphPatcher
    {
        [HarmonyPatch(typeof(GuiCharacter), "Name", MethodType.Getter)]
        class GuiCharacter_Name
        {
            static void Postfix(GuiCharacter __instance, ref string __result)
            {
                var character = __instance.rulesetCharacter;
                if (character == null)
                {
                    return;
                }

                var original = Polymorph.extractOriginalFromWildshaped(character);
                if (original != null)
                {
                    __result = original.Name;
                }
            }
        }


        [HarmonyPatch(typeof(GuiCharacter), "FullName", MethodType.Getter)]
        class GuiCharacter_FullName
        {
            static void Postfix(GuiCharacter __instance, ref string __result)
            {
                var character = __instance.rulesetCharacter;
                if (character == null)
                {
                    return;
                }

                var original = Polymorph.extractOriginalFromWildshaped(character);
                if (original != null)
                {
                    __result = original.Name;
                }
            }
        }


        [HarmonyPatch(typeof(RulesetEffect), "Terminate")]
        class RulesetEffect_Terminate
        {
            static bool Prefix(RulesetEffect __instance, bool self)
            {
                if (__instance.Terminated)
                {
                    return true;
                }

                RulesetCondition original_condition = null;
                RulesetCondition wildshaped_condition = null;
                foreach (var c_guid in __instance.trackedConditionGuids)
                {
                    RulesetCondition local = null;
                    if (RulesetEntity.TryGetEntity<RulesetCondition>(c_guid, out local))
                    {
                        if (local.conditionDefinition == Common.polymorph_merge_condition)
                        {
                            original_condition = local;
                        }
                        else if (local.conditionDefinition == Common.wildshaped_unit_condition)
                        {
                            wildshaped_condition = local;
                        }
                    }
                }

                if (wildshaped_condition == null || original_condition == null || wildshaped_condition.sourceGuid != original_condition.sourceGuid)
                {
                    return true;
                }

                RulesetCharacter original = null;
                RulesetCharacter wildshaped = null;
                if (!Polymorph.extractOriginalAndWildshapeFromEffect(__instance, out original, out wildshaped))
                {
                    return true;
                }

                Polymorph.undoPolymorph(Helpers.Misc.findGameLocationCharacter(original), Helpers.Misc.findGameLocationCharacter(wildshaped), __instance);

                return true;
            }
        }


        [HarmonyPatch(typeof(PartyControlPanel), "Refresh")]
        class PartyControlPanel_Refresh
        {
            static List<GameLocationCharacter> original_party;
            static bool Prefix(PartyControlPanel __instance)
            {
                IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>();
                if (service1 == null)
                {
                    return true;
                }
                original_party = service1.PartyCharacters.ToArray().ToList();

                foreach (var pc in service1.PartyCharacters.ToArray())
                {
                    if (pc.RulesetCharacter.ConditionsByCategory.ContainsKey(Polymorph.tagWildshapeMerge)
                        && pc.RulesetCharacter.ConditionsByCategory[Polymorph.tagWildshapeMerge].Count > 0)
                    {
                        service1.PartyCharacters.Remove(pc);
                    }
                }
               
                return true;
            }


            static void Postfix(PartyControlPanel __instance)
            {
                IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>();
                if (service1 == null || original_party.Count == 0)
                {
                    return;
                }
                service1.PartyCharacters.Clear();
                foreach (var pc in original_party)
                {
                    service1.PartyCharacters.Add(pc);
                }
            }
        }


        //TODO: patch ApplyVictoryEffects to give  exp to polymorphed characters

        [HarmonyPatch(typeof(GameLocationBattleManager), "TriggerBattleStart")]
        class GameLocationBattleManager_TriggerBattleStart
        {
            static List<GameLocationCharacter> removed_party_characters;
            static bool Prefix(GameLocationCharacter attacker,
                               GameLocationCharacter defender,
                               bool surprised)
            {
                removed_party_characters = new List<GameLocationCharacter>();
                var service = ServiceRepository.GetService<IGameLocationCharacterService>();
                if (service == null)
                {
                    return true;
                }
                foreach (var p in service.PartyCharacters.ToArray())
                {
                    if (p.RulesetCharacter.isRemovedFromTheGame)
                    {
                        removed_party_characters.Add(p);
                        service.PartyCharacters.Remove(p);
                    }
                }
                return true;
            }

            static void Postfix(GameLocationCharacter attacker,
                                GameLocationCharacter defender,
                                bool surprised)
            {
                var service = ServiceRepository.GetService<IGameLocationCharacterService>();
                if (service == null || removed_party_characters.Count() == 0)
                {
                    return;
                }

                foreach (var p in removed_party_characters)
                {
                        service.PartyCharacters.Add(p);
                }
                removed_party_characters.Clear();
            }
        }


        [HarmonyPatch(typeof(GameLocationSelectionManager), "IsCharacterSelectable")]
        internal class GameLocationSelectionManager_IsCharacterSelectable
        {
            static internal bool ignore_selection_constraints = false;

            static void Postfix(GameLocationSelectionManager __instance, GameLocationCharacter gameCharacter, bool ignoreInactive, ref bool __result)
            {
                if (ignore_selection_constraints)
                {
                    __result = true;
                    return;
                }
                if ((gameCharacter.RulesetCharacter?.IsRemovedFromTheGame).GetValueOrDefault())
                {                   
                    __result = false;
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "TerminateAllSpellsAndEffects")]
        internal class RulesetCharacter_TerminateAllSpellsAndEffects
        {

            static bool Prefix(RulesetCharacter __instance)
            {
                Polymorph.maybeProcessPolymorphedDeath(__instance);
                return true;
            }
        }


        [HarmonyPatch(typeof(GameLocationCharacter), "InvokeMoved")]
        class GameLocationCharacter_InvokeMoved
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var invoke_moved = codes.FindLastIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ldstr && x.operand.ToString().Contains("InvokeMoved"));
                var insert_point = invoke_moved + 3;

                codes.InsertRange(insert_point, new List<CodeInstruction>{ new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0),
                                                                       new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldloc_2),
                                                                       new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Action<GameLocationCharacter, int3>(maybeTrackWildshape).Method)
                                                                     }

                                                                 );
                return codes.AsEnumerable();
            }

            static void maybeTrackWildshape(GameLocationCharacter character, int3 fromWorldPosition)
            {
                var ruleset_character = character.RulesetCharacter;
                if (ruleset_character == null || !ruleset_character.ConditionsByCategory.ContainsKey(Polymorph.tagWildshapePolymorphed)
                    || ruleset_character.ConditionsByCategory[Polymorph.tagWildshapePolymorphed].Count() != 1)
                {
                    return;
                }
                var condition = ruleset_character.ConditionsByCategory[Polymorph.tagWildshapePolymorphed][0];

                RulesetCharacter caster = (RulesetCharacter)null;
                if (RulesetEntity.TryGetEntity<RulesetCharacter>(condition.sourceGuid, out caster))
                {
                    var eff = caster.EnumerateActiveEffectsActivatedByMe().Where(e => e.TrackedConditionGuids.Contains(condition.guid)).FirstOrDefault();
                    if (eff != null)
                    {
                        RulesetCharacter original = null;
                        RulesetCharacter wildshaped = null;
                        if (Polymorph.extractOriginalAndWildshapeFromEffect(eff, out original, out wildshaped) && wildshaped == ruleset_character)
                        {
                            GameLocationCharacter fromActor = GameLocationCharacter.GetFromActor(original);
                            if (fromActor != null && original.IsRemovedFromTheGame)
                            {
                                fromActor.LocationPosition = fromWorldPosition;
                                fromActor.StartTeleportTo(fromWorldPosition, fromActor.Orientation, false);
                                fromActor.FinishMoveTo(fromWorldPosition, fromActor.Orientation);
                            }
                        }
                    }
                }
            }
        }
    }
}



