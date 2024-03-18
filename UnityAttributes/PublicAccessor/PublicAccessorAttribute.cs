namespace UnityAttributes.PublicAccessor;

public static class PublicAccessorAttribute
{
    public const string ATTRIBUTE_SHORT_NAME = "PublicAccessor";
    public const string ATTRIBUTE_FULL_NAME = ATTRIBUTE_SHORT_NAME + "Attribute";

    public const string ATTRIBUTE_TEXT = 
        $$"""
          /// <auto-generated />

          [global::System.AttributeUsage(global::System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
          internal sealed class {{ATTRIBUTE_FULL_NAME}} : global::System.Attribute
          {
              public {{ATTRIBUTE_FULL_NAME}}() { }
          }
          """;
}