using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using UnityEditor;
using SlugDB;
using System.Linq;
using System;
using UnityEngine;

public class SlugDBBrowser : OdinMenuEditorWindow
{
    public static OdinMenuTree Tree => tree;
    static OdinMenuTree tree = null;

    [MenuItem("Db/Browser")]
    public static void Open()
    {
        OdinMenuEditorWindow window = GetWindow<SlugDBBrowser>();
        //window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
    }

    public static void ForceClose()
    {
        GetWindow<SlugDBBrowser>().Close();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        BuildTree();
        tree.Selection.SelectionChanged += OnSelectionChanged;
        return tree;
    }

    public static void BuildTree()
    {
        tree = null;
        tree = new OdinMenuTree(true);
        tree.UpdateMenuTree();
        tree.Config.DrawSearchToolbar = true;


        Add<Person>();

        //tree.SortMenuItemsByName();
    }

    private static void Add<T>() where T : Row
    {
        string className = typeof(T).ToString();
        string tableName = Table<T>.Name;


        foreach (string key in Table<T>.GetAllKeys())
        {
            if (key != "")
            {
                tree.Add($"{tableName}/{key}" , className);
            }
        }
        tree.Add(tableName, new AddRow<T>());
    }

    //TODO duplicate
    public static void AddRow<T>(string key) where T : Row
    {
        string className = typeof(T).ToString();
        string tableName = Table<T>.Name;

        tree.Add($"{tableName}/{key}", className);

        //tree.SortMenuItemsByName();
    }

    private void OnSelectionChanged(SelectionChangedType obj)
    {
        if (obj == SelectionChangedType.ItemAdded)
        {
            if (tree.Selection.First().Value is string name && name == nameof(Person))
            {
                string tableName = Table<Person>.Name;
                string key = tree.Selection.First().GetFullPath().Replace($"{tableName}/", "");
                Person person = Table<Person>.Find(key, true);

                var treeItem = tree.GetMenuItem($"{tableName}/{key}");
                if (treeItem != null)
                {
                    treeItem.Value = person;
                }
            }
        }
    }
}
