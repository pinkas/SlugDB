using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using UnityEditor;
using SlugDB;

public class SlugDBBrowser : OdinMenuEditorWindow
{
    public static OdinMenuTree Tree => tree;
    static OdinMenuTree tree = null;

    [MenuItem("Db/Browser")]
    public static void Open()
    {
        OdinMenuEditorWindow window = GetWindow<SlugDBBrowser>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        tree = new OdinMenuTree(true);
        tree.Config.DrawSearchToolbar = true;

        Add<Person>();


        tree.Selection.SelectionChanged += Selection_SelectionChanged;

        return tree;
    }

    private void Add<T>() where T : Row
    {
        string className = typeof(T).ToString();

        tree.Add($"{className}Table", Table<T>.Load());
        foreach (T row in Table<T>.Instance.rows)
        {
            if (string.IsNullOrEmpty(row.prettyName))
            {
                continue;
            }
            tree.Add($"{className}Table/" + row.prettyName, row);
        }
    }

    private void Selection_SelectionChanged(SelectionChangedType obj) {}
}
