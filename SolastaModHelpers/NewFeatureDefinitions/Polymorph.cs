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
    public class CancelPolymorphFeature: FeatureDefinition, ITargetApplyEffectOnEffectApplication, IApplyEffectOnConditionRemoval
    {
        public BaseDefinition effectSource;
        private bool removing = false;
        public void processConditionRemoval(RulesetActor actor, ConditionDefinition condition)
        {
            if (condition == Common.wildshaped_unit_condition)
            {
                if (!removing)
                {
                    removing = true;
                    Polymorph.maybeProcessPolymorphedDeath(actor as RulesetCharacter);
                    removing = false;
                }
            }
        }

        public void processTargetEffectApplication(RulesetCharacter target, List<EffectForm> effectForms, RulesetImplementationDefinitions.ApplyFormsParams formsParams)
        {

            if (formsParams.activeEffect?.SourceDefinition == effectSource)
            {
                var battle = ServiceRepository.GetService<IGameLocationBattleService>()?.Battle;
                if (battle != null)
                {
                    if (formsParams.activeEffect.ActionType == ActionDefinitions.ActionType.Bonus || formsParams.activeEffect.ActionType == ActionDefinitions.ActionType.Main)
                    {
                        var game_location_target = Helpers.Misc.findGameLocationCharacter(target);
                        game_location_target.CurrentActionRankByType[formsParams.activeEffect.ActionType]++;
                    }
                }
                Polymorph.maybeProcessPolymorphedDeath(target);
            }
        }
    }

    public class Polymorph : FeatureDefinition, IApplyEffectOnConditionApplication
    {
        static public string tagWildshapeOriginal = "255TagWildshapeOriginal";
        static public string tagWildshapePolymorphed = "255TagWildshapePolymorphed";
        static public string tagWildshapeMerge = "255TagMerge";

        static public HashSet<ConditionDefinition> unstransferableConditions = new HashSet<ConditionDefinition> {DatabaseHelper.ConditionDefinitions.ConditionHealthy,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionUnconscious,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionEncumbered,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionHeavilyEncumbered,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionHeavyArmorOverload,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionHeavilyObscured,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionDead,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionSeverelyWounded,
                                                                                                           DatabaseHelper.ConditionDefinitions.ConditionBanished
                                                                                                          };

        static public HashSet<FeatureDefinitionPower> transferablePowers = new HashSet<FeatureDefinitionPower>();

        public MonsterDefinition monster;
        public bool transferFeatures;
        public string[] statsToTransfer = new string[0];
        public bool allowSpellcasting;


        public void processConditionApplication(RulesetActor actor, ConditionDefinition applied_condition, RulesetImplementationDefinitions.ApplyFormsParams formParams)
        {
            List<GameLocationCharacter> dummyCharacterList = new List<GameLocationCharacter>();
            List<RulesetActor.SizeParameters> dummySizesList = new List<RulesetActor.SizeParameters>();
            List<int3> placementPositions = new List<int3>();
            List<int3> emptyFormationPositions = new List<int3>();
            var condition = (actor as RulesetCharacter)?.FindFirstConditionHoldingFeature(this)?.conditionDefinition;
            var target = actor as RulesetCharacter;
            if (applied_condition != condition)
            {
                return;
            }
            var game_location_character = Helpers.Misc.findGameLocationCharacter(actor as RulesetCharacter);

            IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>();
            RulesetCharacterMonster characterMonster = new RulesetCharacterMonster(monster, 0, new RuleDefinitions.SpawnOverrides(), GadgetDefinitions.CreatureSex.Random);
            RulesetCondition condition2 = characterMonster.InflictCondition(Common.wildshaped_unit_condition.Name, RuleDefinitions.DurationType.Round, formParams.activeEffect.RemainingRounds, RuleDefinitions.TurnOccurenceType.StartOfTurn, tagWildshapePolymorphed, actor.Guid, (actor as RulesetCharacter).CurrentFaction.Name , 1, string.Empty, 0, 5);
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
            if (transferFeatures && (target is RulesetCharacterHero))
            {
                var hero = target as RulesetCharacterHero;
                foreach (var f in hero.activeFeatures)
                {
                    features_to_transfer.AddRange(f.Value);
                }
            }
            if (!allowSpellcasting)
            {
                features_to_transfer.Add(Common.polymorph_spellcasting_forbidden);
            }
            transferContextToWildshapedUnit(target_character, character, formParams.activeEffect, features_to_transfer);
            Main.Logger.Log("Finished context transfer");

            foreach (var s in statsToTransfer)
            {
                characterMonster.Attributes[s] = target.Attributes[s];
            }

            foreach (var u in target.UsablePowers)
            {
                if (transferablePowers.Contains(u.powerDefinition))
                {
                    characterMonster.UsablePowers.Add(u);
                }
            }
            characterMonster.Attributes["CharacterLevel"] = target.Attributes["CharacterLevel"];
            characterMonster.Attributes["ProficiencyBonus"] = target.Attributes["ProficiencyBonus"];
            Main.Logger.Log("Finished attribute transfer");

            characterMonster.spellRepertoires = target.spellRepertoires;
            //remove from the game
            RulesetCondition condition3 = target.InflictCondition(Common.polymorph_merge_condition.name, RuleDefinitions.DurationType.Permanent, formParams.activeEffect.RemainingRounds, RuleDefinitions.TurnOccurenceType.StartOfTurn, tagWildshapeMerge, target.Guid, target.CurrentFaction.Name, 1, string.Empty, 0, 5);
            formParams.activeEffect.TrackCondition(formParams.sourceCharacter, formParams.sourceCharacter.Guid, target, character.Guid, condition3, tagWildshapeMerge);

            IGameLocationPositioningService service3 = ServiceRepository.GetService<IGameLocationPositioningService>();
            int3 position = game_location_character.locationPosition;
            LocationDefinitions.Orientation orientation = LocationDefinitions.Orientation.North;
            dummyCharacterList.Clear();
            dummyCharacterList.Add(character);
            dummySizesList.Clear();
            dummySizesList.Add(characterMonster.SizeParams);
            placementPositions.Clear();

            var rand = new Random();
            int k = 1;
            while (placementPositions.Empty())
            {
                service3.ComputeFormationPlacementPositions(dummyCharacterList, position, orientation, emptyFormationPositions, CellHelpers.PlacementMode.Station, placementPositions, dummySizesList);
                position = position + new int3(rand.Next(-k, k), rand.Next(-k, k), rand.Next(-k, k));
                k++;
            }

            service3.PlaceCharacter(character, placementPositions[0], orientation);
            character.RefreshActionPerformances();
            
            //identifyMonster(characterMonster);
            service1.RevealCharacter(character);

            fixSelectedCharacters(target_character, character);
            makeReadyForBattle(target_character, character, formParams.activeEffect);
           

            Main.Logger.Log("Finished Polymorph Transformation");
        }


        /*static void identifyMonster(RulesetCharacterMonster monster)
        {
            IGameLoreService service = ServiceRepository.GetService<IGameLoreService>();
            if (!service.HasBestiaryEntry(monster))
                return;
            MonsterDefinition monsterDefinition = monster.MonsterDefinition;
            KnowledgeLevelDefinition monsterKnowledgeLevel = (KnowledgeLevelDefinition)null;
            foreach (KnowledgeLevelDefinition knowledgeLevelDefinition in DatabaseRepository.GetDatabase<KnowledgeLevelDefinition>())
            {
                if (knowledgeLevelDefinition.Level == 4)
                {
                    monsterKnowledgeLevel = knowledgeLevelDefinition;
                    break;
                }
            }
            if (!((BaseDefinition)service.GetCreatureKnowledgeLevel(monster) != (BaseDefinition)monsterKnowledgeLevel))
                return;
            service.LearnMonsterKnowledge(monsterDefinition, monsterKnowledgeLevel);
        }*/


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
                    if (battle.ActiveContender != original)
                    {
                        battle.initiativeSortedContenders.Remove(wildshaped);
                        battle.initiativeSortedContenders.Insert(idx + 1, wildshaped);
                        battle.initiativeSortedContenders.Remove(original);
                    }
                    else
                    {
                        int used_tactical_moves = original.UsedTacticalMoves;
                        bool usedBonusSpell = original.usedBonusSpell;
                        bool usedMainSpell = original.usedMainSpell;
                        int sustainedAttacks = original.sustainedAttacks;
                        int usedMainAttacks = original.usedMainAttacks;
                        var action_performance = new Dictionary<ActionDefinitions.ActionType, int>();
                        foreach (var kv in original.currentActionRankByType)
                        {
                            action_performance[kv.Key] = kv.Value;
                        }

                        Main.Logger.Log(battle.activeContenderIndex.ToString() + " / " + battle.initiativeSortedContenders.Count.ToString());
                        battle.initiativeSortedContenders.Remove(wildshaped);
                        battle.initiativeSortedContenders.Insert(idx + 1, wildshaped);
                        battle.activeContenderIndex--;
                        battle.NextTurn();
                        battle.initiativeSortedContenders.Remove(original);
                        battle.activeContenderIndex = idx;
                        battle.ActiveContender = wildshaped;
                        foreach (var kv in action_performance)
                        {
                            wildshaped.currentActionRankByType[kv.Key] = kv.Value;
                        }
                        if (effect != null && (effect.ActionType == ActionDefinitions.ActionType.Bonus || effect.ActionType == ActionDefinitions.ActionType.Main))
                        {
                            wildshaped.CurrentActionRankByType[effect.ActionType]++;
                        }
                        wildshaped.usedBonusSpell = usedBonusSpell;
                        wildshaped.usedMainSpell = usedMainSpell;
                        wildshaped.sustainedAttacks = sustainedAttacks;
                        wildshaped.usedMainAttacks = usedMainAttacks;
                        wildshaped.RefreshActionPerformances();
                        wildshaped.ActionsRefreshed?.Invoke(wildshaped);
                        wildshaped.RulesetCharacter.RefreshAll();
                        wildshaped.UsedTacticalMoves = Math.Min(used_tactical_moves, wildshaped.MaxTacticalMoves);
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
            Gui.GuiService.GetScreen<GameLocationScreenExploration>()?.Refresh();
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


        public static RulesetCharacter extractWildshapedFromOriginal(RulesetCharacter character)
        {
            if (character.conditionsByCategory.ContainsKey(Polymorph.tagWildshapeMerge)
                && character.conditionsByCategory[Polymorph.tagWildshapeMerge].Count == 1)
            {
                var condition = character.conditionsByCategory[Polymorph.tagWildshapeMerge][0];
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

                        return wildshaped;
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
        [HarmonyPatch(typeof(RulesetCharacter), "GetSpellcastingLevel")]
        class RulesetCharacter_GetSpellcastingLevel
        {
            static bool Prefix(RulesetCharacter __instance, RulesetSpellRepertoire spellRepertoire, ref int __result)
            {
                var original = Polymorph.extractOriginalFromWildshaped(__instance);
                if (original != null)
                {
                    __result = original.GetSpellcastingLevel(spellRepertoire);
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "RefreshSpellRepertoires")]
        class RulesetCharacter_RefreshSpellRepertoires
        {
            static bool Prefix(RulesetCharacter __instance)
            {
                var original = Polymorph.extractOriginalFromWildshaped(__instance);
                if (original != null)
                {
                    original.RefreshSpellRepertoires();
                    __instance.spellRepertoires = original.spellRepertoires;
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(RulesetActor), "ProcessConditionsMatchingOccurenceType")]
        class RulesetActor_ProcessConditionsMatchingOccurenceType
        {
            static bool Prefix(RulesetActor __instance, RuleDefinitions.TurnOccurenceType occurenceType)
            {
                if (__instance.conditionsByCategory.ContainsKey(Polymorph.tagWildshapePolymorphed)
                    && __instance.conditionsByCategory[Polymorph.tagWildshapePolymorphed].Count == 1)
                {
                    var c = __instance.conditionsByCategory[Polymorph.tagWildshapePolymorphed][0];
                    if (c.EndOccurence == occurenceType && c.HasFinished)
                    {
                        __instance.RemoveCondition(c);
                        __instance.ProcessConditionDurationEnded(c);
                    }
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "AddActiveSpell")]
        class RulesetCharacter_AddActiveSpell
        {
            static void Postfix(RulesetCharacter __instance, RulesetEffectSpell rulesetEffectSpell)
            {
                foreach (var c_guid in rulesetEffectSpell.trackedConditionGuids)
                {
                    RulesetCondition local = null;
                    if (RulesetEntity.TryGetEntity<RulesetCondition>(c_guid, out local))
                    {
                        if (local.conditionDefinition == Common.polymorph_merge_condition
                            || local.conditionDefinition == Common.wildshaped_unit_condition)
                        {
                            Gui.GuiService.GetScreen<GameLocationScreenExploration>()?.partyControlPanel?.Refresh();
                            Polymorph.extractWildshapedFromOriginal(__instance)?.RefreshAll();
                            return;
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(RulesetCharacter), "AddActivePower")]
        class RulesetCharacter_AddActivePower
        {
            static void Postfix(RulesetCharacter __instance, RulesetEffectPower rulesetEffectPower)
            {
                foreach (var c_guid in rulesetEffectPower.trackedConditionGuids)
                {
                    RulesetCondition local = null;
                    if (RulesetEntity.TryGetEntity<RulesetCondition>(c_guid, out local))
                    {
                        if (local.conditionDefinition == Common.polymorph_merge_condition
                            || local.conditionDefinition == Common.wildshaped_unit_condition)
                        {
                            Gui.GuiService.GetScreen<GameLocationScreenExploration>()?.partyControlPanel?.Refresh();
                            Polymorph.extractWildshapedFromOriginal(__instance)?.RefreshAll();
                            return;
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(RulesetEffectSpell), "GetClassLevel")]
        class RulesetEffectSpell_GetClassLevel
        {
            static void Prefix(RulesetEffectSpell __instance, ref RulesetCharacter character)
            {
                if (__instance.spellRepertoire != null)
                {
                    var original = Polymorph.extractOriginalFromWildshaped(character);
                    if (original != null)
                    {
                        character = original;
                    }
                }
            }
        }


        [HarmonyPatch(typeof(RulesetEffectPower), "GetClassLevel")]
        class RulesetEffectPower_GetClassLevel
        {
            static void Prefix(RulesetEffectPower __instance, ref RulesetCharacter character)
            {
                var original = Polymorph.extractOriginalFromWildshaped(character);
                if (original != null)
                {
                    character = original;
                }
            }
        }



        /*[HarmonyPatch(typeof(GuiCharacter), "Name", MethodType.Getter)]
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
        }*/


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
            static List<GameLocationCharacter> original_guests;
            static bool Prefix(PartyControlPanel __instance)
            {
                IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>();
                if (service1 == null)
                {
                    return true;
                }
                original_party = service1.PartyCharacters.ToArray().ToList();
                original_guests = service1.GuestCharacters.ToArray().ToList();

                for (int i = 0; i < service1.PartyCharacters.Count; i++)
                {
                    var wildshaped = Helpers.Misc.findGameLocationCharacter(Polymorph.extractWildshapedFromOriginal(service1.PartyCharacters[i].RulesetCharacter));
                    if (wildshaped != null && service1.GuestCharacters.Contains(wildshaped))
                    {
                        service1.GuestCharacters.Remove(wildshaped);
                        service1.PartyCharacters[i] = wildshaped;
                    }
                }

                foreach (var gc in service1.GuestCharacters.ToArray())
                {
                    if (gc?.RulesetCharacter == null)
                    {
                        continue;
                    }
                    if (gc.RulesetCharacter.conditionsByCategory.ContainsKey(Polymorph.tagWildshapePolymorphed) 
                        && gc.RulesetCharacter.conditionsByCategory[Polymorph.tagWildshapePolymorphed].Count > 0)
                    {
                        service1.GuestCharacters.Remove(gc);
                    }
                }

                return true;
            }


            static void Postfix(PartyControlPanel __instance)
            {
                IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>();
                if (service1 == null)
                {
                    return;
                }
                service1.PartyCharacters.Clear();
                foreach (var pc in original_party)
                {
                    service1.PartyCharacters.Add(pc);
                }

                service1.GuestCharacters.Clear();
                foreach (var gc in original_guests)
                {
                    service1.GuestCharacters.Add(gc);
                }
            }
        }

        //fix experience distribution
        [HarmonyPatch(typeof(GameLocationBattle), "ApplyVictoryEffects")]
        class GameLocationBattle_ApplyVictoryEffects
        {
            static List<GameLocationCharacter> original_contenders_characters;
            static bool Prefix(GameLocationBattle __instance)
            {
                original_contenders_characters = __instance.playerContenders.ToArray().ToList();
                IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>();
                if (service1 == null)
                {
                    return true;
                }

                var party_characters = service1.PartyCharacters.ToArray().ToList();

                foreach (var pc in party_characters)
                {
                    if (!__instance.playerContenders.Contains(pc))
                    {
                        __instance.playerContenders.Add(pc);
                    }
                }
                foreach (var pc in __instance.playerContenders.ToArray())
                {
                    if (pc.RulesetCharacter.conditionsByCategory.ContainsKey(Polymorph.tagWildshapePolymorphed) && pc.RulesetCharacter.conditionsByCategory[Polymorph.tagWildshapePolymorphed].Count > 0)
                    {
                        __instance.playerContenders.Remove(pc);
                    }
                }
                return true;
            }

            static void Postfix(GameLocationBattle __instance)
            {
                __instance.playerContenders = original_contenders_characters;
            }
        }


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
                if (!__result &&
                    gameCharacter.Side == RuleDefinitions.Side.Ally && !gameCharacter.RulesetCharacter.HasForcedBehavior
                    && !((gameCharacter?.RulesetCharacter?.IsDeadOrDyingOrUnconscious).GetValueOrDefault() && ignoreInactive))
                {
                    ServiceRepository.GetService<IPlayerControllerService>()?.ActivePlayerController?.RecomputeControlledCharacters();
                    __result = true;
                }

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

        //remove wildshape units when entering new location, since game seem to just silently terminate all effects without proceeding to removing summoned unt
        [HarmonyPatch(typeof(GameLocationManager), "StopCharacterEffectsIfRelevant")]
        internal class GameLocationManager_StopCharacterEffectsIfRelevant
        {
            static bool Prefix(GameLocationManager __instance, bool willEnterChainedLocation)
            {
                IGameLocationCharacterService service1 = ServiceRepository.GetService<IGameLocationCharacterService>() as GameLocationCharacterManager;
                if (true)
                {
                    foreach (var gc in service1.GuestCharacters.ToArray())
                    {
                        if (Polymorph.extractOriginalFromWildshaped(gc.RulesetCharacter) != null)
                        {
                            Main.Logger.Log("Removed Wildshaped unit on lcoation leave: " + gc.Name);
                            service1.DestroyCharacterBody(gc);
                        }
                    }
                }
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



