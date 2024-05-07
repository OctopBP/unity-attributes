namespace UnityAttributes.Test.EnumTypeFor;

[UsesVerify]
public class EnumTypeForTests
{
    [Fact]
    public Task GenerateClass()
    {
        const string source =
            """
            namespace Test
            {{
                public class ClassWithInt
                {{
                    public int Value;
                }}
            }}
            
            public class ClassWithString
            {{
                public string Value;
            }}
            
            public class ClassWithT<T>
            {{
                public T Value;
            }}
            
            // [EnumTypeFor(typeof(int))]
            [EnumTypeFor(typeof(Test.ClassWithInt))]
            [EnumTypeFor(typeof(ClassWithString))]
            [EnumTypeFor(typeof(ClassWithT<int>))]
            public enum ColorTest
            {
                Red,
                Blue,
                Green,
            }
            """;
        
        return TestHelper.Verify<UnityAttributes.EnumTypeFor.EnumTypeForGenerator>(source, "EnumTypeFor/Tests");
    }
}