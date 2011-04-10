using System;

using MbUnit.Framework;

using Rhino.Mocks;

namespace NAntExtensions.ForTesting
{
	public abstract class TypeSpec : AbstractSpec
	{
		readonly MockRepository _staticMocks;

		protected TypeSpec(MockRepository staticMocks)
		{
			if (staticMocks == null)
			{
				throw new ArgumentNullException("staticMocks");
			}

			_staticMocks = staticMocks;
		}

		protected override MockRepository StaticMocks
		{
			get { return _staticMocks; }
		}

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
		public void SetUp(object systemUnderTest)
		{
			base.SetUp();

			Before_each_spec(systemUnderTest);
		}

		[TearDown]
		public void TearDown(object systemUnderTest)
		{
			base.TearDown();

			After_each_spec(systemUnderTest);
		}

		protected virtual void Before_all_specs()
		{
		}

		protected virtual void After_all_specs()
		{
		}

		protected virtual void Before_each_spec(object systemUnderTest)
		{
		}

		protected virtual void After_each_spec(object systemUnderTest)
		{
		}
	}
}