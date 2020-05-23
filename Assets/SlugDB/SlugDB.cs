using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

namespace SlugDB
{
    [Serializable]
    public class Row
    {
        [Required("No pretty name means not easily accessible via code", InfoMessageType.Warning)]
        [DelayedProperty, OnValueChanged("OnPrettyNameChanged"), HorizontalGroup("1/1"), BoxGroup("1", showLabel: false)]
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
            Debug.Log(this.GetType().Name );
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
            GenericSelector<T> selector = new GenericSelector<T>("",
                false, item => item.prettyName, Table<T>.Instance.rows);

            var window = selector.ShowInPopup();
            selector.SelectionConfirmed += selection => this.uid = selection.FirstOrDefault().Uid;
            selector.EnableSingleClickToSelect();
        }

        [VerticalGroup("1/2")]
        [Button(18)]
        public void Focus()
        {
            var item = SlugDBBrowser.Tree.GetMenuItem($"{typeof(T).ToString()}Table/{prettyName}");
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
                    instance = Load();
                }
                return instance;
            }
        }
        static Table<T> instance;

        [ListDrawerSettings(CustomAddFunction = nameof(OnRowAdded))]
        public List<T> rows = new List<T>();

        public static string FilePath => Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_table.txt");
        public static string KeysPath => Path.Combine(Directory.GetCurrentDirectory(), "Assets", typeof(T).ToString() + "_keys.cs");

        public static Table<T> Load()
        {
            if (instance != null)
            {
                return instance;
            }

#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += SaveAndExport;
#endif

            Debug.Log("loading " + typeof(T).ToString() + " table");

            if (!File.Exists(FilePath))
            {
                var stream = File.Create(FilePath);
                stream.Dispose();
            }

            string serializedList = File.ReadAllText(FilePath);
            instance = JsonUtility.FromJson<Table<T>>(serializedList) ?? new Table<T>();

            return instance;
        }

        public static void Unload()
        {
            Debug.Log($"Unloading {typeof(T).ToString()} table");
            instance = null;
        }

        public int NextId => nextId;

        [SerializeField, ReadOnly]
        private int nextId;

#if UNITY_EDITOR
        private void OnRowAdded()
        {
            //T member = (T) Activator.CreateInstance(typeof(T), nextId);
            T member = (T) Activator.CreateInstance(typeof(T));
            member.SetUid(nextId);
            rows.Add(member);

            nextId++;

            File.WriteAllText(FilePath, JsonUtility.ToJson(this, true));
        }

        [Button]
        public static void SaveToDisk()
        {
            string serializedDb = JsonUtility.ToJson(instance, true);
            File.WriteAllText(Table<Person>.FilePath, serializedDb);
        }

        [Button]
        public static void SaveAndExport()
        {
            SaveToDisk();

            string className = typeof(T).ToString();

            string classFile = "using System.Linq;\n";
            classFile += "using SlugDB;\n";
            classFile += $"public class {className}Table : Table<{className}>\n";
            classFile += "{\n";

            foreach(Row item in instance.rows)
            {
                if ( string.IsNullOrEmpty(item.prettyName))
                {
                    continue;
                }

                classFile += $"    public static {className} {item.prettyName} => Table<{className}>.{nameof(Instance)}.{nameof(rows)}.SingleOrDefault(p => p.prettyName == \"{item.prettyName}\");\n";
            }
            classFile += "}\n";

            File.WriteAllText(Table<Person>.KeysPath, classFile);

            AssetDatabase.ImportAsset("Assets/" + typeof(T).ToString() + "_keys.cs");
        }
#endif
    }

}


