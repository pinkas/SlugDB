using Sirenix.OdinInspector;
using System;
using SlugDB;

[Serializable, InlineProperty, HideReferenceObjectPicker]
public partial class Person : Row
{
    public string nickName;
    public int age;
    public int height;
    public PersonReference partner;
}


// TODO have Row classes auto generated so that we avoid the following boiler plate code
// The following needs to be added to all classes that derive from Row
public partial class Person : Row
{
    public Person(string prettyName)
    {
        this.prettyName = prettyName;
    }
}

[Serializable, InlineProperty, HideReferenceObjectPicker, HideDuplicateReferenceBox]
public class PersonReference : RowReference<Person> {}
// That's it ... !
