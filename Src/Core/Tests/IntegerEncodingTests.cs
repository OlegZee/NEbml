#if NUNIT

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace NEbml.Core.Tests
{
	[TestFixture]
	public class IntegerEncodingTests
	{
		[TestCase(0, Result = 0)]
		[TestCase(1, Result = 1)]
		[TestCase(-1, Result = 1)]
		[TestCase(127, Result = 1)]
		[TestCase(128, Result = 2)]
		[TestCase(129, Result = 2)]
		[TestCase(-127, Result = 1)]
		[TestCase(-128, Result = 1)]
		[TestCase(-129, Result = 2)]
		[TestCase(0x0000000000007fffL, Result = 2)]
		[TestCase(0x0000000000008000L, Result = 3)]
		[TestCase(0x0000000000008001L, Result = 3)]
		[TestCase(-32767, Result = 2)]
		[TestCase(-32768, Result = 2)]
		[TestCase(-32769, Result = 3)]
		public int MinimumSignedInteger(long value)
		{
			return EncodeImpl.GetMinimumSignedIntegerEncodedSize(value);
		}

		[Test]
		public void MinimumUnsignedInteger()
		{
			var data = new Dictionary<ulong, int>
				{
					{0L, 0},

					{0x01L, 1},
					{0x80L, 1},
					{0xffL, 1},

					{0x0100L, 2},
					{0x8000L, 2},
					{0xffffL, 2},

					{0x010000L, 3},
					{0x800000L, 3},
					{0xffffffL, 3},

					{0x01000000L, 4},
					{0x80000000L, 4},
					{0xffffffffL, 4},

					{0x0100000000L, 5},
					{0x8000000000L, 5},
					{0xffffffffffL, 5},

					{0x010000000000L, 6},
					{0x800000000000L, 6},
					{0xffffffffffffL, 6},

					{0x01000000000000L, 7},
					{0x80000000000000L, 7},
					{0xffffffffffffffL, 7},

					{0x0100000000000000L, 8},
					{0x8000000000000000L, 8},
					{0xffffffffffffffffL, 8}
				};

			foreach (var test in data)
			{
				Assert.AreEqual(test.Value, EncodeImpl.GetMinimumUnsignedIntegerEncodedSize(test.Key));
			}
		}
	}
}

#endif