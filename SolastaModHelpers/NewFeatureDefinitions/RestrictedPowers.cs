using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public interface IRestriction
    {
        bool isForbidden(RulesetActor character);
    }


    public class InBattleRestriction : IRestriction
    {
        public bool isForbidden(RulesetActor character)
        {
            return ServiceRepository.GetService<IGameLocationBattleService>()?.Battle == null;
        }
    }


    public class NoConditionRestriction : IRestriction
    {
        private ConditionDefinition condition;

        public bool isForbidden(RulesetActor character)
        {
            return character.HasConditionOfType(condition.Name);
        }

        public NoConditionRestriction(ConditionDefinition forbidden_condition)
        {
            condition = forbidden_condition;
        }
    }


    public interface IPowerRestriction
    {
        bool isForbidden(RulesetActor character);
    }


    public class PowerWithRestrictions : LinkedPower, IPowerRestriction 
    {
        public List<IRestriction> restrictions = new List<IRestriction>();

        public bool isForbidden(RulesetActor character)
        {
            foreach (var r in restrictions)
            {
                if (r.isForbidden(character))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
