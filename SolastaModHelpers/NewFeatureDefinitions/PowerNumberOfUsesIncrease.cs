using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    interface IPowerNumberOfUsesIncrease
    {
        void apply(CharacterBuildingManager manager);
        bool isRetroactive();
    }


    public class IncreaseNumberOfPowerUsesPerClassLevel : FeatureDefinition, IPowerNumberOfUsesIncrease
    {
        public List<FeatureDefinitionPower> powers = new List<FeatureDefinitionPower>();
        public CharacterClassDefinition characterClass;
        public Dictionary<int, int> levelIncreaseMap = new Dictionary<int, int>();

        public void apply(CharacterBuildingManager manager)
        {
            CharacterClassDefinition current_class;
            int current_level;
            manager.GetLastAssignedClassAndLevel(out current_class, out current_level);

            if (current_class != characterClass || !levelIncreaseMap.ContainsKey(current_level))
            {
                return;
            }

            int bonus_uses = levelIncreaseMap[current_level];
            var powers_to_process = manager.heroCharacter.UsablePowers.Where(up => powers.Contains(up.PowerDefinition));

            foreach (var p in powers_to_process)
            {
                p.maxUses += bonus_uses;
            }
        }

        public bool isRetroactive()
        {
            return true;
        }
    }


    public class IncreaseNumberOfPowerUses : FeatureDefinition, IPowerNumberOfUsesIncrease
    {
        public List<FeatureDefinitionPower> powers = new List<FeatureDefinitionPower>();
        public int value;

        public void apply(CharacterBuildingManager manager)
        {
            int bonus_uses = value;
            var powers_to_process = manager.heroCharacter.UsablePowers.Where(up => powers.Contains(up.PowerDefinition));

            foreach (var p in powers_to_process)
            {
                p.maxUses += bonus_uses;
            }
        }

        public bool isRetroactive()
        {
            return false;
        }
    }
}
