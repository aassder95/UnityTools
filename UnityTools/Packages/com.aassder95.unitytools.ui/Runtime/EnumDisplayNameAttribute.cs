using System;

namespace UnityTools.Ui
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : Attribute
    {
        //============================================================
        // Properties
        //============================================================
        public string Name { get; }

        //============================================================
        // Constructors
        //============================================================
        public EnumDisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}
