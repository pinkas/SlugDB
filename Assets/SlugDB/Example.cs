using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using SlugDB;

[Serializable, InlineProperty, HideReferenceObjectPicker]
public class Person : Row
{
    [DelayedProperty, OnValueChanged("Save")]
    public string nickName;
    public int age;
    public PersonalityType personalityType;
    
    public int height;

    public List<PersonReference> siblings = new List<PersonReference>();

    public ResourcesReference buildings;

    public void Save()
    {
        Table<Person>.SaveToDisk();
    }
}

[Serializable, InlineProperty, HideReferenceObjectPicker, HideDuplicateReferenceBox]
public class PersonReference : RowReference<Person> {}

public enum PersonalityType
{
    Extra,
    Regular,
    Intra,
    Crazy,
}
