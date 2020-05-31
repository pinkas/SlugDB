using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Text;

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
            Table<T>.Instance.rows.Add(newRow);

            // TODO do I need uids?
            //nextId++;

            Table<T>.keysAdded.Add(newRow, key);
            Table<T>.SaveToDisk();
        }

        [Button]
        public void Save()
        {
            Table<T>.SaveToDisk();
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
        private string prettyName => Table<T>.Instance.rows.FirstOrDefault(p => p.Uid == uid).prettyName;

        public T Get => Table<T>.Instance.rows.FirstOrDefault(p => p.Uid == uid);

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
            rows = new List<T>();
        }

        public static Table<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    //instance = Load();
                    instance = new Table<T>();
                }
                return instance;
            }
        }
        static Table<T> instance;

        [ShowInInspector,  ListDrawerSettings(CustomAddFunction = nameof(OnRowAdded))]
        public List<T> rows = new List<T>();

        public static Dictionary<T, string> keysAdded = new Dictionary<T, string>();
        public static List<string> keysDeleted = new List<string>();

        public static string FilePath => filePath;
        private static string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_table.txt");

        public static string TempFilePath => tempFilePath;
        private static string tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_table_temp.txt");

        public static string KeysPath => Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_keys.cs");
        public static string Name => theName;
        private static string theName = typeof(T) + "Table";

        public static Table<T> Load()
        {
            if (instance != null)
            {
                return instance;
            }

#if UNITY_EDITOR
         //   AssemblyReloadEvents.beforeAssemblyReload += SaveAndExport;
#endif

            if (!File.Exists(FilePath))
            {
                var stream = File.Create(FilePath);
                stream.Dispose();
            }

            string serializedList = File.ReadAllText(FilePath);
            instance = JsonUtility.FromJson<Table<T>>(serializedList) ?? new Table<T>();

            return instance;
        }

        public static T Find(string name, bool cache)
        {
            //T theObject = Instance.rows.FirstOrDefault(row => row != null && row.prettyName == name);

            T theObject = null;
            for (int i = 0; i < Instance.rows.Count; i++)
            {
                T row = Instance.rows[i];
                if (row != null && row.prettyName == name)
                {
                    theObject = row;
                }
            }

            if (theObject != null)
            {
                Debug.Log("was cached!");
                return theObject;
            }

            int objectStartingLine = 0;

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

            if (cache && !Instance.rows.Contains(theObject))
            {
                Instance.rows.Add(theObject);
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
            instance = null;
        }

        public int NextId => nextId;

        [SerializeField, ReadOnly]
        private int nextId;

#if UNITY_EDITOR
        private void OnRowAdded()
        {
            T member = (T) Activator.CreateInstance(typeof(T));
            member.SetUid(nextId);
            rows.Add(member);

            nextId++;
        }

        [Button]
        public static void SaveToDisk()
        {
            DateTime time = DateTime.Now;

            // easy way
            //string serializedDb = JsonUtility.ToJson(instance, true);
            //File.WriteAllText(Table<Person>.FilePath, serializedDb);
            //AssetDatabase.SaveAssets();

            File.Delete(TempFilePath);
            using (FileStream fs = File.Open(TempFilePath, FileMode.OpenOrCreate))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8, 1024*4))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();

                writer.WritePropertyName("nextId");
                writer.WriteValue(Instance.nextId);

                writer.WritePropertyName("rows");
                writer.WriteStartArray();

                // TODO
                // get the list of what's dirty
                // you stream to it 

                List<string> allKeys = GetAllKeys();

                for (int i = 0; i < allKeys.Count; i++)
                {
                    string key = allKeys[i];

                    T theRow = null;
                    foreach(T aRow in Instance.rows)
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
                        writer.WriteRaw(rowSerialized);
                        if (i != allKeys.Count - 1)
                        {
                            writer.WriteRaw(",");
                        }
                    }
/*
                    T row = Instance.rows.SingleOrDefault(roow => roow != null && roow.prettyName == key) ?? Find(key, false);
                    if (row != null)
                    {
                        string rowSerialized = JsonConvert.SerializeObject(row, Formatting.Indented);
                        writer.WriteRaw(rowSerialized);
                        if (i != allKeys.Count - 1)
                        {
                            writer.WriteRaw(",");
                        }
                    }
*/
                }

                writer.WriteEnd();
                writer.WriteEndObject();
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
            SaveToDisk();

            string className = typeof(T).ToString();

            string classFile = "using System.Linq;\n";
            classFile += "using SlugDB;\n";
            classFile += "\n";
            classFile += $"public class {className}Table : Table<{className}>\n";
            classFile += "{\n";

            foreach(Row item in Instance.rows)
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

}


