using System;
using System.Threading;

using Machine.Specifications;

namespace NAntExtensions.Machine.Specifications.Example.Tests
{
	[Subject("mspec NAnt Task")]
	public class When_tests_are_run
	{
		It should_run_very_long = () => Thread.Sleep(1500);

		[Ignore]
		It should_be_ignored = () => { };

		It should_fail = () =>
			{
				Console.Out.WriteLine("Some Console.Out message");
				Console.Error.WriteLine("Some Console.Error message");
				throw new InvalidOperationException("Something bad happened");
			};

		It should_succeed_with_NUnit = () => NUnitShouldExtensionMethods.ShouldBeTrue(true);
		It should_succeed_with_xUnit = () => XUnitShouldExtensionMethods.ShouldBeTrue(true);
		It is_not_implemented;
	}
}