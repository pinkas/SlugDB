using Sirenix.OdinInspector.Editor;
using UnityEditor;
using SlugDB;

public class SlugDBBrowser : OdinMenuEditorWindow
{
    public static OdinMenuTree Tree => tree;
    static OdinMenuTree tree = null;

    [MenuItem("Db/Browser")]
    public static void Open()
    {
        GetWindow<SlugDBBrowser>();
    }

    public static void Refresh()
    {
        GetWindow<SlugDBBrowser>().ForceMenuTreeRebuild();
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
        //tree.Selection.SelectionChanged += OnSelectionChanged;
        return tree;
    }

    public static void BuildTree()
    {
        tree = null;
        tree = new OdinMenuTree(true);
        tree.UpdateMenuTree();
        tree.Config.DrawSearchToolbar = true;

        // TODO - Whenever creating a Table it needs to be explicitely added here. Probably avoidable.

        AddTable<Person>();
        AddTable<Animal>();

        // super slow when dealing with big tables
        //tree.SortMenuItemsByName();
    }

    private static void AddTable<T>() where T : Row
    {
        string tableName = Table<T>.Name;

        Table<T>.Load();

        // In the tree the 'root' for each table is a utility class that allows you to add rows to the table
        tree.Add(tableName, new RowFactory<T>());

        // Add the rows of the table to the Odin tree
        foreach (T row in Table<T>.Rows)
        {
            if (row != null)
            {
                string rowPath = $"{tableName}/{row.PrettyName}";
                tree.Add(rowPath, row);
            }
        }
    }

    //TODO duplicate
    public static void AddRow<T>(string key) where T : Row
    {
        string className = typeof(T).ToString();
        string tableName = Table<T>.Name;

        tree.Add($"{tableName}/{key}", className);

        //tree.SortMenuItemsByName();
    }

    /*
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
    */
}
