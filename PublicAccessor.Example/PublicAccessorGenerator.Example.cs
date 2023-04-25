namespace PublicAccessor.Example;

public partial class PublicAccessorGeneratorExample {
	[PublicAccessor] string test1;
	[PublicAccessor] int test2;
	[PublicAccessor] int Test3;
	[PublicAccessor] int _test4;
	[PublicAccessor] public int Test5;
	[PublicAccessor] static int Test6;
	[PublicAccessor] bool _test7, _test8, _test9;
}

public partial class Test {
	[PublicAccessor] string test1;
}

public partial class Test3 {
	[PublicAccessor] string test1;
	// [PublicAccessor] string test2;
}

// [Record]
public partial class RecordTest {
	public readonly string test1;
	public readonly int test2;
	int test3;
}


public partial class Test2 {
	public partial class NestedTest {
		[PublicAccessor] string test;
	}
}