namespace UnityAttributes.Test.PublicAccessor;

[UsesVerify]
public class PublicAccessorTests
{
    [Fact]
    public Task GenerateClass()
    {
        const string source =
            """
            namespace Test1
            {{
                public partial class TestClass
                {{
                    [PublicAccessor] private int _value1;
                    [PublicAccessor] public int value2;
                    [PublicAccessor] private readonly int Value3;
                }}
            }}
            """;
        
        return TestHelper.Verify<UnityAttributes.PublicAccessor.PublicAccessorGenerator>(source, "PublicAccessor/Tests");
    }
    
    [Fact]
    public Task GenerateNestedClass()
    {
        const string source =
            """
            namespace Test2
            {{
                public partial class TestClass
                {{
                    [PublicAccessor] private NestedClass _nestedClass;
                    
                    public partial class NestedClass
                    {{
                        [PublicAccessor] private int _value;
                    }}
                }}
            }}
            """;
        
        return TestHelper.Verify<UnityAttributes.PublicAccessor.PublicAccessorGenerator>(source, "PublicAccessor/Tests");
    }
}