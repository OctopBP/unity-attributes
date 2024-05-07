namespace UnityAttributes.Test.GenConstructor;

[UsesVerify]
public class GenConstructorTests
{
    [Fact]
    public Task GenerateClass()
    {
        const string source =
            """
            namespace Test1
            {{
                [GenConstructor]
                public partial class TestClass
                {{
                    private int _value1, _value2;
                    public int value3;
                    [GenConstructorIgnore] private int _value4, _value5; 
                    private readonly int Value6;
                }}
            }}
            """;
        
        return TestHelper.Verify<UnityAttributes.GenConstructor.GenConstructorGenerator>(source, "GenConstructor/Tests");
    }
}