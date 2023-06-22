using UnityEngine;
using System;


#if UNITY_EDITOR
namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(ClassNameAttribute), true)]
    public class ClassNameAttributeDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //TODO....
        }
    }
}
#endif


[AttributeUsage(AttributeTargets.Class)]
public class ClassNameAttribute : PropertyAttribute
{
    public ClassNameAttribute()
    { 
    
    }
}
