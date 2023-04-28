using System;
using System.Collections.Generic;
using System.Text;

namespace MySourceGenerator
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
    public class AutoRegisterAttribute : Attribute
    {
        public AutoRegisterAttribute()
        {
        }

        public AutoRegisterAttribute(Type idType)
        {
        }
        public AutoRegisterAttribute(string typeName)
        {
        }
    }
}
