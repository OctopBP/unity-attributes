namespace PublicAccessor.Example;

public partial class PublicAccessorGeneratorExample {
	[PublicAccessor] string test1;
	[PublicAccessor] int test2;
	[PublicAccessor] int Test3;
	[PublicAccessor] int _test4;
	[PublicAccessor] public int Test5;
	[PublicAccessor] static int Test6;
	[PublicAccessor] bool _test7, _test8;
}

public partial class Test {
	[PublicAccessor] string test1;
}

public class Test2 {
	public class NestedTest {
		[PublicAccessor] string test;
	}
}