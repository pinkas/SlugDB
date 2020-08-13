using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Newtonsoft.Json;

namespace SlugDB
{
    /// <summary>
    /// The table contains your rows and utilities (some editor only) to save/load/find
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable, InlineProperty, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class Table <T> where T : Row
    {
        protected Table()
        {
            rows = new RowList<T>();
        }

        public static List<T> Rows => rows.value;
        private static RowList<T> rows = new RowList<T>();

        public static string TableFilePathAbsolute => tableFilePathAbsolute;
        private static readonly string tableFilePathUnityProject = Path.Combine("Assets", typeof(T).ToString() + "_table.txt");
        private static readonly string tableFilePathAbsolute = Path.Combine(Directory.GetCurrentDirectory(), tableFilePathUnityProject);

        private static readonly string KeysFilePathUnityProject = Path.Combine("Assets", typeof(T).ToString() + "_keys.cs");
        public static string KeysFilePathAbsolute => Path.Combine(Directory.GetCurrentDirectory(), KeysFilePathUnityProject);

        public static string Name => name;
        private static readonly string name = typeof(T) + "Table";


        public static void Load()
        {
#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += ()=> { SaveToDisk(); };
#endif
            if (!File.Exists(TableFilePathAbsolute))
            {
                var stream = File.Create(TableFilePathAbsolute);
                stream.Dispose();
            }

            string serializedList = File.ReadAllText(TableFilePathAbsolute);
            rows = JsonUtility.FromJson<RowList<T>>(serializedList) ?? new RowList<T>();
        }

        public static T Find(string name, bool cache)
        {
            //T theObject = Instance.rows.FirstOrDefault(row => row != null && row.prettyName == name);

            T theObject = null;
            for (int i = 0; i < Rows.Count; i++)
            {
                T row = Rows[i];
                if (row != null && row.PrettyName == name)
                {
                    theObject = row;
                }
            }

            if (theObject != null)
            {
                return theObject;
            }

            JsonSerializer serializer = new JsonSerializer();

            using (FileStream s = File.Open(TableFilePathAbsolute, FileMode.Open))
            using (StreamReader sr = new StreamReader(s))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                while (reader.Read())
                {
                    if (reader.Depth == 2 && reader.TokenType == JsonToken.StartObject)
                    {
                        theObject = serializer.Deserialize<T>(reader);
                        if (theObject.PrettyName == name)
                        {
                            break;
                        }
                    }
                }
            }

            if (cache && !Rows.Contains(theObject))
            {
                Rows.Add(theObject);
            }

            return theObject;
        }

        public static List<string> GetAllKeys()
        {
            List<string> keys = new List<string>();

            // TODO caching even with newKeys!
            if (!File.Exists(TableFilePathAbsolute) )
            {
                return keys;
            }

            using (FileStream s = File.Open(TableFilePathAbsolute, FileMode.Open))
            using (StreamReader sr = new StreamReader(s))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                while (reader.Read())
                {
                    if (reader.Value != null && reader.TokenType == JsonToken.PropertyName && reader.Value is string keyName && keyName == "prettyName")
                    {
                        reader.Read();
                        keys.Add(reader.Value as string);
                    }
                }
            }

            return keys;
        }

        public static void Unload()
        {
            Rows.Clear();
        }

        #region Editor stuff
#if UNITY_EDITOR

        [Button]
        public static void SaveToDisk(SaveAlgorythm saveAlgorythm = SaveAlgorythm.UnityJsonUtility)
        {
            // other save styles on the 'experimental' branch
            if (saveAlgorythm == SaveAlgorythm.UnityJsonUtility)
            {
                string serializedDb = JsonUtility.ToJson(rows, prettyPrint: true);
                WriteToUnityProject(TableFilePathAbsolute, serializedDb);
            }

            AssetDatabase.ImportAsset(tableFilePathUnityProject);
            SlugDBBrowser.Refresh();

            BuildKeysFile();
        }

        [Button]
        public static void BuildKeysFile()
        {
            string className = typeof(T).ToString();

            string classFile = "using System.Linq;\n";
            classFile += "using SlugDB;\n";
            classFile += "\n";
            classFile += $"public class {className}Table : Table<{className}>\n";
            classFile += "{\n";

            foreach(Row item in Rows)
            {
                if ( string.IsNullOrEmpty(item.PrettyName))
                {
                    continue;
                }

                classFile += $"    public static {className} {item.PrettyName} => Find(\"{item.PrettyName}\", false);\n";
            }
            classFile += "}\n";

            WriteToUnityProject(KeysFilePathAbsolute, classFile);
            AssetDatabase.ImportAsset(KeysFilePathUnityProject);
        }

        private static void WriteToUnityProject(string path, string content)
        {
            try
            {
                File.WriteAllText(path, content);
            }
            catch (Exception e)
            {
                Debug.Log($"{e}\n{e.StackTrace}");
            }
        }
#endif
        #endregion
    }

    /// <summary>
    /// Class necessary because Unity Json Utility doesn't support Lists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class RowList<T> where T : Row
    {
        public List<T> value = new List<T>();
    }

    public enum SaveAlgorythm
    {
        UnityJsonUtility,
        utf
    }

}

