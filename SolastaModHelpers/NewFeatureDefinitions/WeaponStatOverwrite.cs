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
        public List<IRestriction> restrictions = new List<IRestriction>();

        public bool worksOn(RulesetCharacter character, WeaponDescription weapon_description)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return false;
                }
            }

            return weaponTypes.Contains(weapon_description.WeaponType);
        }
    }
}
