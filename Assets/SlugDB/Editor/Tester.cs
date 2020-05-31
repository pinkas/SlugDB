using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.IO;
using UnityEditor;
using SlugDB;
using UnityEngine;

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

    [SerializeField, InlineButton(nameof(Add1000RandomsToDb))]
    int quantityToAddToDb = 0;
    
    public void Add1000RandomsToDb()
    {

        for (int i = 0; i < quantityToAddToDb; i++)
        {
            Person person = new Person();
            person.age = UnityEngine.Random.Range(1, 99);
            person.prettyName = Path.GetRandomFileName().Replace(".","");
            person.nickName = Path.GetTempFileName();
            person.height = UnityEngine.Random.Range(30, 210);
          
            person.SetUid(Table<Person>.NextId);
            Table<Person>.keysAdded.Add(person, person.prettyName);
            Table<Person>.Rows.Add(person);
        }
        Table<Person>.SaveToDisk(SaveAlgorythm.Legacy);

        //foreach(Person person in DbList<Person>.Instance.value)
    }

    [Button]
    public static void UnloadPerson()
    {
        Table<Person>.Unload();

        Person ben = Table<Person>.Find("ben", false);
        Debug.Log(ben.age);
    }

}

