using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IPowerRestriction
    {
        bool isForbidden(RulesetCharacter character);
    }


    public class PowerUsableOnlyInBattle : LinkedPower, IPowerRestriction 
    {
        public bool isForbidden(RulesetCharacter character)
        {
            return ServiceRepository.GetService<IGameLocationBattleService>()?.Battle == null;
        }
    }
}
