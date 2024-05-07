using System.Runtime.CompilerServices;

namespace UnityAttributes.Test;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Enable();
    }
}