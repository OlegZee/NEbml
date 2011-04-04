#if NUNIT

using System;
using NUnit.Framework;

namespace NEbml.Core.Tests
{
	/// <summary>
	/// Unit tests for VInt class
	/// </summary>
	[TestFixture]
	public class VIntTests
	{
		[TestCase(0, 1, Result = 0x80)]
		[TestCase(1, 1, Result = 0x81)]
		[TestCase(126, 1, Result = 0xfe)]
		[TestCase(127, 2, Result = 0x407f)]
		[TestCase(128, 2, Result = 0x4080)]
		[TestCase(0xdeffad, 4, Result = 0x10deffad)]
		public ulong EncodeSize(int value, int expectedLength)
		{
			var v = VInt.EncodeSize((ulong)value);
			Assert.AreEqual(expectedLength, v.Length);

			return v.EncodedValue;
		}

		[TestCase(0, 1, Result = 0x80)]
		[TestCase(0, 2, Result = 0x4000)]
		[TestCase(0, 3, Result = 0x200000)]
		[TestCase(0, 4, Result = 0x10000000)]
		[TestCase(127, 1, ExpectedException = typeof(ArgumentException))]
		[TestCase(127, 2, Result = 0x407f)]
		public ulong EncodeSizeWithLength(int value, int length)
		{
			var v = VInt.EncodeSize((ulong)value, length);
			Assert.AreEqual(length, v.Length);
			return v.EncodedValue;
		}

		[TestCase(-1, ExpectedException = typeof(ArgumentOutOfRangeException))]
		[TestCase(0, ExpectedException = typeof(ArgumentOutOfRangeException))]
		[TestCase(1, Result = 0xffL)]
		[TestCase(2, Result = 0x7fffL)]
		[TestCase(3, Result = 0x3fffffL)]
		[TestCase(4, Result = 0x1fffffffL)]
		[TestCase(5, Result = 0x0fffffffffL)]
		[TestCase(6, Result = 0x07ffffffffffL)]
		[TestCase(7, Result = 0x03ffffffffffffL)]
		[TestCase(8, Result = 0x01ffffffffffffffL)]
		[TestCase(9, ExpectedException = typeof(ArgumentOutOfRangeException))]
		public ulong CreatesReserved(int length)
		{
			var size = VInt.UnknownSize(length);

			Assert.AreEqual(length, size.Length);
			Assert.IsTrue(size.IsReserved);

			return size.EncodedValue;
		}

		[TestCase(0ul, ExpectedException = typeof(ArgumentException))]
		[TestCase(1ul, ExpectedException = typeof(ArgumentException))]
		[TestCase(0x80ul, Result = 0)]
		[TestCase(0xaful, Result = 0x2f)]
		[TestCase(0x40ul, ExpectedException = typeof(ArgumentException))]
		[TestCase(0x20ul, ExpectedException = typeof(ArgumentException))]
		[TestCase(0x10ul, ExpectedException = typeof(ArgumentException))]
		[TestCase(0x8000ul, ExpectedException = typeof(ArgumentException))]
		[TestCase(0x40FFul, Result = 0xFF)]
		[TestCase(0x2000FFul, Result = 0xFF)]
		[TestCase(0x100000FFul, Result = 0xFF)]
		[TestCase(0x1f1020FFul, Result = 0xF1020FF)]
		public ulong CreatesFromEncodedValue(ulong encodedValue)
		{
			return VInt.FromEncoded(encodedValue).Value;
		}

		[TestCase(0ul, 1)]
		[TestCase(126ul, 1)]
		[TestCase(127ul, 2)]
		[TestCase(128ul, 2)]
		[TestCase(0xFFFFul, 3)]
		[TestCase(0xFFffFFul, 4)]
		public void CreatesSizeOrIdFromEncodedValue(ulong value, int expectedLength)
		{
			var v = VInt.EncodeSize(value);
			Assert.IsFalse(v.IsReserved);
			Assert.AreEqual(value, v.Value);
			Assert.AreEqual(expectedLength, v.Length);
		}

		[TestCase(0x80ul, Result = true)]
		[TestCase(0x81ul, Result = true)]
		[TestCase(0x4001ul, Result = false, Description = "Allows shorter form")]
		[TestCase(0xfful, Result = false, Description = "Reserved value")]
		[TestCase(0x7ffful, Result = false, Description = "Reserved value")]
		public bool ValidIdentifiers(ulong encodedValue)
		{
			return VInt.FromEncoded(encodedValue).IsValidIdentifier;
		}

	}
}

#endif