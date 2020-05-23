using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.IO;
using UnityEditor;
using SlugDB;

public class Tester : OdinEditorWindow
{
    [MenuItem("Db/Tester")]
    public static void Open()
    {
        var window = GetWindow<Tester>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
    }

    [Button]
    public static void Add1000RandomsToDb()
    {

        for (int i = 0; i < 1000; i++)
        {
            Person person = new Person();
            person.age = UnityEngine.Random.Range(1, 99);
            person.prettyName = Path.GetRandomFileName().Replace(".","");
            person.nickName = Path.GetTempFileName();
            person.height = UnityEngine.Random.Range(30, 210);
          
            person.SetUid(Table<Person>.Instance.NextId);
            Table<Person>.Instance.rows.Add(person);
        }

        //foreach(Person person in DbList<Person>.Instance.value)
    }

    [Button]
    public static void UnloadPerson()
    {
        Table<Person>.Unload();
    }

}

