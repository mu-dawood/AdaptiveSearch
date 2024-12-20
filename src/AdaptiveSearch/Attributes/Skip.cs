using System;

namespace AdaptiveSearch.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [PropertyType(typeof(int))]
    public class SkipAttribute : Attribute
    {
    }
}