using System;
using Sirenix.OdinInspector;

[Serializable, InlineProperty, HideReferenceObjectPicker, IncludeMyAttributes]
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class SlugRowAttribute : Attribute { }
