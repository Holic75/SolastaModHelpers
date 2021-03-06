using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaModHelpers.Helpers
{

    public class StringProcessing
    {
        public static string addStringCopy(string old_string_id, string new_string_id)
        {
            var languageSourceData = LocalizationManager.Sources[0];
            if (!languageSourceData.mDictionary.ContainsKey(old_string_id))
            {
                throw new SystemException($"String: {old_string_id} is not present in LanguageSourceData");
            }
            if (languageSourceData.mDictionary.ContainsKey(new_string_id))
            {
                throw new SystemException($"String: {new_string_id} is already present in LanguageSourceData");
            }

            var term = languageSourceData.mDictionary[old_string_id];
            var new_term = languageSourceData.AddTerm(new_string_id).Languages = term.Languages.ToArray();

            return new_string_id;
        }


        public static string appendToString(string old_string_id, string new_string_id, string text_to_append)
        {
            var languageSourceData = LocalizationManager.Sources[0];
            if (!languageSourceData.mDictionary.ContainsKey(old_string_id))
            {
                throw new SystemException($"String: {old_string_id} is not present in LanguageSourceData");
            }
            if (languageSourceData.mDictionary.ContainsKey(new_string_id))
            {
                throw new SystemException($"String: {new_string_id} is already present in LanguageSourceData");
            }

            var term = languageSourceData.mDictionary[old_string_id];
            var new_term = languageSourceData.AddTerm(new_string_id);
            new_term.Languages = term.Languages.ToArray();

            for (int i = 0; i < new_term.Languages.Count(); i++)
            {
                new_term.Languages[i] = new_term.Languages[i] + text_to_append;
            }

            return new_string_id;
        }


        public static string replaceTagInString(string old_string_id, string new_string_id, string tag, string tag_replacement)
        {
            var languageSourceData = LocalizationManager.Sources[0];
            if (!languageSourceData.mDictionary.ContainsKey(old_string_id))
            {
                throw new SystemException($"String: {old_string_id} is not present in LanguageSourceData");
            }
            if (languageSourceData.mDictionary.ContainsKey(new_string_id))
            {
                throw new SystemException($"String: {new_string_id} is already present in LanguageSourceData");
            }

            var term = languageSourceData.mDictionary[old_string_id];
            var new_term = languageSourceData.AddTerm(new_string_id);
            new_term.Languages = term.Languages.ToArray();

            for (int i = 0; i < new_term.Languages.Count(); i++)
            {
                if (new_term.Languages[i] != null)
                {
                    new_term.Languages[i] = new_term.Languages[i].Replace(tag, tag_replacement);
                }
            }

            return new_string_id;
        }


        public static string replaceTagsInString(string old_string_id, string new_string_id, params (string, string)[] tag_replacement)
        {
            var languageSourceData = LocalizationManager.Sources[0];
            if (!languageSourceData.mDictionary.ContainsKey(old_string_id))
            {
                throw new SystemException($"String: {old_string_id} is not present in LanguageSourceData");
            }
            if (languageSourceData.mDictionary.ContainsKey(new_string_id))
            {
                throw new SystemException($"String: {new_string_id} is already present in LanguageSourceData");
            }

            var term = languageSourceData.mDictionary[old_string_id];
            var new_term = languageSourceData.AddTerm(new_string_id);
            new_term.Languages = term.Languages.ToArray();
            for (int i = 0; i < new_term.Languages.Count(); i++)
            {
                var s = new_term.Languages[i];
                if (s == null)
                {
                    continue;
                }
                foreach (var tr in tag_replacement)
                {
                    s = s.Replace(tr.Item1, tr.Item2);
                }
                new_term.Languages[i] = s;
            }

            return new_string_id;
        }


        public static string addCustomString(string new_string_id, string format_string, params (string, string)[] tag_replacement)
        {
            var languageSourceData = LocalizationManager.Sources[0];
            if (languageSourceData.mDictionary.ContainsKey(new_string_id))
            {
                throw new SystemException($"String: {new_string_id} is already present in LanguageSourceData");
            }

            List<(string, TermData)> tag_terms = new List<(string, TermData)>();

            foreach (var tr in tag_replacement)
            {
                if (!languageSourceData.mDictionary.ContainsKey(tr.Item2))
                {
                    throw new SystemException($"String: {tr.Item2} is not found");
                }
                tag_terms.Add((tr.Item1, languageSourceData.mDictionary[tr.Item2]));
            }
            var term = languageSourceData.mDictionary[Common.common_no_title];
            var new_term = languageSourceData.AddTerm(new_string_id);
            new_term.Languages = term.Languages.ToArray();
            for (int i = 0; i < new_term.Languages.Count(); i++)
            {
                string s = format_string;

                foreach (var tt in tag_terms)
                {
                    s = s.Replace(tt.Item1, tt.Item2.Languages[i]);
                }
                new_term.Languages[i] = s;
            }

            return new_string_id;
        }


        public static string concatenateStrings(string old_string_id1, string old_string_id2, string new_string_id, string text_in_between = "")
        {
            var languageSourceData = LocalizationManager.Sources[0];
            if (!languageSourceData.mDictionary.ContainsKey(old_string_id1))
            {
                throw new SystemException($"String: {old_string_id1} is not present in LanguageSourceData");
            }

            if (!languageSourceData.mDictionary.ContainsKey(old_string_id2))
            {
                throw new SystemException($"String: {old_string_id2} is not present in LanguageSourceData");
            }

            if (languageSourceData.mDictionary.ContainsKey(new_string_id))
            {
                throw new SystemException($"String: {new_string_id} is already present in LanguageSourceData");
            }

            var term1 = languageSourceData.mDictionary[old_string_id1];
            var term2 = languageSourceData.mDictionary[old_string_id2];

            var new_term = languageSourceData.AddTerm(new_string_id);
            new_term.Languages = term1.Languages.ToArray();

            for (int i = 0; i < new_term.Languages.Count(); i++)
            {
                if (new_term.Languages[i] == null)
                {
                    continue;
                }
                if (term2.Languages.Count() > i)
                {
                    new_term.Languages[i] = new_term.Languages[i] + text_in_between + term2.Languages[i];
                }
            }
            return new_string_id;
        }


        public static string concatenateStrings(string new_string_id, params (string, string)[] string_separator_list)
        {
            var languageSourceData = LocalizationManager.Sources[0];
            var terms = new List<TermData>();

            foreach (var s in string_separator_list)
            {
                if (!languageSourceData.mDictionary.ContainsKey(s.Item1))
                {
                    throw new SystemException($"String: {s.Item1} is not present in LanguageSourceData");
                }
                terms.Add(languageSourceData.mDictionary[s.Item1]);
            }


            if (languageSourceData.mDictionary.ContainsKey(new_string_id))
            {
                throw new SystemException($"String: {new_string_id} is already present in LanguageSourceData");
            }

                     
            var new_term = languageSourceData.AddTerm(new_string_id);
            new_term.Languages = terms[0].Languages.ToArray();

            for (int i = 0; i < new_term.Languages.Count(); i++)
            {
                if (new_term.Languages[i] == null)
                {
                    continue;
                }

                for (int j = 1; j < terms.Count; j++)
                {
                    if (terms[j].Languages.Count() > i)
                    {
                        new_term.Languages[i] = new_term.Languages[i] + string_separator_list[j - 1].Item2 + terms[j].Languages[i];
                    }
                    new_term.Languages[i] += string_separator_list.Last().Item2;
                }
            }
            return new_string_id;
        }



        public static string insertStrings(string old_string_id1, string old_string_id2, string new_string_id, string insert_tag)
        {
            var languageSourceData = LocalizationManager.Sources[0];
            if (!languageSourceData.mDictionary.ContainsKey(old_string_id1))
            {
                throw new SystemException($"String: {old_string_id1} is not present in LanguageSourceData");
            }

            if (!languageSourceData.mDictionary.ContainsKey(old_string_id2))
            {
                throw new SystemException($"String: {old_string_id2} is not present in LanguageSourceData");
            }

            if (languageSourceData.mDictionary.ContainsKey(new_string_id))
            {
                throw new SystemException($"String: {new_string_id} is already present in LanguageSourceData");
            }

            var term1 = languageSourceData.mDictionary[old_string_id1];
            var term2 = languageSourceData.mDictionary[old_string_id2];

            var new_term = languageSourceData.AddTerm(new_string_id);
            new_term.Languages = term1.Languages.ToArray();

            for (int i = 0; i < new_term.Languages.Count(); i++)
            {
                if (new_term.Languages[i] == null)
                {
                    continue;
                }
                if (term2.Languages.Count() > i)
                {
                    new_term.Languages[i] = new_term.Languages[i].Replace(insert_tag, term2.Languages[i]);
                }
            }
            return new_string_id;
        }


        public static void addPowerReactStrings(FeatureDefinitionPower power, string use_power_title, string use_power_description, 
                                               string react_title, string react_description, string prefix = "Use")
        {
            var power_name = power.name;
            addStringCopy(use_power_title, "Reaction/&" + prefix + power_name + "Title");
            addStringCopy(use_power_description, "Reaction/&" + prefix + power_name + "Description");
            addStringCopy(react_title, "Reaction/&" + prefix + power_name + "ReactTitle");
            addStringCopy(react_description, "Reaction/&" + prefix + power_name + "ReactDescription");
        }
    }
}
