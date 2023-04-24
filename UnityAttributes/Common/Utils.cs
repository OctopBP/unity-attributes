using System;

namespace UnityAttributes.Common; 

public static class Utils {
  public static string generatedCodeAttribute(this Type type) {
    var assemblyName = type.Assembly.GetName();
    return $@"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{assemblyName.Name}"", ""{assemblyName.Version}"")]";
  }
}