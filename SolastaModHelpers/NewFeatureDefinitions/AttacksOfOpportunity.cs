using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IIgnoreAooImmunity
    {
        bool canIgnore(RulesetCharacter attacker, RulesetCharacter defender);
    }


    public interface IMakeAooOnEnemyMoveEnd
    {
        bool canMakeAooOnEnemyMoveEnd(GameLocationCharacter attacker, GameLocationCharacter defender, TA.int3 previous_position);
    }


    public class CanIgnoreDisengage: FeatureDefinition, IIgnoreAooImmunity
    {
        public bool canIgnore(RulesetCharacter attacker, RulesetCharacter defender)
        {
            bool cannot_ignore = Helpers.Accessors.extractFeaturesHierarchically<ICombatAffinityProvider>(defender)
                                    .Any(f => f != DatabaseHelper.FeatureDefinitionCombatAffinitys.CombatAffinityDisengaging
                                              && f.IsImmuneToOpportunityAttack(defender, attacker));
            return !cannot_ignore;
        }
    }



    public class AooIfAllyIsAttacked: FeatureDefinition
    {

    }


    public class Warcaster : FeatureDefinition
    {

    }


    public class AooWhenEnemyEntersReachWithSpecifiedWeaponGroup : FeatureDefinition, IMakeAooOnEnemyMoveEnd
    {
        public List<string> weaponTypes = new List<string>();

        public bool canMakeAooOnEnemyMoveEnd(GameLocationCharacter attacker, GameLocationCharacter defender, int3 previous_position)
        {
            var game_location_battle_manager = ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;
            if (game_location_battle_manager == null)
            {
                return false;
            }

            RulesetAttackMode attack_mode = null;
            if (game_location_battle_manager.CanPerformOpportunityAttackOnCharacter(attacker, defender, defender.locationPosition, previous_position, out attack_mode))
            {
                var weapon2 = (attack_mode?.sourceDefinition as ItemDefinition);
                if (weapon2 == null || !weapon2.isWeapon)
                {
                    return false;
                }

                var description = weapon2.WeaponDescription;
                if (description == null)
                {
                    return false;
                }

                if (!weaponTypes.Empty() && !weaponTypes.Contains(description.WeaponType))
                {
                    return false;
                }
                return true;
            }
            return false;

        }
    }


}
