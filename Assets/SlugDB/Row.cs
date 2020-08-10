#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

namespace SlugDB
{
    // TODO add validation
    [Serializable]
    public class Row
    {
        [SerializeField, ReadOnly, HorizontalGroup("1/1"), BoxGroup("1", showLabel: false)]
        protected string prettyName;
        public string PrettyName => prettyName;

        [ReadOnly, SerializeField, TableColumnWidth(10), Required]
        [HorizontalGroup("1/1"), BoxGroup("1", showLabel: false)]
        protected int uid = 0;
        public int Uid => uid;

        public void SetUid(int uid)
        {
            if (this.uid == 0)
            {
                this.uid = uid;
            }
        }
    }

    /// <summary>
    /// Used when a row needs to reference another row.
    /// Only the uid of the referenced row will be serialized but we can display its values as read only and have buttons to pick or focus it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable, InlineProperty, HideReferenceObjectPicker]
    public class RowReference<T> where T : Row
    {
        [HideLabel, ReadOnly, SerializeField, HorizontalGroup]
        private string prettyName;

        // TODO bring back serialization by UID (int) to avoid excessive string comparisons
        public T Get => Table<T>.Rows.FirstOrDefault(p => p.PrettyName == prettyName);

#if UNITY_EDITOR
        [Button(18), HorizontalGroup]
        public void Pick()
        {
            GenericSelector<string> selector = new GenericSelector<string>("", false, item => item, Table<T>.GetAllKeys());

            var window = selector.ShowInPopup();
            selector.SelectionConfirmed += selection => this.prettyName = Table<T>.Find(selection.FirstOrDefault(), true).PrettyName;
            selector.EnableSingleClickToSelect();
        }

        [Button(18), HorizontalGroup]
        public void Focus()
        {
            var item = SlugDBBrowser.Tree.GetMenuItem($"{typeof(T)}Table/{prettyName}");
            SlugDBBrowser.Tree.Selection.Clear();
            SlugDBBrowser.Tree.Selection.Add(item);
        }
#endif    
    }

#if UNITY_EDITOR
    /// <summary>
    /// Pure editor class that allows you to add/remove rows to/from your table in the Odin Tree window
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [HideReferenceObjectPicker, HideLabel, ShowInInspector, Serializable]
    public class RowFactory<T> where T : Row
    {
        [BoxGroup("Add Row")]
        public string key;
        [BoxGroup("Add Row")]
        public int uid;

        [Button, BoxGroup("Add Row"), EnableIf("@!string.IsNullOrEmpty(key)")]
        public void Add()
        {
            T newRow = (T) Activator.CreateInstance(typeof(T), key);
            //newRow.prettyName = key;
            //newRow.SetUid(nextId);
            Table<T>.Rows.Add(newRow);

            // TODO do I need uids?
            //nextId++;

            Table<T>.SaveToDisk(SaveAlgorythm.UnityJsonUtility);
        }

        [ShowInInspector, ValueDropdown(nameof(GetAllKeys)), BoxGroup("Delete Row")]
        string keyToDelete;

        [Button, EnableIf("@!string.IsNullOrEmpty(keyToDelete)"), BoxGroup("Delete Row")]
        public void Delete()
        {
            Table<T>.SaveToDisk(SaveAlgorythm.utf);
        }

        [PropertySpace, PropertyOrder(10)]
        [Button(30)]
        public void Save(SaveAlgorythm saveAlgorythm)
        {
            Table<T>.SaveToDisk(saveAlgorythm);
            Table<T>.BuildKeysFile(); 
        }

        private List<string> GetAllKeys()
        {
            return Table<T>.GetAllKeys();
        }
    }
#endif
}

