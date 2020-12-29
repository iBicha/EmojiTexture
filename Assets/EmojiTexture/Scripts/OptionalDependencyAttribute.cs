using System;
using System.Diagnostics;

[Conditional("UNITY_CCU")]                                    // | This is necessary for CCU to pick up the right attributes
public class OptionalDependencyAttribute : Attribute        // | Must derive from System.Attribute
{
    public string dependentClass;                           // | Required field specifying the fully qualified dependent class
    public string define;                                   // | Required field specifying the define to add

    public OptionalDependencyAttribute(string dependentClass, string define)
    {
        this.dependentClass = dependentClass;
        this.define = define;
    }
}
