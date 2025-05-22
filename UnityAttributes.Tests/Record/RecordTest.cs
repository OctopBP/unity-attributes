namespace UnityAttributes.Test.Record;

[UsesVerify]
public class RecordTests
{
    [Fact]
    public Task GenerateClass()
    {
        const string source =
            """
            public class Test
            {{
                public int Value;
            }}
        
            public enum SomeEnum
            {{
                case One, case Two
            }}
        
            [Record]
            public partial class TestClass
            {{
                public int _intValue;
                private SomeEnum _enumValue;
                private Test _classValue;
            }}
            """;
        
        return TestHelper.Verify<UnityAttributes.Record.RecordGenerator>(source, "Record/Tests");
    }
}