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
using System.IO;
using NEbml.Core;
using NUnit.Framework;

namespace Core.Tests
{
	[TestFixture]
	public class EbmlWriterMasterElementTests : EbmlWriterTestBase
	{
		[Test]
		public void ReadWriteContainer()
		{
			var innerdata = new MemoryStream();
			var container = new EbmlWriter(innerdata);
			container.WriteAscii(VInt.MakeId(1), "Hello");
			container.Write(VInt.MakeId(2), 12345);
			container.Write(VInt.MakeId(3), 123.45);

			_writer.Write(VInt.MakeId(5), innerdata.ToArray());
			_writer.WriteAscii(VInt.MakeId(6), "end");

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(VInt.MakeId(5), reader.ElementId);

			reader.EnterContainer();

			// reading inner data
			AssertRead(reader, 1, "Hello", r => r.ReadAscii());
			AssertRead(reader, 2, 12345, r => r.ReadInt());
			AssertRead(reader, 3, 123.45, r => r.ReadFloat());

			reader.LeaveContainer();

			// back to main stream
			AssertRead(reader, 6, "end", r => r.ReadAscii());
		}

		[Test]
		public void StartMasterElement_CreatesValidMasterBlockWriter()
		{
			var masterId = VInt.MakeId(999);
			using (var master = _writer.StartMasterElement(masterId))
			{
				Assert.IsNotNull(master);
				master.WriteAscii(VInt.MakeId(1), "test");
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
		}

		[Test]
		public void StartMasterElement_NestedElements_WritesCorrectly()
		{
			var masterId = VInt.MakeId(100);
			var childId1 = VInt.MakeId(101);
			var childId2 = VInt.MakeId(102);

			using (var master = _writer.StartMasterElement(masterId))
			{
				master.WriteAscii(childId1, "child1");
				master.Write(childId2, 42L);
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read master element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);

			reader.EnterContainer();

			// Read first child
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId1, reader.ElementId);
			Assert.AreEqual("child1", reader.ReadAscii());

			// Read second child
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId2, reader.ElementId);
			Assert.AreEqual(42L, reader.ReadInt());

			reader.LeaveContainer();
		}

		[Test]
		public void WriteElementHeader_WritesCorrectly()
		{
			var elementId = VInt.MakeId(123);
			var size = VInt.EncodeSize(456);

			var bytesWritten = _writer.WriteElementHeader(elementId, size);

			Assert.Greater(bytesWritten, 0);
			Assert.AreEqual(bytesWritten, _stream.Length);
		}

		[Test]
		public void MultipleWrites_StreamPositionAdvancesCorrectly()
		{
			var initialPosition = _stream.Position;

			var bytes1 = _writer.Write(ElementId, 123L);
			var pos1 = _stream.Position;

			var bytes2 = _writer.WriteAscii(VInt.MakeId(456), "test");
			var pos2 = _stream.Position;

			Assert.AreEqual(initialPosition + bytes1, pos1);
			Assert.AreEqual(pos1 + bytes2, pos2);
			Assert.AreEqual(bytes1 + bytes2, _stream.Length);
		}

		[Test]
		public void WriteOperations_ReturnCorrectByteCount()
		{
			var data = new byte[] { 1, 2, 3 };
			var bytesWritten = _writer.Write(ElementId, data);

			// Should include element ID, size, and data
			Assert.AreEqual(bytesWritten, _stream.Length);
			Assert.Greater(bytesWritten, data.Length); // Should be more than just data due to headers
		}

		[Test]
		public void WriteRawBinary_ReturnsExactLength()
		{
			var data = new byte[] { 1, 2, 3, 4, 5 };
			var bytesWritten = _writer.Write(data, 0, data.Length);

			Assert.AreEqual(data.Length, bytesWritten);
		}
	}
}
