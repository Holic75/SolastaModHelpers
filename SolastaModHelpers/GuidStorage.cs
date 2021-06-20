using SolastaModApi;
using System;
using System.Collections.Generic;

namespace SolastaModHelpers
{
    public static class GuidStorage
    {
        static Dictionary<string, string> guids_in_use = new Dictionary<string, string>();
        static bool allow_guid_generation;

        static public void load(string file_content)
        {
            load(file_content, false);
        }

        static public void load(string file_content, bool debug_mode)
        {
            allow_guid_generation = debug_mode;
            guids_in_use = new Dictionary<string, string>();
            using (System.IO.StringReader reader = new System.IO.StringReader(file_content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] items = line.Split('\t');
                    guids_in_use.Add(items[0], items[1]);
                }
            }
        }

        static public void dump(string guid_file_name)
        {
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(guid_file_name))
            {
                foreach (var pair in guids_in_use)
                {
                    sw.WriteLine(pair.Key + '\t' + pair.Value);
                }
            }
        }

        static public void addEntry(string name, string guid)
        {
            string original_guid;
            if (guids_in_use.TryGetValue(name, out original_guid))
            {
                if (original_guid != guid)
                {
                    throw new SystemException($"Asset: {name}, is already registered for object with another guid: {guid}");
                }
            }
            else
            {
                guids_in_use.Add(name, guid);
            }
        }


        static public bool hasStoredGuid(string blueprint_name)
        {
            string stored_guid = "";
            return guids_in_use.TryGetValue(blueprint_name, out stored_guid);
        }


        static public string getGuid(string name)
        {
            string original_guid;
            if (guids_in_use.TryGetValue(name, out original_guid))
            {
                return original_guid;
            }
            else if (allow_guid_generation)
            {
                var new_guid = Guid.NewGuid().ToString("D");
                return new_guid;
            }
            else
            {
                throw new SystemException($"Missing AssetId for: {name}"); //ensure that no guids generated in release mode
            }
        }


        static public string maybeGetGuid(string name, string new_guid = "")
        {
            string original_guid;
            if (guids_in_use.TryGetValue(name, out original_guid))
            {
                return original_guid;
            }
            else
            {
                return new_guid;
            }
        }
    }


    public class BaseDefinitionBuilderWithGuidStorage<TDefinition> : BaseDefinitionBuilder<TDefinition> where TDefinition : BaseDefinition
    {
        protected BaseDefinitionBuilderWithGuidStorage(TDefinition original)
            : base(original)
        {

        }
        protected BaseDefinitionBuilderWithGuidStorage(string name, string guid)
            : base(name, guid == "" ? GuidStorage.getGuid(name) : guid)
        {
            GuidStorage.addEntry(name, Definition.GUID);
        }
        protected BaseDefinitionBuilderWithGuidStorage(TDefinition original, string name, string guid)
            : base(original, name, guid == "" ? GuidStorage.getGuid(name) : guid)
        {
            GuidStorage.addEntry(name, Definition.GUID);
        }
    }

}
