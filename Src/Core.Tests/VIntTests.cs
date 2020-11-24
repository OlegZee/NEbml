/* Copyright (c) 2011-2020 Oleg Zee

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * */

using System;
using NEbml.Core;
using NUnit.Framework;

namespace Core.Tests
{
	/// <summary>
	/// Unit tests for VInt class
	/// </summary>
	[TestFixture]
	public class VIntTests
	{
		[TestCase(0, 1, ExpectedResult = 0x80ul)]
		[TestCase(1, 1, ExpectedResult = 0x81ul)]
		[TestCase(126, 1, ExpectedResult = 0xfeul)]
		[TestCase(127, 2, ExpectedResult = 0x407ful)]
		[TestCase(128, 2, ExpectedResult = 0x4080ul)]
		[TestCase(0xdeffad, 4, ExpectedResult = 0x10deffadul)]
		public ulong EncodeSize(int value, int expectedLength)
		{
			var v = VInt.EncodeSize((ulong)value);
			Assert.AreEqual(expectedLength, v.Length);

			return v.EncodedValue;
		}

		[TestCase(0, 1, ExpectedResult = 0x80ul)]
		[TestCase(0, 2, ExpectedResult = 0x4000ul)]
		[TestCase(0, 3, ExpectedResult = 0x200000ul)]
		[TestCase(0, 4, ExpectedResult = 0x10000000ul)]
		[TestCase(127, 2, ExpectedResult = 0x407ful)]
		public ulong EncodeSizeWithLength(int value, int length)
		{
			var v = VInt.EncodeSize((ulong)value, length);
			Assert.AreEqual(length, v.Length);
			return v.EncodedValue;
		}

		[TestCase(127, 1)]
		public void EncodeSizeWithIncorrectLength(int value, int length)
		{
			Assert.Throws<ArgumentException>(() => VInt.EncodeSize((ulong)value, length));
		}

		[TestCase(1, ExpectedResult = 0xfful)]
		[TestCase(2, ExpectedResult = 0x7ffful)]
		[TestCase(3, ExpectedResult = 0x3ffffful)]
		[TestCase(4, ExpectedResult = 0x1ffffffful)]
		[TestCase(5, ExpectedResult = 0x0ffffffffful)]
		[TestCase(6, ExpectedResult = 0x07fffffffffful)]
		[TestCase(7, ExpectedResult = 0x03fffffffffffful)]
		[TestCase(8, ExpectedResult = 0x01fffffffffffffful)]
		public ulong CreatesReserved(int length)
		{
			var size = VInt.UnknownSize(length);

			Assert.AreEqual(length, size.Length);
			Assert.IsTrue(size.IsReserved);

			return size.EncodedValue;
		}

		[TestCase(-1)]
		[TestCase(0)]
		[TestCase(9)]
		public void CreatesReservedInvalidArgs(int length)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => VInt.UnknownSize(length));
		}

		[TestCase(0x80ul, ExpectedResult = 0ul)]
		[TestCase(0xaful, ExpectedResult = 0x2ful)]
		[TestCase(0x40FFul, ExpectedResult = 0xFFul)]
		[TestCase(0x2000FFul, ExpectedResult = 0xFFul)]
		[TestCase(0x100000FFul, ExpectedResult = 0xFFul)]
		[TestCase(0x1f1020FFul, ExpectedResult = 0xF1020FFul)]
		public ulong CreatesFromEncodedValue(ulong encodedValue)
		{
			return VInt.FromEncoded(encodedValue).Value;
		}

		[TestCase(0ul)]
		[TestCase(1ul)]
		[TestCase(0x40ul)]
		[TestCase(0x20ul)]
		[TestCase(0x10ul)]
		[TestCase(0x8000ul)]
		public void CreatesFromEncodedValueInvalid(ulong encodedValue)
		{
			Assert.Throws<ArgumentException>(() => VInt.FromEncoded(encodedValue));
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

		[TestCase(0x80ul, ExpectedResult = true)]
		[TestCase(0x81ul, ExpectedResult = true)]
		[TestCase(0x4001ul, ExpectedResult = false, Description = "Allows shorter form")]
		[TestCase(0xfful, ExpectedResult = false, Description = "Reserved value")]
		[TestCase(0x7ffful, ExpectedResult = false, Description = "Reserved value")]
		public bool ValidIdentifiers(ulong encodedValue)
		{
			return VInt.FromEncoded(encodedValue).IsValidIdentifier;
		}
	}
}
