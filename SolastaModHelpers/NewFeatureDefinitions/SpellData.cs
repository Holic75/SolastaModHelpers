using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.NewFeatureDefinitions
{
    public class SpellData
    {
        static Dictionary<string, SpellDefinition> added_spells = new Dictionary<string, SpellDefinition>();

        public static void registerSpell(SpellDefinition spell)
        {
            added_spells[spell.name] = spell;
        }

        public static SpellDefinition getSpell(string name)
        {
            if (!added_spells.ContainsKey(name))
            {
                return null;
            }
            return added_spells[name];
        }
    }
}
