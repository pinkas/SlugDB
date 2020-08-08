#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;
using System;

namespace SlugDB
{
    [Serializable]
    public class Row
    {
        [DelayedProperty, ReadOnly, OnValueChanged("OnPrettyNameChanged"), HorizontalGroup("1/1"), BoxGroup("1", showLabel: false)]
        // TODO what should it be called? key ? prettyName?
        public string prettyName;

        // TODO add validation
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

    /// <summary>
    /// Used when a row needs to reference another row.
    /// Only the uid of the referenced row will be serialized but we can display its values as read only and have buttons to pick or focus it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

#if UNITY_EDITOR
    /// <summary>
    /// Pure editor class that allows you to add a row to your table in the Odin Tree window
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [HideReferenceObjectPicker, HideLabel, ShowInInspector, Serializable]
    public class AddRow<T> where T : Row
    {
        public string key;
        public int uid;

        [Button]
        public void Add()
        {
            T newRow = (T)Activator.CreateInstance(typeof(T));
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
#endif
}

