#nullable enable
using System;
using SourceGeneration.Utils.CodeBuilder;

namespace SourceGeneration.Utils.Common; 

public static class Utils
{
    public static string GeneratedCodeAttribute(this Type type)
    {
        var assemblyName = type.Assembly.GetName();
        return $"""[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{assemblyName.Name}", "{assemblyName.Version}")]""";
    }

    public static string SimpleAttribute(string name, AttributeTargets target, bool allowMultiple = true)
    {
        return Attribute(name, null, target, allowMultiple, []);
    }
    
    public static string SimpleAttribute(string name, Type type, AttributeTargets target, bool allowMultiple = true)
    {
        return Attribute(name, type, target, allowMultiple, []);
    }
    
    public static string Attribute(
        string name, Type? type, AttributeTargets target, bool allowMultiple, (string type, string name, string? @default)[] fields
    )
    {
        var builder = new CodeBuilder.CodeBuilder();

        builder.AppendLineWithIdent("/// <auto-generated />");
        builder.AppendLine();
        
        using (new NamespaceBlock(builder, "EnumExt"))
        {
            if (type != null)
            {
                builder.AppendLineWithIdent(type.GeneratedCodeAttribute());
            }

            builder.AppendIdent().Append("[global::System.AttributeUsage(global::System.AttributeTargets.")
                .Append(target.ToString()).Append(", Inherited = true, AllowMultiple = ")
                .Append(allowMultiple ? "true" : "false").Append(")]").AppendLine();

            builder.AppendIdent().Append("internal sealed class ").Append(name.WithAttributePostfix())
                .Append(" : global::System.Attribute").AppendLine();

            using (new BracketsBlock(builder))
            {
                foreach (var f in fields)
                {
                    builder.AppendIdent().Append("private ").Append(f.type).Append(" _").Append(f.name).Append(";")
                        .AppendLine();
                }
                
                if (fields.Length > 0)
                {
                    builder.AppendLine();
                }
                
                builder.AppendIdent().Append("internal ").Append(name.WithAttributePostfix()).Append("(");
                builder.AppendArray(fields, (f, b) =>
                {
                    b.Append(f.type).Append(" ").Append(f.name);
                    if (f.@default != null)
                    {
                        b.Append(" = ").Append(f.@default); 
                    }
                }, b => b.Append(", "));
                builder.Append(")").AppendLine();

                using (new BracketsBlock(builder))
                {
                    foreach (var f in fields)
                    {
                        builder.AppendIdent().Append("_").Append(f.name).Append(" = ").Append(f.name).Append(";")
                            .AppendLine();
                    }
                }
            }
        }

        return builder.ToString();
    }
}