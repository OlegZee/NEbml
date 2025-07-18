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
using NEbml.Core;
using NUnit.Framework;

namespace Core.Tests
{
	[TestFixture]
	public class EbmlWriterBasicDataTypesTests : EbmlWriterTestBase
	{
		[TestCase(0L)]
		[TestCase(123L)]
		[TestCase(12345678L)]
		[TestCase(-1L)]
		[TestCase(Int64.MinValue)]
		[TestCase(Int64.MaxValue)]
		public void ReadWriteInt64(Int64 value)
		{
			_writer.Write(ElementId, value);

			var reader = StartRead();
			Assert.AreEqual(value, reader.ReadInt());
		}

		[TestCase(0ul)]
		[TestCase(123ul)]
		[TestCase(12345678ul)]
		[TestCase(UInt64.MinValue)]
		[TestCase(UInt64.MaxValue)]
		public void ReadWriteUInt64(UInt64 value)
		{
			_writer.Write(ElementId, value);

			var reader = StartRead();
			Assert.AreEqual(value, reader.ReadUInt());
			Assert.AreEqual(_stream.Length, _stream.Position);
		}

		[Test]
		[TestCaseSource(nameof(TestDatetimeData))]
		public void ReadWriteDateTime(DateTime value)
		{
			_writer.Write(ElementId, value);

			var reader = StartRead();
			Assert.AreEqual(value, reader.ReadDate());
		}

		[TestCase(0f)]
		[TestCase(-1f)]
		[TestCase(1f)]
		[TestCase(1.12345f)]
		[TestCase(-1.12345e+23f)]
		[TestCase(float.MinValue)]
		[TestCase(float.MaxValue)]
		[TestCase(float.NaN)]
		[TestCase(float.NegativeInfinity)]
		public void ReadWriteFloat(float value)
		{
			_writer.Write(ElementId, value);

			var reader = StartRead();
			Assert.AreEqual(value, reader.ReadFloat());
		}

		[TestCase(0.0)]
		[TestCase(-1.0)]
		[TestCase(1.0)]
		[TestCase(1.12345)]
		[TestCase(-1.12345e+23)]
		[TestCase(double.MinValue)]
		[TestCase(double.MaxValue)]
		[TestCase(double.NaN)]
		[TestCase(double.NegativeInfinity)]
		public void ReadWriteDouble(double value)
		{
			_writer.Write(ElementId, value);

			var reader = StartRead();
			Assert.AreEqual(value, reader.ReadFloat());
		}

		[TestCase("abc")]
		[TestCase("")]
		[TestCase("1bcdefg")]
		public void ReadWriteStringAscii(string value)
		{
			_writer.WriteAscii(ElementId, value);

			var reader = StartRead();
			Assert.AreEqual(value, reader.ReadAscii());
		}

		[TestCase("abc")]
		[TestCase("")]
		[TestCase("1bcdefg")]
		[TestCase("Йцукенг12345Qwerty\u1fa8\u263a")]
		public void ReadWriteStringUtf(string value)
		{
			_writer.WriteUtf(ElementId, value);

			var reader = StartRead();
			Assert.AreEqual(value, reader.ReadUtf());
		}

		public enum WriteStrMode { Ascii, Utf };

		[TestCase(WriteStrMode.Ascii, (string)null)]
		[TestCase(WriteStrMode.Utf, (string)null)]
		public void WriteStringBadArg(WriteStrMode mode, string value)
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				if (mode == WriteStrMode.Ascii) _writer.WriteAscii(ElementId, value);
				if (mode == WriteStrMode.Utf) _writer.WriteUtf(ElementId, value);

			});
		}
	}
}
