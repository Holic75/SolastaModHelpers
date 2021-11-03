using I2.Loc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers
{
    public static class Translations
    {
        public static void Load(string fromFolder)
        {
            var languageSourceData = LocalizationManager.Sources[0];
            Main.Logger.Log($"loading translations from dir: {fromFolder}");
            foreach (var path in Directory.EnumerateFiles(fromFolder, $"Translations-??.txt"))
            {
                var filename = Path.GetFileName(path);
                var code = filename.Substring(13, 2);
                var languageIndex = languageSourceData.GetLanguageIndexFromCode(code);
                Main.Logger.Log($"loading translations from file: {filename}");
                if (languageIndex < 0)
                {
                    Main.Error($"language {code} not currently loaded.");
                    continue;
                }

                foreach (var line in File.ReadLines(path))
                {
                    try
                    {
                        var splitted = line.Split(new[] { '\t', ' ' }, 2);
                        var term = splitted[0];
                        var text = splitted[1];

                        languageSourceData.AddTerm(term).Languages[languageIndex] = text;
                    }
                    catch
                    {
                        Main.Error($"invalid translation line \"{line}\".");
                    }
                }
            }
        }
    }
}
