using System;

namespace UnityAttributes.Common;

public static class StringExtensions
{
    public static string firstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToUpper() + input.Substring(1)
        };
    
    public static string upperFirstCharOrAddUnderline(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0] == '_' && input.Length > 1 && !char.IsUpper(input[1])
                ? upperFirstCharOrAddUnderline(input.Substring(1))
                : char.IsUpper(input[0]) ? "_" + input : firstCharToUpper(input)
        };
}