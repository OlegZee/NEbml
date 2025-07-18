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
	public class EbmlWriterBinaryDataTests : EbmlWriterTestBase
	{
		[Test]
		public void WriteBinary_WithNullArray_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _writer.Write(ElementId, (byte[])null));
		}

		[Test]
		public void WriteBinary_EmptyArray_WritesCorrectly()
		{
			var data = new byte[0];
			var bytesWritten = _writer.Write(ElementId, data);

			var reader = StartRead();
			var buffer = new byte[0];
			var bytesRead = reader.ReadBinary(buffer, 0, 0);

			Assert.AreEqual(-1, bytesRead); // Empty element should return -1
			Assert.Greater(bytesWritten, 0); // Should write header even for empty data
		}

		[Test]
		public void WriteBinary_SingleByte_WritesCorrectly()
		{
			var data = new byte[] { 0xFF };
			_writer.Write(ElementId, data);

			var reader = StartRead();
			var result = ReadAllBinary(reader);

			CollectionAssert.AreEqual(data, result);
		}

		[Test]
		public void WriteBinary_LargeArray_WritesCorrectly()
		{
			var data = new byte[1000];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % 256);
			}

			_writer.Write(ElementId, data);

			var reader = StartRead();
			var result = ReadAllBinary(reader);

			CollectionAssert.AreEqual(data, result);
		}

		[Test]
		public void WriteBinary_WithOffsetAndLength_WritesCorrectly()
		{
			var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			var offset = 2;
			var length = 5;
			var expected = new byte[] { 3, 4, 5, 6, 7 };

			_writer.Write(ElementId, data, offset, length);

			var reader = StartRead();
			var result = ReadAllBinary(reader);

			CollectionAssert.AreEqual(expected, result);
		}

		[Test]
		public void WriteRawBinary_WritesDirectlyToStream()
		{
			var data = new byte[] { 1, 2, 3, 4, 5 };
			var bytesWritten = _writer.Write(data, 0, data.Length);

			Assert.AreEqual(data.Length, bytesWritten);
			Assert.AreEqual(data.Length, _stream.Length);

			_stream.Position = 0;
			var buffer = new byte[data.Length];
			_stream.Read(buffer, 0, buffer.Length);
			CollectionAssert.AreEqual(data, buffer);
		}
	}
}
