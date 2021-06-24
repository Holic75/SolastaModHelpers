using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IDamageDiceIncrease
    {
        int extraDice(RulesetImplementationDefinitions.ApplyFormsParams forms_params);
    }


    public class WeaponDamageDiceIncreaseOnCriticalHit:FeatureDefinition, IDamageDiceIncrease
    {
        public int value;
        public bool applyToRanged;
        public bool applyToMelee;

        public int extraDice(RulesetImplementationDefinitions.ApplyFormsParams forms_params)
        {
            if (!forms_params.criticalSuccess)
            {
                return 0;
            }

            var weapon = (forms_params.attackMode?.sourceDefinition as ItemDefinition)?.WeaponDescription;
            if (weapon == null)
            {
                return 0;
            }
          
            if (weapon.WeaponTypeDefinition.weaponProximity == RuleDefinitions.AttackProximity.Range && !applyToRanged)
            {
                return 0;
            }

            if (weapon.WeaponTypeDefinition.weaponProximity == RuleDefinitions.AttackProximity.Melee && !applyToMelee)
            {
                return 0;
            }

            return value;
        }
    }
}
