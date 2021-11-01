using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers
{
    public class Fixes
    {
        static internal void fixConjureAnimalDuration()
        {
            foreach (var s in DatabaseHelper.SpellDefinitions.ConjureAnimals.subspellsList)
            {
                s.effectDescription.durationType = RuleDefinitions.DurationType.Hour;
            }
        }
    }
}
