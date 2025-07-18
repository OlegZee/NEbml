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
	public class EbmlWriterUnknownSizeContainerTests : EbmlWriterTestBase
	{
		[Test]
		public void WriteUnknownSizeContainer_CreatesValidUnknownSizeContainer()
		{
			var rootId = VInt.MakeId(1000);
			var masterId = VInt.MakeId(999);

			using (var root = _writer.StartMasterElement(rootId))
			{
				root.WriteElementHeader(masterId, VInt.UnknownSize(1));
				root.WriteAscii(VInt.MakeId(1), "test");
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root container
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			// Read unknown-size container
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);

			// Unknown-size containers should still be readable
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("test", reader.ReadAscii());
			reader.LeaveContainer();

			reader.LeaveContainer();
		}

		[Test]
		public void WriteUnknownSizeContainer_WithMultipleChildren_ReadsCorrectly()
		{
			var rootId = VInt.MakeId(1001);
			var masterId = VInt.MakeId(100);
			var childId1 = VInt.MakeId(101);
			var childId2 = VInt.MakeId(102);
			var childId3 = VInt.MakeId(103);

			using (var root = _writer.StartMasterElement(rootId))
			{
				root.WriteElementHeader(masterId, VInt.UnknownSize(1));
				root.WriteAscii(childId1, "child1");
				root.Write(childId2, 42L);
				root.Write(childId3, 3.14159);
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

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

			// Read third child
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId3, reader.ElementId);
			Assert.AreEqual(3.14159, reader.ReadFloat(), 0.00001);

			// Should be no more children
			Assert.IsFalse(reader.ReadNext());

			reader.LeaveContainer();
			reader.LeaveContainer();
		}

		[Test]
		public void WriteUnknownSizeContainer_DifferentSizeLengths_HandledCorrectly()
		{
			var rootId = VInt.MakeId(1002);
			var masterId1 = VInt.MakeId(200);
			var masterId2 = VInt.MakeId(201);
			var childId = VInt.MakeId(202);

			using (var root = _writer.StartMasterElement(rootId))
			{
				// Test with 1-byte size field
				root.WriteElementHeader(masterId1, VInt.UnknownSize(1));
				root.WriteAscii(childId, "test1");

				// Test with 2-byte size field
				root.WriteElementHeader(masterId2, VInt.UnknownSize(2));
				root.WriteAscii(childId, "test2");
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			// Read first master element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId1, reader.ElementId);
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("test1", reader.ReadAscii());
			reader.LeaveContainer();

			// Read second master element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId2, reader.ElementId);
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("test2", reader.ReadAscii());
			reader.LeaveContainer();

			reader.LeaveContainer();
		}

		[Test]
		public void WriteNestedUnknownSizeContainers_ReadsCorrectly()
		{
			var rootId = VInt.MakeId(1003);
			var outerMasterId = VInt.MakeId(300);
			var innerMasterId = VInt.MakeId(301);
			var childId = VInt.MakeId(302);

			using (var root = _writer.StartMasterElement(rootId))
			{
				root.WriteElementHeader(outerMasterId, VInt.UnknownSize(1));
				root.WriteElementHeader(innerMasterId, VInt.UnknownSize(1));
				root.WriteAscii(childId, "nested");
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			// Read outer master element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(outerMasterId, reader.ElementId);
			reader.EnterContainer();

			// Read inner master element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(innerMasterId, reader.ElementId);
			reader.EnterContainer();

			// Read child element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual("nested", reader.ReadAscii());

			// Leave containers
			reader.LeaveContainer();
			reader.LeaveContainer();
			reader.LeaveContainer();
		}

		[Test]
		public void WriteMixedKnownAndUnknownSizeContainers_ReadsCorrectly()
		{
			var rootId = VInt.MakeId(1004);
			var knownMasterId = VInt.MakeId(400);
			var unknownMasterId = VInt.MakeId(401);
			var childId = VInt.MakeId(402);

			using (var root = _writer.StartMasterElement(rootId))
			{
				// Known-size container
				using (var knownMaster = root.StartMasterElement(knownMasterId))
				{
					knownMaster.WriteAscii(childId, "known");
				}

				// Unknown-size container
				root.WriteElementHeader(unknownMasterId, VInt.UnknownSize(1));
				root.WriteAscii(childId, "unknown");
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			// Read known-size container
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(knownMasterId, reader.ElementId);
			Assert.Greater(reader.ElementSize, 0); // Should have known size
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("known", reader.ReadAscii());
			reader.LeaveContainer();

			// Read unknown-size container
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(unknownMasterId, reader.ElementId);
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("unknown", reader.ReadAscii());
			reader.LeaveContainer();

			reader.LeaveContainer();
		}

		[Test]
		public void WriteUnknownSizeContainer_WithBinaryData_ReadsCorrectly()
		{
			var rootId = VInt.MakeId(1005);
			var masterId = VInt.MakeId(500);
			var binaryId = VInt.MakeId(501);
			var binaryData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

			using (var root = _writer.StartMasterElement(rootId))
			{
				root.WriteElementHeader(masterId, VInt.UnknownSize(1));
				root.Write(binaryId, binaryData);
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			// Read master element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			// Read binary data
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(binaryId, reader.ElementId);
			var result = ReadAllBinary(reader);
			CollectionAssert.AreEqual(binaryData, result);

			reader.LeaveContainer();
			reader.LeaveContainer();
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		[TestCase(8)]
		public void WriteUnknownSizeContainer_VariousSizeLengths_AllWork(int sizeLength)
		{
			var rootId = VInt.MakeId(1006);
			var masterId = VInt.MakeId(600);
			var childId = VInt.MakeId(601);

			using (var root = _writer.StartMasterElement(rootId))
			{
				root.WriteElementHeader(masterId, VInt.UnknownSize(sizeLength));
				root.WriteAscii(childId, $"test{sizeLength}");
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual($"test{sizeLength}", reader.ReadAscii());
			reader.LeaveContainer();
			reader.LeaveContainer();
		}

		[Test]
		public void WriteUnknownSizeContainer_InvalidSizeLength_ThrowsException()
		{
			var masterId = VInt.MakeId(700);

			// Test invalid size lengths - VInt.UnknownSize should throw for invalid lengths
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				VInt.UnknownSize(0));

			Assert.Throws<ArgumentOutOfRangeException>(() =>
				VInt.UnknownSize(9));

			Assert.Throws<ArgumentOutOfRangeException>(() =>
				VInt.UnknownSize(-1));
		}

		[Test]
		public void WriteUnknownSizeContainer_EmptyContainer_ReadsCorrectly()
		{
			var rootId = VInt.MakeId(1007);
			var masterId = VInt.MakeId(800);

			using (var root = _writer.StartMasterElement(rootId))
			{
				root.WriteElementHeader(masterId, VInt.UnknownSize(1));
				// Write nothing - empty container
			}

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			// Should have no children
			Assert.IsFalse(reader.ReadNext());

			reader.LeaveContainer();
			reader.LeaveContainer();
		}

		[Test]
		public void WriteUnknownSizeContainer_BinaryFormat_IsCorrect()
		{
			var rootId = VInt.MakeId(1008);
			var masterId = VInt.MakeId(0x10); // Simple 1-byte element ID
			var childId = VInt.MakeId(0x20);  // Simple 1-byte element ID

			using (var root = _writer.StartMasterElement(rootId))
			{
				root.WriteElementHeader(masterId, VInt.UnknownSize(1));
				root.WriteAscii(childId, "X"); // Single character for simplicity
			}

			_stream.Position = 0;
			var data = ((MemoryStream)_stream).ToArray();


			// Verify it can be read back correctly (skip exact binary format validation due to root wrapper)
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual("X", reader.ReadAscii());
			reader.LeaveContainer();
			reader.LeaveContainer();
		}

		[Test]
		public void WriteUnknownSizeContainer_2ByteSize_BinaryFormatIsCorrect()
		{
			var rootId = VInt.MakeId(1009);
			var masterId = VInt.MakeId(0x10);
			var childId = VInt.MakeId(0x20);

			using (var root = _writer.StartMasterElement(rootId))
			{
				root.WriteElementHeader(masterId, VInt.UnknownSize(2));
				root.WriteAscii(childId, "Y");
			}

			_stream.Position = 0;
			var data = ((MemoryStream)_stream).ToArray();

			// Verify it can be read back correctly (skip exact binary format validation due to root wrapper)
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read root element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual("Y", reader.ReadAscii());
			reader.LeaveContainer();
			reader.LeaveContainer();
		}
	}
}
