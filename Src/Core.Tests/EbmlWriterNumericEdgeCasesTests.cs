/* Copyright (c) 2011-2025 Oleg Zee

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
using NUnit.Framework;

namespace Core.Tests
{
	[TestFixture]
	public class EbmlWriterNumericEdgeCasesTests : EbmlWriterTestBase
	{
		#region Integer Edge Cases Tests

		[TestCase(0L)]
		[TestCase(1L)]
		[TestCase(-1L)]
		[TestCase(127L)]
		[TestCase(128L)]
		[TestCase(-128L)]
		[TestCase(-129L)]
		[TestCase(32767L)]
		[TestCase(32768L)]
		[TestCase(-32768L)]
		[TestCase(-32769L)]
		[TestCase(long.MaxValue)]
		[TestCase(long.MinValue)]
		public void WriteInt64_EdgeCases_RoundTrip(long value)
		{
			_writer.Write(ElementId, value);

			var reader = StartRead();
			var result = reader.ReadInt();

			Assert.AreEqual(value, result);
		}

		[TestCase(0UL)]
		[TestCase(1UL)]
		[TestCase(255UL)]
		[TestCase(256UL)]
		[TestCase(65535UL)]
		[TestCase(65536UL)]
		[TestCase(4294967295UL)]
		[TestCase(4294967296UL)]
		[TestCase(ulong.MaxValue)]
		public void WriteUInt64_EdgeCases_RoundTrip(ulong value)
		{
			_writer.Write(ElementId, value);

			var reader = StartRead();
			var result = reader.ReadUInt();

			Assert.AreEqual(value, result);
		}

		#endregion

		#region Float Special Values Tests

		[TestCase(0.0f)]
		[TestCase(-0.0f)]
		[TestCase(1.0f)]
		[TestCase(-1.0f)]
		[TestCase(float.Epsilon)]
		[TestCase(-float.Epsilon)]
		[TestCase(float.MaxValue)]
		[TestCase(float.MinValue)]
		[TestCase(float.PositiveInfinity)]
		[TestCase(float.NegativeInfinity)]
		public void WriteFloat_SpecialValues_RoundTrip(float value)
		{
			_writer.Write(ElementId, value);

			var reader = StartRead();
			var result = reader.ReadFloat();

			Assert.AreEqual(value, result);
		}

		[Test]
		public void WriteFloat_NaN_RoundTrip()
		{
			_writer.Write(ElementId, float.NaN);

			var reader = StartRead();
			var result = reader.ReadFloat();

			Assert.IsTrue(double.IsNaN(result));
		}

		[TestCase(0.0)]
		[TestCase(-0.0)]
		[TestCase(1.0)]
		[TestCase(-1.0)]
		[TestCase(double.Epsilon)]
		[TestCase(-double.Epsilon)]
		[TestCase(double.MaxValue)]
		[TestCase(double.MinValue)]
		[TestCase(double.PositiveInfinity)]
		[TestCase(double.NegativeInfinity)]
		public void WriteDouble_SpecialValues_RoundTrip(double value)
		{
			_writer.Write(ElementId, value);

			var reader = StartRead();
			var result = reader.ReadFloat();

			Assert.AreEqual(value, result);
		}

		[Test]
		public void WriteDouble_NaN_RoundTrip()
		{
			_writer.Write(ElementId, double.NaN);

			var reader = StartRead();
			var result = reader.ReadFloat();

			Assert.IsTrue(double.IsNaN(result));
		}

		#endregion

		#region DateTime Edge Cases Tests

		[Test]
		public void WriteDateTime_MinReasonableValue_RoundTrip()
		{
			// EBML DateTime is relative to millennium start, so very old dates might not work
			var date = new DateTime(1901, 1, 1);

			_writer.Write(ElementId, date);

			var reader = StartRead();
			var result = reader.ReadDate();

			Assert.AreEqual(date, result);
		}

		[Test]
		public void WriteDateTime_MaxReasonableValue_RoundTrip()
		{
			var date = new DateTime(2101, 12, 31, 23, 59, 59, 999);

			_writer.Write(ElementId, date);

			var reader = StartRead();
			var result = reader.ReadDate();

			Assert.AreEqual(date, result);
		}

		[Test]
		public void WriteDateTime_MilleniumStart_RoundTrip()
		{
			// Test the exact millennium start reference point
			var date = new DateTime(2001, 1, 1);

			_writer.Write(ElementId, date);

			var reader = StartRead();
			var result = reader.ReadDate();

			Assert.AreEqual(date, result);
		}

		#endregion
	}
}
