using System;
using UnityEngine;

namespace HierarchicalJPS.Attribute
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute{}
}