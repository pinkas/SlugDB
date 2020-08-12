# SlugDB


The SlugDB is a database solution for Unity (**you'll need [Odin](https://odininspector.com/) to use it**.
SlugDB's goal is to combine Unity's powerfull editor scripting with a traditional database approach (think MySQL). Most of the heavy lifting in terms of editor scripting will be left to Odin and will allow us to edit json text files in the Unity.

### How to use / core concepts
The database is made of tables, tables contains rows (just like MySQL).

Create a table by creating a class that inherits from Row (I recognize it is funny)
```c#
[Serializable, InlineProperty, HideReferenceObjectPicker]
public class Person : Row
{
    public string nickName;
    public int age;
    public int height;

    // a bit of boiler plate required at the moment
    public Person(string prettyName)
    {
        this.prettyName = prettyName;
    }
}
```

If you wish to reference a row from any other row (whether they share the same type or not) you'll need to add the following:

```c#
[Serializable, InlineProperty, HideReferenceObjectPicker, HideDuplicateReferenceBox]
public class PersonReference : RowReference<Person> {}
```

This will allow you to that:

![RowReference Gif](ReadmeResources/rowReference.gif)

To add a row:



### A few gifs

### still a work in progress


#### - Memory consumption
#### - Experimental branch

### Notes:
* Odin is obviously not included in the repository and you'll get compiler errors when cloning/downloading until you import it to your project)
* Minimum compatible version of Unity is 2018.3 since I'm using Addressables (via my ResourcesReference class). 
