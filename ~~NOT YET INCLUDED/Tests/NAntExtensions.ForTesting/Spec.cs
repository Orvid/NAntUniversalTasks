using MbUnit.Framework;

namespace NAntExtensions.ForTesting
{
	[TestFixture]
	public abstract class Spec : AbstractSpec
	{
		[TestFixtureSetUp]
		public virtual void TestFixtureSetUp()
		{
			Before_all_specs();
		}

		[TestFixtureTearDown]
		public virtual void TestFixtureTearDown()
		{
			After_all_specs();
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			Before_each_spec();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();

			After_each_spec();
		}

		protected virtual void Before_all_specs()
		{
		}

		protected virtual void After_all_specs()
		{
		}

		protected virtual void Before_each_spec()
		{
		}

		protected virtual void After_each_spec()
		{
		}
	}
}