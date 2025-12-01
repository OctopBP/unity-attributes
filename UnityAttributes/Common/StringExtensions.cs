using System;

namespace SourceGeneration.Utils.Common;

public static class StringExtensions
{
    public static string FirstCharToUpper(this string input)
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToUpper() + input.Substring(1),
        };
    }
    
    public static string FirstCharToLower(this string input)
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToLower() + input.Substring(1),
        };
    }
    
    public static string RemoveUnderlineAndFirstCharToLower(this string input)
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            "_" => "_",
            _ when input.StartsWith("_") => input[1].ToString().ToLower() + input.Substring(2),
            _ => input[0].ToString().ToLower() + input.Substring(1),
        };
    }

    public static string UpperFirstCharOrAddUnderline(this string input)
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0] == '_' && input.Length > 1 && !char.IsUpper(input[1])
                ? UpperFirstCharOrAddUnderline(input.Substring(1))
                : char.IsUpper(input[0])
                    ? "_" + input
                    : FirstCharToUpper(input),
        };
    }
    
    public static string WithAttributePostfix(this string value) => value + "Attribute";
    
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        // Remove leading underscore if present
        var processed = input.StartsWith("_") ? input.Substring(1) : input;
        
        if (string.IsNullOrEmpty(processed))
            return processed;
        
        // Split by underscore and capitalize first letter of each word
        var parts = processed.Split('_');
        var result = new System.Text.StringBuilder();
        
        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;
                
            result.Append(part[0].ToString().ToUpper());
            if (part.Length > 1)
            {
                result.Append(part.Substring(1));
            }
        }
        
        return result.ToString();
    }
}