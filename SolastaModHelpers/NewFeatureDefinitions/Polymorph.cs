using HarmonyLib;
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
        static public string tagWildshape = "255TagWildshape";
        static public string tagMerge = "255TagMerge";

        public MonsterDefinition monster;
        public ConditionDefinition condition;
        public ConditionDefinition merge_condition;

        private List<GameLocationCharacter> dummyCharacterList = new List<GameLocationCharacter>();
        private List<RulesetActor.SizeParameters> dummySizesList = new List<RulesetActor.SizeParameters>();
        private List<int3> placementPositions = new List<int3>();
        private List<int3> emptyFormationPositions = new List<int3>();

        public void processConditionApplication(RulesetActor actor, ConditionDefinition applied_condition, RulesetImplementationDefinitions.ApplyFormsParams formParams)
        {
            var target = actor as RulesetCharacter;
            if (applied_condition != condition)
            {
                return;
            }
            var game_location_character = Helpers.Misc.findGameLocationCharacter(actor as RulesetCharacter);

            IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>();
            RulesetCharacterMonster characterMonster = new RulesetCharacterMonster(monster, 0, new RuleDefinitions.SpawnOverrides(), GadgetDefinitions.CreatureSex.Random);
            RulesetCondition condition2 = characterMonster.InflictCondition("ConditionConjuredCreature", RuleDefinitions.DurationType.Round, formParams.activeEffect.RemainingRounds, RuleDefinitions.TurnOccurenceType.EndOfTurn, tagWildshape, actor.Guid, (actor as RulesetCharacter).CurrentFaction.Name , 1, string.Empty, 0, 5);
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
            formParams.activeEffect.TrackCondition(formParams.sourceCharacter, formParams.sourceCharacter.Guid, (RulesetActor)characterMonster, character.Guid, condition2, tagWildshape);

            var target_character = Helpers.Misc.findGameLocationCharacter(target);
            transferContextToSummonnedUnit(target_character, character, formParams, applied_condition);
            Main.Logger.Log("Finished context transfer");
            //remove from the game
            Main.Logger.Log("Remaining rounds: " + formParams.activeEffect.RemainingRounds.ToString());
            RulesetCondition condition3 = target.InflictCondition(merge_condition.name, RuleDefinitions.DurationType.Round, formParams.activeEffect.RemainingRounds, RuleDefinitions.TurnOccurenceType.EndOfTurn, tagMerge, target.Guid, target.CurrentFaction.Name, 1, string.Empty, 0, 5);
            formParams.activeEffect.TrackCondition(formParams.sourceCharacter, formParams.sourceCharacter.Guid, target, character.Guid, condition3, tagMerge);

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
            makeReadyForBattle(target_character, character, formParams);
            Main.Logger.Log("Finished Transformation");
        }


        static void makeReadyForBattle(GameLocationCharacter original, GameLocationCharacter wildshaped, RulesetImplementationDefinitions.ApplyFormsParams formParams)
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
                    battle.initiativeSortedContenders.Insert(idx + 1, wildshaped);
                    battle.NextTurn();
                    battle.initiativeSortedContenders.Remove(original);
                    battle.activeContenderIndex = idx;
                    battle.ActiveContender = wildshaped;
                    foreach (var kv in original.currentActionRankByType)
                    {
                        wildshaped.currentActionRankByType[kv.Key] = kv.Value;
                    }
                    wildshaped.CurrentActionRankByType[formParams.activeEffect.ActionType]++;
                    wildshaped.usedBonusSpell = original.usedBonusSpell;
                    wildshaped.usedMainSpell = original.usedMainSpell;
                    wildshaped.sustainedAttacks = original.sustainedAttacks;
                    wildshaped.usedMainAttacks = original.usedMainAttacks;
                    wildshaped.RefreshActionPerformances();
                    wildshaped.ActionsRefreshed?.Invoke(wildshaped);
                    wildshaped.RulesetCharacter.RefreshAll();
                }
            }
        }

        static void transferContextToSummonnedUnit(GameLocationCharacter original, GameLocationCharacter wildshaped, 
                                                   RulesetImplementationDefinitions.ApplyFormsParams formParams,
                                                   ConditionDefinition applied_condition)
        {
            var monster = wildshaped.RulesetCharacter as RulesetCharacterMonster;
            var character = original.RulesetCharacter as RulesetCharacterHero;
            if (monster == null || character == null)
            {
                return;
            }


            foreach (var f in character.activeFeatures)
            {
                foreach (var ff in f.Value)
                {
                    monster.activeFeatures.Add(ff);
                    Main.Logger.Log("Transferred Feature: " + ff.Name);
                }
            }


            monster.concentratedSpell = character.concentratedSpell;
            character.concentratedSpell = null;
            foreach (var s in character.spellsCastByMe.ToArray())
            {
                if (s == formParams.activeEffect)
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
                if (s == formParams.activeEffect)
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
                    if (cc.conditionDefinition == applied_condition)
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
    }

    class PolymorphPatcher
    {
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

                foreach (var p in removed_party_characters)
                {
                        service.PartyCharacters.Add(p);
                }
                removed_party_characters.Clear();
            }
        }


        [HarmonyPatch(typeof(GameLocationSelectionManager), "IsCharacterSelectable")]
        class GameLocationSelectionManager_IsCharacterSelectable
        {
            static void Postfix(GameLocationSelectionManager __instance, GameLocationCharacter gameCharacter, bool ignoreInactive, ref bool __result)
            {
                if ((gameCharacter.RulesetCharacter?.IsRemovedFromTheGame).GetValueOrDefault())
                {
                    __result = false;
                }
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
                Main.Logger.Log("Trying to move character: " + ruleset_character.Name);
                if (ruleset_character == null || !ruleset_character.ConditionsByCategory.ContainsKey(Polymorph.tagWildshape)
                    || ruleset_character.ConditionsByCategory[Polymorph.tagWildshape].Count() != 1)
                {
                    return;
                }
                var condition = ruleset_character.ConditionsByCategory[Polymorph.tagWildshape][0];

                RulesetCharacter entity = (RulesetCharacter)null;
                if (RulesetEntity.TryGetEntity<RulesetCharacter>(condition.sourceGuid, out entity))
                {
                    GameLocationCharacter fromActor = GameLocationCharacter.GetFromActor((RulesetActor)entity);
                    if (fromActor != null && entity.IsRemovedFromTheGame)
                    {
                        Main.Logger.Log("Moving character");
                        fromActor.LocationPosition = fromWorldPosition;
                        fromActor.StartTeleportTo(fromWorldPosition, fromActor.Orientation, false);
                        fromActor.FinishMoveTo(fromWorldPosition, fromActor.Orientation);
                    }
                }
            }
        }
    }
}



