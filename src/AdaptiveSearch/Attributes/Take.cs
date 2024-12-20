using System;

namespace AdaptiveSearch.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    [PropertyType(typeof(int))]
    public class TakeAttribute : Attribute
    {
    }
}