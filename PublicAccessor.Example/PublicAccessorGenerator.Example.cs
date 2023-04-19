namespace PublicAccessor.Example; 

public partial class PublicAccessorGeneratorExample {
	[PublicAccessor] string test1;
	[PublicAccessor] int test2;
	[PublicAccessor] int Test3;
	[PublicAccessor] int _test4;
	[PublicAccessor] public int Test5;
	[PublicAccessor] static int Test6;
}

public class Test {
	PublicAccessorGeneratorExample publicAccessorGeneratorExample;

	void test() {
		var a = publicAccessorGeneratorExample.Test5;
		publicAccessorGeneratorExample.Test();
	}
}