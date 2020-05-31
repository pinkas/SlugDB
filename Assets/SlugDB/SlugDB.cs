using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;
using System.Text;
using Newtonsoft.Json;

namespace SlugDB
{
    [HideReferenceObjectPicker, HideLabel, ShowInInspector, Serializable]
    public class AddRow<T> where T : Row
    {
        public string key;
        public int uid;

        [Button]
        public void Add()
        {
            T newRow = (T)Activator.CreateInstance(typeof(T));
            // TODO what should it be called? key ? prettyName?
            newRow.prettyName = key;
            //newRow.SetUid(nextId);
            Table<T>.Rows.Add(newRow);

            // TODO do I need uids?
            //nextId++;

            Table<T>.keysAdded.Add(newRow, key);
            Table<T>.SaveToDisk(SaveAlgorythm.Legacy);
        }

        [Button]
        public void Save(SaveAlgorythm saveAlgorythm)
        {
            Table<T>.SaveToDisk(saveAlgorythm);
        }
    }

    [Serializable]
    public class Row
    {
        [DelayedProperty, ReadOnly, OnValueChanged("OnPrettyNameChanged"), HorizontalGroup("1/1"), BoxGroup("1", showLabel: false)]
        public string prettyName;

        //TODO add validation
        [ReadOnly, SerializeField, TableColumnWidth(10), Required]
        [HorizontalGroup("1/1"), BoxGroup("1", showLabel: false)]
        protected int uid;
        public int Uid => uid;


        public void SetUid(int uid)
        {
            if (this.uid == 0)
            {
                this.uid = uid;
            }
        }

        private void OnPrettyNameChanged(string prettyName)
        {
            //Debug.Log(this.GetType().Name );
        }
    }


    [Serializable, InlineProperty, HideReferenceObjectPicker]
    public class RowReference<T> where T : Row
    {
        [HorizontalGroup("1")]
        [VerticalGroup("1/1")]
        [SerializeField, ReadOnly]
        private int uid;
        
        [VerticalGroup("1/1")]
        [ShowInInspector, ReadOnly]
        //TODO really not performance friendly
        private string prettyName => Table<T>.Rows.FirstOrDefault(p => p.Uid == uid).prettyName;

        public T Get => Table<T>.Rows.FirstOrDefault(p => p.Uid == uid);

#if UNITY_EDITOR
        [VerticalGroup("1/2")]
        [Button(18)]
        public void Pick()
        {
            GenericSelector<string> selector = new GenericSelector<string>("", false, item => item, Table<T>.GetAllKeys());

            var window = selector.ShowInPopup();
            selector.SelectionConfirmed += selection => this.uid = Table<T>.Find(selection.FirstOrDefault(), true).Uid;
            selector.EnableSingleClickToSelect();
        }

        [VerticalGroup("1/2")]
        [Button(18)]
        public void Focus()
        {
            var item = SlugDBBrowser.Tree.GetMenuItem($"{typeof(T)}Table/{prettyName}");
            SlugDBBrowser.Tree.Selection.Clear();
            SlugDBBrowser.Tree.Selection.Add(item);
        }
#endif    
    }

    [Serializable, InlineProperty, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class Table <T> where T : Row
    {
        protected Table()
        {
            rows = new RowList<T>();
        }

        [ShowInInspector, ListDrawerSettings(CustomAddFunction = nameof(OnRowAdded))]
        private static RowList<T> rows = new RowList<T>();
        public static List<T> Rows => rows.rows;


        public static Dictionary<T, string> keysAdded = new Dictionary<T, string>();
        public static List<string> keysDeleted = new List<string>();

        public static string FilePath => filePath;
        private static string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_table.txt");

        public static string TempFilePath => tempFilePath;
        private static string tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_table_temp.txt");

        public static string KeysPath => Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_keys.cs");
        public static string Name => theName;
        private static string theName = typeof(T) + "Table";

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
            rows = JsonUtility.FromJson<RowList<T>>(serializedList) ?? new RowList<T>();
        }

        public static T Find(string name, bool cache)
        {
            //T theObject = Instance.rows.FirstOrDefault(row => row != null && row.prettyName == name);

            T theObject = null;
            for (int i = 0; i < Rows.Count; i++)
            {
                T row = Rows[i];
                if (row != null && row.prettyName == name)
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
                        if (theObject.prettyName == name)
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

        public static int NextId => nextId;

        [SerializeField, ReadOnly]
        private static int nextId;

#if UNITY_EDITOR
        private void OnRowAdded()
        {
            T member = (T) Activator.CreateInstance(typeof(T));
            member.SetUid(nextId);
            Rows.Add(member);

            nextId++;
        }

        [Button]
        public static void SaveToDiskGen1()
        {
            string serializedDb = JsonUtility.ToJson(Rows, true);
            File.WriteAllText(Table<Person>.FilePath, serializedDb);
        }

        [Button]
        public static void SaveToDisk(SaveAlgorythm saveAlgorythm)
        {
            DateTime time = DateTime.Now;

            File.Delete(TempFilePath);

            List<string> allKeys = GetAllKeys();

            if (saveAlgorythm == SaveAlgorythm.Legacy)
            {
                string serializedDb = JsonUtility.ToJson(rows, prettyPrint: true);

                var stream = File.Create(TempFilePath);
                stream.Dispose();
                
                File.WriteAllText(TempFilePath, serializedDb);
            }
            else if (saveAlgorythm == SaveAlgorythm.utf)
            {
                var stream = File.Create(TempFilePath);

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

                    T theRow = null;
                    foreach (T aRow in Rows)
                    {
                        if (aRow != null && aRow.prettyName == key)
                        {
                            theRow = aRow;
                            break;
                        }
                    }

                    if (theRow == null)
                    {
                        theRow = Find(key, false);
                    }

                    if (theRow != null)
                    {
                        var rowSerialized = Utf8Json.JsonSerializer.Serialize<T>(theRow);
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

                stream.Write(bytes, 0, bytes.Length);
                stream.Dispose();
            }
            else
            {
                using (FileStream fs = File.Open(TempFilePath, FileMode.OpenOrCreate))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartObject();

                    //writer.WritePropertyName("nextId");
                    //writer.WriteValue(nextId);

                    writer.WritePropertyName("rows");
                    writer.WriteStartArray();

                    // TODO
                    // get the list of what's dirty
                    // you stream to it 

                    for (int i = 0; i < allKeys.Count; i++)
                    {
                        string key = allKeys[i];

                        T theRow = null;
                        foreach(T aRow in Rows)
                        {
                            if(aRow != null && aRow.prettyName == key)
                            {
                                theRow = aRow;
                                break;
                            }
                        }

                        if (theRow == null)
                        {
                            theRow = Find(key, false);
                        }

                        if (theRow != null)
                        {
                            string rowSerialized = JsonConvert.SerializeObject(theRow, Formatting.Indented);
                            //string rowSerialized = JsonSerializer.ToJsonString<T>(theRow);
                            writer.WriteRaw(rowSerialized);
                            if (i != allKeys.Count - 1)
                            {
                                writer.WriteRaw(",");
                            }
                        }
                    }

                    writer.WriteEnd();
                    writer.WriteEndObject();
                }

            }

            File.Delete(FilePath);
            File.Copy(TempFilePath, FilePath);

            //TODO the path of the table should come from only place. duplicate
            AssetDatabase.ImportAsset("Assets/" + typeof(T).ToString() + "_table.txt");

            if (keysDeleted.Count > 0 || keysAdded.Count > 0)
            {
                // TODO is it ok to call SlugDBBrowser in from that class (ie calling an editor class)
                SlugDBBrowser.ForceClose();
                SlugDBBrowser.Open();
            }

            keysDeleted.Clear();
            keysAdded.Clear();

            Debug.LogError(DateTime.Now - time);
        }

        [Button]
        public static void SaveAndExport()
        {
            SaveToDisk(SaveAlgorythm.Legacy);

            string className = typeof(T).ToString();

            string classFile = "using System.Linq;\n";
            classFile += "using SlugDB;\n";
            classFile += "\n";
            classFile += $"public class {className}Table : Table<{className}>\n";
            classFile += "{\n";

            foreach(Row item in Rows)
            {
                if ( string.IsNullOrEmpty(item.prettyName))
                {
                    continue;
                }

                classFile += $"    public static {className} {item.prettyName} => Table<{className}>.Find(\"{item.prettyName}\", false);\n";
            }
            classFile += "}\n";

            File.WriteAllText(Table<Person>.KeysPath, classFile);

            AssetDatabase.ImportAsset("Assets/" + typeof(T).ToString() + "_keys.cs");
        }
#endif
    }
    
    [Serializable]
    public class RowList<T> where T : Row
    {
        public List<T> rows = new List<T>();
    }

    public enum SaveAlgorythm
    {
        Legacy,
        net,
        utf
    }

}



