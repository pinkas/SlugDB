using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;
using Newtonsoft.Json;
using Utf8Json.Resolvers;

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

        // TODO better name for keysAdded and keysDeleted - did that 2 months ago and looking at the name I have no idea what it actually does
        public static Dictionary<T, string> keysAdded = new Dictionary<T, string>();
        public static List<string> keysDeleted = new List<string>();

        public static string FilePath => absoluteFilePath;
        private static readonly string unityProjectFilePath = Path.Combine("Assets", typeof(T).ToString() + "_table.txt");
        private static readonly string absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), unityProjectFilePath);

        public static string TempFilePath => tempFilePath;
        private static readonly string tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_table_temp.txt");

        public static string KeysPath => Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_keys.cs");

        public static string Name => name;
        private static readonly string name = typeof(T) + "Table";


        public static void Load()
        {

#if UNITY_EDITOR
         //   AssemblyReloadEvents.beforeAssemblyReload += SaveAndExport;
#endif
            if (!File.Exists(FilePath))
            {
                var stream = File.Create(FilePath);
                stream.Dispose();
            }

            string serializedList = File.ReadAllText(FilePath);
            rows = Utf8Json.JsonSerializer.Deserialize<RowList<T>>( serializedList, StandardResolver.AllowPrivate ) ?? new RowList<T>();
            //rows = JsonUtility.FromJson<RowList<T>>(serializedList) ?? new RowList<T>();
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

            using (FileStream s = File.Open(FilePath, FileMode.Open))
            using (StreamReader sr = new StreamReader(s))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                while (reader.Read())
                {
                    //if (reader.Value != null && reader.TokenType == JsonToken.PropertyName && reader.Value is string keyName && keyName == "prettyName")
                    //{
                    //    reader.Read();
                    //    if (reader.Value is string value && value == name)
                    //    {
                    //        break;
                    //    }
                    //}
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
            if (!File.Exists(FilePath) )
            {
                return keys;
            }

            using (FileStream s = File.Open(FilePath, FileMode.Open))
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

            foreach(KeyValuePair<T, string> kvp in keysAdded)
            {
                keys.Add(kvp.Value);
            }

            return keys.Except(keysDeleted).ToList();
        }

        public static void Unload()
        {
            Rows.Clear();
        }

        #region Editor stuff
#if UNITY_EDITOR

        [Button]
        public static void SaveToDiskGen1()
        {
            string serializedDb = JsonUtility.ToJson(Rows, true);
            File.WriteAllText(FilePath, serializedDb);
        }

        [Button]
        public static void SaveToDisk(SaveAlgorythm saveAlgorythm)
        {
            File.Delete(TempFilePath);

            List<string> allKeys = GetAllKeys();

            if (saveAlgorythm == SaveAlgorythm.UnityJsonUtility)
            {

                var stream = File.Create(TempFilePath);
                stream.Dispose();

                string serializedDb = JsonUtility.ToJson(rows, prettyPrint: true);
                File.WriteAllText(TempFilePath, serializedDb);
            }
            else if (saveAlgorythm == SaveAlgorythm.utf)
            {
                Utf8Json.JsonWriter jsonWriter = new Utf8Json.JsonWriter();
                jsonWriter.WriteBeginObject();
                //jsonWriter.WritePropertyName("nextId");
                //jsonWriter.WriteString(NextId.ToString());
                //jsonWriter.WriteValueSeparator();
                jsonWriter.WritePropertyName("rows");
                jsonWriter.WriteBeginArray();

                for (int i = 0; i < allKeys.Count; i++)
                {
                    string key = allKeys[i];

                    T row = null;
                    // Get the reference of the row from memory (if available)
                    foreach (T cachedRow in Rows)
                    {
                        if (cachedRow != null && cachedRow.PrettyName == key)
                        {
                            row = cachedRow;
                            break;
                        }
                    }

                    // if the row is not cached then go through the text file (slow)
                    if (row == null)
                    {
                        row = Find(key, false);
                    }

                    if (row != null)
                    {
                        byte[] rowSerialized = Utf8Json.JsonSerializer.Serialize<T>(row, StandardResolver.AllowPrivate);
                        jsonWriter.WriteRaw(rowSerialized);
                        if (i != allKeys.Count - 1)
                        {
                            jsonWriter.WriteValueSeparator();
                        }
                    }
                }

                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();

                byte[] bytes = jsonWriter.ToUtf8ByteArray();
                bytes = Utf8Json.JsonSerializer.PrettyPrintByteArray(bytes);

                var stream = File.Create(TempFilePath);
                stream.Write(bytes, 0, bytes.Length);
                stream.Dispose();
            }

            File.Delete(FilePath);
            File.Copy(TempFilePath, FilePath);

            AssetDatabase.ImportAsset(unityProjectFilePath);
            SlugDBBrowser.Refresh();

            keysDeleted.Clear();
            keysAdded.Clear();
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

                classFile += $"    public static {className} {item.PrettyName} => Table<{className}>.Find(\"{item.PrettyName}\", false);\n";
            }
            classFile += "}\n";

            File.WriteAllText(Table<Person>.KeysPath, classFile);

            AssetDatabase.ImportAsset("Assets/" + typeof(T).ToString() + "_keys.cs");
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

