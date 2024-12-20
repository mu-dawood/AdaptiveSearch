using System;

namespace AdaptiveSearch.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class PropertyTypeAttribute : Attribute
    {
        public Type[] Types { get; private set; }

        public PropertyTypeAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}