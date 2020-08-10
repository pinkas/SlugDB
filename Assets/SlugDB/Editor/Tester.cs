using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.IO;
using UnityEditor;
using SlugDB;
using UnityEngine;
using System.Text.RegularExpressions;

public class Tester : OdinEditorWindow
{
    [MenuItem("Db/Tester")]
    public static void Open()
    {
        var window = GetWindow<Tester>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
    }

    [SerializeField]
    SaveAlgorythm saveAlgo;

    [SerializeField, InlineButton(nameof(CreatePersonTableWithXentries), label: "Create")]
    int quantityToAddToDb = 0;
    
    public void CreatePersonTableWithXentries()
    {

        for (int i = 0; i < quantityToAddToDb; i++)
        {
            string prettyName = Path.GetRandomFileName().Replace(".", "");
            prettyName = Regex.Replace(prettyName, "[0-9]", "");

            Person person = new Person(prettyName)
            {
                age = UnityEngine.Random.Range(1, 99),
                nickName = Regex.Replace(Path.GetRandomFileName(), "[0-9]", ""),
                height = UnityEngine.Random.Range(30, 210),
            };
          
            Table<Person>.Rows.Add(person);
        }
    
        Table<Person>.SaveToDisk(saveAlgo);
    }

    [Button]
    public static void GetAllKeys()
    {
        Table<Person>.GetAllKeys();
    }

    [Button]
    public static void UnloadPerson()
    {
        Table<Person>.Unload();

        Person ben = Table<Person>.Find("ben", false);
        Debug.Log(ben.age);
    }

}

