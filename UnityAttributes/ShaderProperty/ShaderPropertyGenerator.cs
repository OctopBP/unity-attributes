using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGeneration.Utils.CodeAnalysisExtensions;
using SourceGeneration.Utils.CodeBuilder;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.ShaderProperty;

[Generator]
public sealed class ShaderPropertyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (syntaxContext, token) => GetSemanticTargetForGeneration(syntaxContext, token))
            .Collect()
            .SelectMany(static (array, _) => array.Collect());

        context.RegisterPostInitializationOutput(i =>
        {
            i.AddSource($"{ShaderPropertyType.EnumName}.g", ShaderPropertyType.EnumText);
            i.AddSource($"{ShaderPropertyMode.EnumName}.g", ShaderPropertyMode.EnumText);
            i.AddSource($"{ShaderPropertyAttribute.AttributeFullName}.g", ShaderPropertyAttribute.AttributeText);
        });

        context.RegisterSourceOutput(classes, GenerateCode!);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }

    private static Optional<ClassToProcess> GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax) ctx.Node;
        var classDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, token);
        if (classDeclarationSymbol is not ITypeSymbol classTypeSymbol)
        {
            return OptionalExt.None<ClassToProcess>();
        }

        var attributes = classDeclarationSyntax.AllAttributesWithName(ShaderPropertyAttribute.AttributeName);
        if (attributes.Count == 0)
        {
            return OptionalExt.None<ClassToProcess>();
        }

        var properties = new List<PropertyToProcess>();
        foreach (var attributeSyntax in attributes)
        {
            if (attributeSyntax.ArgumentList is not { Arguments.Count: >= 2 })
            {
                continue;
            }

            string? propertyName = null;
            string? propertyType = null;
            var mode = "Default";
            var isArray = false;
            var count = 1;
            var startIndex = 1;

            var arguments = attributeSyntax.ArgumentList.Arguments;
            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];

                // A named argument may sit anywhere, but an unnamed one is always at its own position:
                // C# only allows a named argument to precede a positional one when it is in its own slot.
                var parameterName = argument.NameColon?.Name.Identifier.Text ?? PositionalParameterName(i);

                switch (parameterName)
                {
                    case "name" when argument.Expression is LiteralExpressionSyntax nameLiteral:
                        propertyName = nameLiteral.Token.ValueText;
                        break;
                    // Enums are read syntactically, so both `ShaderPropertyType.Float` and the fully
                    // qualified form come out as the bare member name.
                    case "type" when argument.Expression is MemberAccessExpressionSyntax typeExpr:
                        propertyType = typeExpr.Name.Identifier.Text;
                        break;
                    case "mode" when argument.Expression is MemberAccessExpressionSyntax modeExpr:
                        mode = modeExpr.Name.Identifier.Text;
                        break;
                    // count is a group size; 0 (the attribute default) or less means "not specified",
                    // so a single property is generated exactly as it was before groups existed.
                    case "count" when TryGetIntValue(ctx.SemanticModel, argument.Expression, token, out var countValue):
                        isArray = countValue > 0;
                        count = countValue;
                        break;
                    case "startIndex" when TryGetIntValue(ctx.SemanticModel, argument.Expression, token, out var startValue):
                        startIndex = startValue;
                        break;
                }
            }

            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(propertyType))
            {
                continue;
            }

            properties.Add(new PropertyToProcess(propertyName!, propertyType!, mode, isArray, count, startIndex));
        }

        if (properties.Count == 0)
        {
            return OptionalExt.None<ClassToProcess>();
        }

        return new ClassToProcess(classTypeSymbol, properties);
    }

    private static string? PositionalParameterName(int index)
    {
        return index switch
        {
            0 => "name",
            1 => "type",
            2 => "mode",
            3 => "count",
            4 => "startIndex",
            _ => null
        };
    }

    private static bool TryGetIntValue(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken token, out int value)
    {
        var constant = semanticModel.GetConstantValue(expression, token);
        if (constant is { HasValue: true, Value: int intValue })
        {
            value = intValue;
            return true;
        }

        value = 0;
        return false;
    }

    private static void GenerateCode(SourceProductionContext context, ClassToProcess classToProcess)
    {
        var code = GenerateCode(classToProcess);
        context.AddSource($"{classToProcess.FullCsharpName}.g", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(ClassToProcess classToProcess)
    {
        var builder = new CodeBuilder();

        builder.AppendLineWithIdent(Const.AutoGeneratedText);
        builder.AppendLine();

        builder.AppendLine("using UnityEngine;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine();

        using (new NamespaceBlock(builder, classToProcess.ClassSymbol))
        {
            using (new ParentsBlock(builder, classToProcess.ClassSymbol))
            {
                builder.AppendIdent().Append("public partial class ").Append(classToProcess.ClassSymbol.Name).AppendLine();
                using (new BracketsBlock(builder))
                {
                    foreach (var property in classToProcess.Properties)
                    {
                        GeneratePropertyCode(builder, property);
                    }
                }
            }
        }

        return builder.ToString();
    }

    private static void GeneratePropertyCode(CodeBuilder builder, PropertyToProcess property)
    {
        if (!property.IsArray)
        {
            GenerateSinglePropertyCode(builder, property, property.Name);
            return;
        }

        // "_Fill_" with count: 5 expands into _Fill_1 .. _Fill_5, an array aggregating their ids
        // and a set of ...At methods addressing that array by index.
        var elementNames = new string[property.Count];
        for (var i = 0; i < property.Count; i++)
        {
            var shaderName = property.Name + (property.StartIndex + i);
            elementNames[i] = shaderName.ToPascalCase();
            GenerateSinglePropertyCode(builder, property, shaderName);
        }

        var arrayName = property.Name.ToPascalCase();
        GenerateIdsArray(builder, arrayName, elementNames);
        GenerateMethods(builder, property, Emit.Indexed(arrayName));
    }

    private static void GenerateSinglePropertyCode(CodeBuilder builder, PropertyToProcess property, string shaderName)
    {
        var propertyName = shaderName.ToPascalCase();

        builder.AppendLine();
        builder.AppendIdent().Append("public static readonly int ").Append(propertyName).Append(" = Shader.PropertyToID(\"").Append(shaderName).Append("\");");
        builder.AppendLine();

        GenerateMethods(builder, property, Emit.Single(propertyName));
    }

    // Emitted after the element fields: static field initializers run in declaration order.
    private static void GenerateIdsArray(CodeBuilder builder, string arrayName, string[] elementNames)
    {
        builder.AppendLine();
        builder.AppendIdent().Append("public static readonly int[] ").Append(arrayName).Append(" =");
        builder.AppendLine();
        using (new BracketsBlock(builder, withSemicolon: true))
        {
            foreach (var elementName in elementNames)
            {
                builder.AppendIdent().Append(elementName).Append(",");
                builder.AppendLine();
            }
        }
    }

    /// <summary>
    /// Describes what a batch of methods addresses: a single property id (<see cref="Single"/>) or an element of
    /// the generated id array (<see cref="Indexed"/>), in which case every method takes an extra index parameter
    /// and its name gets the "At" postfix.
    /// </summary>
    private readonly struct Emit
    {
        private readonly string _name;
        private readonly string _postfix;
        private readonly string? _indexParameter;

        private Emit(string name, string id, string postfix, string? indexParameter)
        {
            _name = name;
            _postfix = postfix;
            _indexParameter = indexParameter;
            Id = id;
        }

        public static Emit Single(string propertyName) => new(propertyName, propertyName, "", null);

        public static Emit Indexed(string arrayName) => new(arrayName, arrayName + "[index]", "At", "int index");

        /// <summary>Expression the Unity call is made with, e.g. "Fill1" or "Fill[index]".</summary>
        public string Id { get; }

        public string Method(string verb, string namePostfix = "") => verb + _name + namePostfix + _postfix;

        /// <summary>Builds a parameter list, placing the index right after <paramref name="target"/>.</summary>
        public string Params(string? target, params string[] values)
        {
            var parameters = new List<string>();
            if (!string.IsNullOrEmpty(target))
            {
                parameters.Add(target!);
            }

            if (_indexParameter != null)
            {
                parameters.Add(_indexParameter);
            }

            parameters.AddRange(values);
            return string.Join(", ", parameters);
        }
    }

    private static void EmitMethod(CodeBuilder builder, string signature, string body)
    {
        builder.AppendLine();
        builder.AppendIdent().Append(signature);
        builder.AppendLine();
        using (new BracketsBlock(builder))
        {
            builder.AppendIdent().Append(body);
            builder.AppendLine();
        }
    }

    private static void GenerateMethods(CodeBuilder builder, PropertyToProcess property, Emit p)
    {
        var type = property.Type;
        var mode = property.Mode;

        // Handle Compute mode separately
        if (mode == "Compute")
        {
            GenerateComputeMethods(builder, property, p);
            return;
        }

        switch (type)
        {
            case "Float":
                GenerateFloatMethods(builder, p, mode);
                break;
            case "Integer":
                GenerateIntegerMethods(builder, p, mode);
                break;
            case "Bool":
                GenerateBoolMethods(builder, p, mode);
                break;
            case "Color":
                GenerateColorMethods(builder, p, mode);
                break;
            case "Vector":
                GenerateVectorMethods(builder, p, mode);
                break;
            case "Matrix":
                GenerateMatrixMethods(builder, p, mode);
                break;
            case "Texture":
                GenerateTextureMethods(builder, p, mode);
                break;
            case "Buffer":
                GenerateBufferMethods(builder, p, mode);
                break;
            case "ConstantBuffer":
                GenerateConstantBufferMethods(builder, p, mode);
                break;
            case "FloatArray":
                GenerateValueArrayMethods(builder, p, mode, "float", "FloatArray");
                break;
            case "ColorArray":
                GenerateValueArrayMethods(builder, p, mode, "Color", "ColorArray");
                break;
            case "VectorArray":
                GenerateValueArrayMethods(builder, p, mode, "Vector4", "VectorArray");
                break;
            case "MatrixArray":
                GenerateValueArrayMethods(builder, p, mode, "Matrix4x4", "MatrixArray");
                break;
        }
    }

    private static void GenerateFloatMethods(CodeBuilder builder, Emit p, string mode)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "float value")})", $"Shader.SetGlobalFloat({p.Id}, value);");
            EmitMethod(builder, $"public static float {p.Method("Get")}({p.Params(null)})", $"return Shader.GetGlobalFloat({p.Id});");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "float value")})", $"propertyBlock.SetFloat({p.Id}, value);");
            EmitMethod(builder, $"public static float {p.Method("Get")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.GetFloat({p.Id});");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "float value")})", $"material.SetFloat({p.Id}, value);");
            EmitMethod(builder, $"public static float {p.Method("Get")}({p.Params("Material material")})", $"return material.GetFloat({p.Id});");
        }
    }

    private static void GenerateBoolMethods(CodeBuilder builder, Emit p, string mode)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "bool value")})", $"Shader.SetGlobalInt({p.Id}, value ? 1 : 0);");
            EmitMethod(builder, $"public static bool {p.Method("Get")}({p.Params(null)})", $"return Shader.GetGlobalInt({p.Id}) != 0;");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "bool value")})", $"propertyBlock.SetInt({p.Id}, value ? 1 : 0);");
            EmitMethod(builder, $"public static bool {p.Method("Get")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.GetInt({p.Id}) != 0;");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "bool value")})", $"material.SetInt({p.Id}, value ? 1 : 0);");
            EmitMethod(builder, $"public static bool {p.Method("Get")}({p.Params("Material material")})", $"return material.GetInt({p.Id}) != 0;");
        }
    }

    private static void GenerateIntegerMethods(CodeBuilder builder, Emit p, string mode)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "int value")})", $"Shader.SetGlobalInt({p.Id}, value);");
            EmitMethod(builder, $"public static int {p.Method("Get")}({p.Params(null)})", $"return Shader.GetGlobalInt({p.Id});");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "int value")})", $"propertyBlock.SetInt({p.Id}, value);");
            EmitMethod(builder, $"public static int {p.Method("Get")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.GetInt({p.Id});");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "int value")})", $"material.SetInteger({p.Id}, value);");
            EmitMethod(builder, $"public static int {p.Method("Get")}({p.Params("Material material")})", $"return material.GetInteger({p.Id});");
        }
    }

    private static void GenerateColorMethods(CodeBuilder builder, Emit p, string mode)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "Color value")})", $"Shader.SetGlobalColor({p.Id}, value);");
            EmitMethod(builder, $"public static Color {p.Method("Get")}({p.Params(null)})", $"return Shader.GetGlobalColor({p.Id});");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "Color value")})", $"propertyBlock.SetColor({p.Id}, value);");
            EmitMethod(builder, $"public static Color {p.Method("Get")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.GetColor({p.Id});");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "Color value")})", $"material.SetColor({p.Id}, value);");
            EmitMethod(builder, $"public static Color {p.Method("Get")}({p.Params("Material material")})", $"return material.GetColor({p.Id});");
        }
    }

    private static void GenerateVectorMethods(CodeBuilder builder, Emit p, string mode)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "Vector4 value")})", $"Shader.SetGlobalVector({p.Id}, value);");
            EmitMethod(builder, $"public static Vector4 {p.Method("Get")}({p.Params(null)})", $"return Shader.GetGlobalVector({p.Id});");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "Vector4 value")})", $"propertyBlock.SetVector({p.Id}, value);");
            EmitMethod(builder, $"public static Vector4 {p.Method("Get")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.GetVector({p.Id});");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "Vector4 value")})", $"material.SetVector({p.Id}, value);");
            EmitMethod(builder, $"public static Vector4 {p.Method("Get")}({p.Params("Material material")})", $"return material.GetVector({p.Id});");
        }
    }

    private static void GenerateMatrixMethods(CodeBuilder builder, Emit p, string mode)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "Matrix4x4 value")})", $"Shader.SetGlobalMatrix({p.Id}, value);");
            EmitMethod(builder, $"public static Matrix4x4 {p.Method("Get")}({p.Params(null)})", $"return Shader.GetGlobalMatrix({p.Id});");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "Matrix4x4 value")})", $"propertyBlock.SetMatrix({p.Id}, value);");
            EmitMethod(builder, $"public static Matrix4x4 {p.Method("Get")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.GetMatrix({p.Id});");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "Matrix4x4 value")})", $"material.SetMatrix({p.Id}, value);");
            EmitMethod(builder, $"public static Matrix4x4 {p.Method("Get")}({p.Params("Material material")})", $"return material.GetMatrix({p.Id});");
        }
    }

    private static void GenerateTextureMethods(CodeBuilder builder, Emit p, string mode)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "Texture value")})", $"Shader.SetGlobalTexture({p.Id}, value);");
            EmitMethod(builder, $"public static Texture {p.Method("Get")}({p.Params(null)})", $"return Shader.GetGlobalTexture({p.Id});");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "Texture value")})", $"propertyBlock.SetTexture({p.Id}, value);");
            EmitMethod(builder, $"public static Texture {p.Method("Get")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.GetTexture({p.Id});");
            EmitMethod(builder, $"public static void {p.Method("Set", "Offset")}({p.Params("this MaterialPropertyBlock propertyBlock", "Vector2 value")})", $"propertyBlock.SetTextureOffset({p.Id}, value);");
            EmitMethod(builder, $"public static Vector2 {p.Method("Get", "Offset")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.GetTextureOffset({p.Id});");
            EmitMethod(builder, $"public static void {p.Method("Set", "Scale")}({p.Params("this MaterialPropertyBlock propertyBlock", "Vector2 value")})", $"propertyBlock.SetTextureScale({p.Id}, value);");
            EmitMethod(builder, $"public static Vector2 {p.Method("Get", "Scale")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.GetTextureScale({p.Id});");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "Texture value")})", $"material.SetTexture({p.Id}, value);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "RenderTexture value", "UnityEngine.Rendering.RenderTextureSubElement element")})", $"material.SetTexture({p.Id}, value, element);");
            EmitMethod(builder, $"public static Texture {p.Method("Get")}({p.Params("Material material")})", $"return material.GetTexture({p.Id});");
            EmitMethod(builder, $"public static void {p.Method("Set", "Offset")}({p.Params("Material material", "Vector2 value")})", $"material.SetTextureOffset({p.Id}, value);");
            EmitMethod(builder, $"public static Vector2 {p.Method("Get", "Offset")}({p.Params("Material material")})", $"return material.GetTextureOffset({p.Id});");
            EmitMethod(builder, $"public static void {p.Method("Set", "Scale")}({p.Params("Material material", "Vector2 value")})", $"material.SetTextureScale({p.Id}, value);");
            EmitMethod(builder, $"public static Vector2 {p.Method("Get", "Scale")}({p.Params("Material material")})", $"return material.GetTextureScale({p.Id});");
        }
    }

    private static void GenerateBufferMethods(CodeBuilder builder, Emit p, string mode)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "ComputeBuffer value")})", $"Shader.SetGlobalBuffer({p.Id}, value);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "GraphicsBuffer value")})", $"Shader.SetGlobalBuffer({p.Id}, value);");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "ComputeBuffer value")})", $"propertyBlock.SetBuffer({p.Id}, value);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "GraphicsBuffer value")})", $"propertyBlock.SetBuffer({p.Id}, value);");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "ComputeBuffer value")})", $"material.SetBuffer({p.Id}, value);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "GraphicsBuffer value")})", $"material.SetBuffer({p.Id}, value);");
        }
    }

    private static void GenerateConstantBufferMethods(CodeBuilder builder, Emit p, string mode)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "ComputeBuffer value", "int offset", "int size")})", $"Shader.SetGlobalConstantBuffer({p.Id}, value, offset, size);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, "GraphicsBuffer value", "int offset", "int size")})", $"Shader.SetGlobalConstantBuffer({p.Id}, value, offset, size);");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "ComputeBuffer value", "int offset", "int size")})", $"propertyBlock.SetConstantBuffer({p.Id}, value, offset, size);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", "GraphicsBuffer value", "int offset", "int size")})", $"propertyBlock.SetConstantBuffer({p.Id}, value, offset, size);");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "ComputeBuffer value", "int offset", "int size")})", $"material.SetConstantBuffer({p.Id}, value, offset, size);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", "GraphicsBuffer value", "int offset", "int size")})", $"material.SetConstantBuffer({p.Id}, value, offset, size);");
        }
    }

    /// <param name="valueType">Element type of the value array, e.g. "float".</param>
    /// <param name="unityPostfix">Postfix of the Unity call, e.g. "FloatArray" for Set/GetFloatArray.</param>
    private static void GenerateValueArrayMethods(CodeBuilder builder, Emit p, string mode, string valueType, string unityPostfix)
    {
        if (mode == "Global")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, $"List<{valueType}> values")})", $"Shader.SetGlobal{unityPostfix}({p.Id}, values);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params(null, $"{valueType}[] values")})", $"Shader.SetGlobal{unityPostfix}({p.Id}, values);");
            EmitMethod(builder, $"public static {valueType}[] {p.Method("Get")}({p.Params(null)})", $"return Shader.GetGlobal{unityPostfix}({p.Id});");
        }
        else if (mode == "WithPropertyBlock")
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", $"List<{valueType}> values")})", $"propertyBlock.Set{unityPostfix}({p.Id}, values);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("this MaterialPropertyBlock propertyBlock", $"{valueType}[] values")})", $"propertyBlock.Set{unityPostfix}({p.Id}, values);");
            EmitMethod(builder, $"public static {valueType}[] {p.Method("Get")}({p.Params("this MaterialPropertyBlock propertyBlock")})", $"return propertyBlock.Get{unityPostfix}({p.Id});");
        }
        else // Default
        {
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", $"List<{valueType}> values")})", $"material.Set{unityPostfix}({p.Id}, values);");
            EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("Material material", $"{valueType}[] values")})", $"material.Set{unityPostfix}({p.Id}, values);");
            EmitMethod(builder, $"public static {valueType}[] {p.Method("Get")}({p.Params("Material material")})", $"return material.Get{unityPostfix}({p.Id});");
        }
    }

    private static void GenerateComputeMethods(CodeBuilder builder, PropertyToProcess property, Emit p)
    {
        var type = property.Type;

        switch (type)
        {
            case "Float":
                GenerateComputeFloatMethods(builder, p);
                break;
            case "Integer":
                GenerateComputeIntegerMethods(builder, p);
                break;
            case "Bool":
                GenerateComputeBoolMethods(builder, p);
                break;
            case "Color":
            case "Vector":
                GenerateComputeVectorMethods(builder, p);
                break;
            case "Matrix":
                GenerateComputeMatrixMethods(builder, p);
                break;
            case "Texture":
                GenerateComputeTextureMethods(builder, p);
                break;
            case "Buffer":
                GenerateComputeBufferMethods(builder, p);
                break;
            case "ConstantBuffer":
                GenerateComputeConstantBufferMethods(builder, p);
                break;
            // Array types are skipped for Compute mode
        }
    }

    private static void GenerateComputeFloatMethods(CodeBuilder builder, Emit p)
    {
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader", "float value")})", $"computeShader.SetFloat({p.Id}, value);");
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader", "params float[] values")})", $"computeShader.SetFloats({p.Id}, values);");
    }

    private static void GenerateComputeIntegerMethods(CodeBuilder builder, Emit p)
    {
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader", "int value")})", $"computeShader.SetInt({p.Id}, value);");
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader", "params int[] values")})", $"computeShader.SetInts({p.Id}, values);");
    }

    private static void GenerateComputeBoolMethods(CodeBuilder builder, Emit p)
    {
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader", "bool value")})", $"computeShader.SetBool({p.Id}, value);");
    }

    private static void GenerateComputeVectorMethods(CodeBuilder builder, Emit p)
    {
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader", "Vector4 value")})", $"computeShader.SetVector({p.Id}, value);");
    }

    private static void GenerateComputeMatrixMethods(CodeBuilder builder, Emit p)
    {
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader", "Matrix4x4 value")})", $"computeShader.SetMatrix({p.Id}, value);");
    }

    private static void GenerateComputeTextureMethods(CodeBuilder builder, Emit p)
    {
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader, int kernelIndex", "Texture texture")})", $"computeShader.SetTexture(kernelIndex, {p.Id}, texture);");
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader, int kernelIndex", "Texture texture", "int mipLevel")})", $"computeShader.SetTexture(kernelIndex, {p.Id}, texture, mipLevel);");
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader, int kernelIndex", "RenderTexture texture", "int mipLevel", "UnityEngine.Rendering.RenderTextureSubElement element")})", $"computeShader.SetTexture(kernelIndex, {p.Id}, texture, mipLevel, element);");
        EmitMethod(builder, $"public static void {p.Method("Set", "FromGlobal")}({p.Params("ComputeShader computeShader, int kernelIndex", "int globalTextureNameID")})", $"computeShader.SetTextureFromGlobal(kernelIndex, {p.Id}, globalTextureNameID);");
    }

    private static void GenerateComputeBufferMethods(CodeBuilder builder, Emit p)
    {
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader, int kernelIndex", "ComputeBuffer buffer")})", $"computeShader.SetBuffer(kernelIndex, {p.Id}, buffer);");
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader, int kernelIndex", "GraphicsBuffer buffer")})", $"computeShader.SetBuffer(kernelIndex, {p.Id}, buffer);");
    }

    private static void GenerateComputeConstantBufferMethods(CodeBuilder builder, Emit p)
    {
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader", "ComputeBuffer buffer", "int offset", "int size")})", $"computeShader.SetConstantBuffer({p.Id}, buffer, offset, size);");
        EmitMethod(builder, $"public static void {p.Method("Set")}({p.Params("ComputeShader computeShader", "GraphicsBuffer buffer", "int offset", "int size")})", $"computeShader.SetConstantBuffer({p.Id}, buffer, offset, size);");
    }
}
