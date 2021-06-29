using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface ICanUSeDexterityWithWeapon
    {
        bool worksOn(RulesetCharacter character, WeaponDescription weapon_description);
    }


    public class canUseDexterityWithSpecifiedWeaponTypes : FeatureDefinition, ICanUSeDexterityWithWeapon
    {
        public List<string> weaponTypes;
        public bool allowArmor;
        public bool allowShield;

        public bool worksOn(RulesetCharacter character, WeaponDescription weapon_description)
        {
            if (character.IsWearingArmor() && !allowArmor)
            {
                return false;
            }

            if (character.IsWearingShield() && !allowShield)
            {
                return false;
            }

            return weaponTypes.Contains(weapon_description.WeaponType);
        }
    }
}
