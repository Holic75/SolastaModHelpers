using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityModManagerNet;
using HarmonyLib;
using I2.Loc;
using SolastaModApi;
using SolastaModApi.Extensions;
using SolastaModHelpers;
using System.Collections.Generic;

namespace SolastaModHelpers
{
    public class Main
    {
        [Conditional("DEBUG")]
        internal static void Log(string msg) => Logger.Log(msg);
        internal static void Error(Exception ex) => Logger?.Error(ex.ToString());
        internal static void Error(string msg) => Logger?.Error(msg);
        internal static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                Logger = modEntry.Logger;
                Translations.Load($@"{UnityModManager.modsPath}/SolastaModHelpers");

                var harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }

            return true;
        }

        internal static void ModEntryPoint()
        {
            Common.initialize();
            CharacterActionModifyAttackRollViaPower.initialize();
            CharacterActionDeflectMissileCustom.initialize();
            CharacterActionConsumePowerUse.initialize();
            ReactionRequestCastSpellInResponseToAttack.initialize();
            ReactionRequestSpendPowerFromBundle.initialize();
            ReactionRequestWarcaster.initialize();
            DatabaseHelper.SpellDefinitions.MageArmor.EffectDescription.SetTargetFilteringTag(RuleDefinitions.TargetFilteringTag.Unarmored); //fix mage armor tag to unarmored


            /*var spells = DatabaseRepository.GetDatabase<SpellDefinition>().GetAllElements();
            foreach (var s in spells)
            {
                var sprite_reference = s.GuiPresentation.SpriteReference;

                if (sprite_reference != null)
                {
                    CustomIcons.Tools.saveSpriteFromAssetReferenceAsPNG(s.GuiPresentation.SpriteReference, $@"{UnityModManager.modsPath}/SolastaModHelpers/Spells/{s.name}.png");
                }
            }


            var powers = DatabaseRepository.GetDatabase<FeatureDefinitionPower>().GetAllElements();
            foreach (var p in powers)
            {
                var sprite_reference = p.GuiPresentation.SpriteReference;

                if (sprite_reference != null)
                {
                    CustomIcons.Tools.saveSpriteFromAssetReferenceAsPNG(p.GuiPresentation.SpriteReference, $@"{UnityModManager.modsPath}/SolastaModHelpers/Powers/{p.name}.png");
                }
            }


            var conditions = DatabaseRepository.GetDatabase<ConditionDefinition>().GetAllElements();
            foreach (var c in conditions)
            {
                var sprite_reference = c.GuiPresentation.SpriteReference;

                if (sprite_reference != null)
                {
                    CustomIcons.Tools.saveSpriteFromAssetReferenceAsPNG(c.GuiPresentation.SpriteReference, $@"{UnityModManager.modsPath}/SolastaModHelpers/Conditions/{c.name}.png");
                }
            }


            var items = DatabaseRepository.GetDatabase<ItemDefinition>().GetAllElements();
            foreach (var c in items)
            {
                var sprite_reference = c.GuiPresentation.SpriteReference;

                if (sprite_reference != null)
                {
                    CustomIcons.Tools.saveSpriteFromAssetReferenceAsPNG(c.GuiPresentation.SpriteReference, $@"{UnityModManager.modsPath}/SolastaModHelpers/Items/{c.name}.png");
                }
            }


            var races = DatabaseRepository.GetDatabase<CharacterRaceDefinition>().GetAllElements();
            foreach (var r in races)
            {
                var sprite_reference = r.GuiPresentation.SpriteReference;

                if (sprite_reference != null)
                {
                    CustomIcons.Tools.saveSpriteFromAssetReferenceAsPNG(r.GuiPresentation.SpriteReference, $@"{UnityModManager.modsPath}/SolastaModHelpers/Races/{r.name}.png");
                }
            }


            var actions = DatabaseRepository.GetDatabase<ActionDefinition>().GetAllElements();
            foreach (var a in actions)
            {
                var sprite_reference = a.GuiPresentation.SpriteReference;

                if (sprite_reference != null)
                {
                    CustomIcons.Tools.saveSpriteFromAssetReferenceAsPNG(a.GuiPresentation.SpriteReference, $@"{UnityModManager.modsPath}/SolastaModHelpers/Actions/{a.name}.png");
                }
            }*/
        }
    }
}

