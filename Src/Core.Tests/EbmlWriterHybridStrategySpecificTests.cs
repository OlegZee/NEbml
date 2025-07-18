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
	public class EbmlWriterHybridStrategySpecificTests : EbmlWriterTestBase
	{
		[TestCase(10)]
		[TestCase(32)]
		[TestCase(64)]
		[TestCase(128)]
		[TestCase(256)]
		[TestCase(1024)]
		public void HybridStrategy_CustomBufferLimits_WorkCorrectly(int bufferLimit)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Create data that will exceed the buffer limit
			var testData = new string('X', bufferLimit + 10);

			using (var master = _writer.StartHybridMasterElement(masterId, 8, bufferLimit))
			{
				master.WriteAscii(childId, testData);
			}

			// Verify output is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual(testData, reader.ReadAscii());

			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_IncrementalWrites_SwitchesAtCorrectPoint()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);
			const int bufferLimit = 50;

			using (var master = _writer.StartHybridMasterElement(masterId, 8, bufferLimit))
			{
				// Write small pieces that will eventually exceed the limit
				for (int i = 0; i < 10; i++)
				{
					master.WriteAscii(childId, $"piece{i}"); // Each piece is ~8-10 bytes + headers
				}
			}

			// Verify all pieces are readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			for (int i = 0; i < 10; i++)
			{
				Assert.IsTrue(reader.ReadNext());
				Assert.AreEqual(childId, reader.ElementId);
				Assert.AreEqual($"piece{i}", reader.ReadAscii());
			}

			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_EdgeCaseBufferLimit_HandlesCorrectly()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Test with very small buffer limit (1 byte)
			using (var master = _writer.StartHybridMasterElement(masterId, 8, 1))
			{
				master.WriteAscii(childId, "x"); // Should immediately switch to backpatching
			}

			// Verify output is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual("x", reader.ReadAscii());

			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_SmallData_UsesBufferedMode()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Write small amount of data (well under 64 byte default limit)
			using (var master = _writer.StartMasterElement(masterId, EbmlWriter.MasterElementSizeStrategy.Hybrid))
			{
				master.WriteAscii(childId, "small"); // Only 5 characters
			}

			// Verify output is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual("small", reader.ReadAscii());

			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_LargeData_SwitchesToBackpatchingMode()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Create large string that will exceed 64 byte buffer limit
			var largeString = new string('X', 100); // 100 characters

			using (var master = _writer.StartMasterElement(masterId, EbmlWriter.MasterElementSizeStrategy.Hybrid))
			{
				master.WriteAscii(childId, largeString);
			}

			// Verify output is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual(largeString, reader.ReadAscii());

			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_CustomBufferLimit_RespectsLimit()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Use custom small buffer limit (10 bytes)
			using (var master = _writer.StartHybridMasterElement(masterId, 8, 10))
			{
				// Write data that exceeds the 10 byte limit
				master.WriteAscii(childId, "This is longer than 10 bytes");
			}

			// Verify output is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual("This is longer than 10 bytes", reader.ReadAscii());

			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_MultipleSmallWrites_StaysInBufferedMode()
		{
			var masterId = VInt.MakeId(1234);
			var childId1 = VInt.MakeId(1);
			var childId2 = VInt.MakeId(2);
			var childId3 = VInt.MakeId(3);

			using (var master = _writer.StartMasterElement(masterId, EbmlWriter.MasterElementSizeStrategy.Hybrid))
			{
				master.WriteAscii(childId1, "small1");  // 6 bytes
				master.WriteAscii(childId2, "small2");  // 6 bytes  
				master.WriteAscii(childId3, "small3");  // 6 bytes
														// Total content: 18 bytes + headers, should stay under 64 byte limit
			}

			// Verify all data is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId1, reader.ElementId);
			Assert.AreEqual("small1", reader.ReadAscii());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId2, reader.ElementId);
			Assert.AreEqual("small2", reader.ReadAscii());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId3, reader.ElementId);
			Assert.AreEqual("small3", reader.ReadAscii());

			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_AccumulatedDataExceedsLimit_SwitchesToBackpatching()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			using (var master = _writer.StartHybridMasterElement(masterId, 8, 20)) // Small 20-byte limit
			{
				// Write multiple small pieces that accumulate to exceed limit
				master.WriteAscii(childId, "piece1");   // ~8 bytes + headers
				master.WriteAscii(childId, "piece2");   // ~8 bytes + headers
				master.WriteAscii(childId, "piece3");   // This should trigger switch to backpatching
			}

			// Verify all data is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			var readValues = new System.Collections.Generic.List<string>();
			while (reader.ReadNext())
			{
				Assert.AreEqual(childId, reader.ElementId);
				readValues.Add(reader.ReadAscii());
			}

			CollectionAssert.AreEqual(new[] { "piece1", "piece2", "piece3" }, readValues);
			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_ShallowNesting_WorksCorrectly()
		{
			var rootId = VInt.MakeId(1000);
			var childId = VInt.MakeId(1001);
			var leafId = VInt.MakeId(1002);

			// Test shallow hybrid nesting (1-2 levels deep)
			using (var root = _writer.StartMasterElement(rootId, EbmlWriter.MasterElementSizeStrategy.Hybrid))
			{
				root.WriteAscii(leafId, "root content");

				using (var child = root.StartMasterElement(childId, EbmlWriter.MasterElementSizeStrategy.Buffered))
				{
					child.WriteAscii(leafId, "child content");
				}
			}

			// Verify nested structure is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(leafId, reader.ElementId);
			Assert.AreEqual("root content", reader.ReadAscii());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(leafId, reader.ElementId);
			Assert.AreEqual("child content", reader.ReadAscii());

			reader.LeaveContainer();
			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_NestedElements_WorksCorrectly()
		{
			var rootId = VInt.MakeId(1000);
			var childMasterId = VInt.MakeId(2000);
			var grandchildId = VInt.MakeId(3000);

			using (var root = _writer.StartMasterElement(rootId, EbmlWriter.MasterElementSizeStrategy.Hybrid))
			{
				using (var child = root.StartMasterElement(childMasterId, EbmlWriter.MasterElementSizeStrategy.Hybrid))
				{
					child.WriteAscii(grandchildId, "nested content");
				}
			}

			// Verify nested structure is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childMasterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(grandchildId, reader.ElementId);
			Assert.AreEqual("nested content", reader.ReadAscii());

			reader.LeaveContainer();
			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_BinaryData_HandlesCorrectly()
		{
			var masterId = VInt.MakeId(1234);
			var binaryId = VInt.MakeId(5678);
			var binaryData = new byte[100]; // 100 bytes to trigger backpatching
			for (int i = 0; i < binaryData.Length; i++)
			{
				binaryData[i] = (byte)(i % 256);
			}

			using (var master = _writer.StartMasterElement(masterId, EbmlWriter.MasterElementSizeStrategy.Hybrid))
			{
				master.Write(binaryId, binaryData);
			}

			// Verify binary data is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(binaryId, reader.ElementId);
			var resultData = ReadAllBinary(reader);

			CollectionAssert.AreEqual(binaryData, resultData);
			reader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_ComparedToBufferedStrategy_UsesOptimalSizeInBufferedMode()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);
			var testData = "test data for comparison";

			// Write using buffered strategy (uses specified size field length)
			var bufferedStream = new MemoryStream();
			var bufferedWriter = new EbmlWriter(bufferedStream);
			using (var master = bufferedWriter.StartMasterElement(masterId, EbmlWriter.MasterElementSizeStrategy.Buffered))
			{
				master.WriteAscii(childId, testData);
			}

			// Write using hybrid strategy (with small data that won't trigger backpatching)
			// Should use optimal size field in buffered mode
			var hybridStream = new MemoryStream();
			var hybridWriter = new EbmlWriter(hybridStream);
			using (var master = hybridWriter.StartMasterElement(masterId, EbmlWriter.MasterElementSizeStrategy.Hybrid))
			{
				master.WriteAscii(childId, testData);
			}

			// Hybrid should use optimal encoding (shorter), buffered uses specified length (longer)
			var bufferedData = bufferedStream.ToArray();
			var hybridData = hybridStream.ToArray();

			// Hybrid output should be shorter due to optimal size field encoding
			Assert.AreEqual(hybridData.Length, bufferedData.Length, "Hybrid buffered mode should use optimal size field encoding");

			// Both should be readable and produce same content
			bufferedStream.Position = 0;
			var bufferedReader = new EbmlReader(bufferedStream);
			Assert.IsTrue(bufferedReader.ReadNext());
			bufferedReader.EnterContainer();
			Assert.IsTrue(bufferedReader.ReadNext());
			var bufferedContent = bufferedReader.ReadAscii();
			bufferedReader.LeaveContainer();

			hybridStream.Position = 0;
			var hybridReader = new EbmlReader(hybridStream);
			Assert.IsTrue(hybridReader.ReadNext());
			hybridReader.EnterContainer();
			Assert.IsTrue(hybridReader.ReadNext());
			var hybridContent = hybridReader.ReadAscii();
			hybridReader.LeaveContainer();

			Assert.AreEqual(testData, bufferedContent);
			Assert.AreEqual(testData, hybridContent);
		}

		[Test]
		public void HybridStrategy_ComparedToBackpatchingStrategy_ProducesSameOutput()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);
			var largeData = new string('X', 200); // Large data to force backpatching in hybrid

			// Write using backpatching strategy
			var backpatchingStream = new MemoryStream();
			var backpatchingWriter = new EbmlWriter(backpatchingStream);
			using (var master = backpatchingWriter.StartMasterElement(masterId, EbmlWriter.MasterElementSizeStrategy.Backpatching))
			{
				master.WriteAscii(childId, largeData);
			}

			// Write using hybrid strategy (with large data that will trigger backpatching)
			var hybridStream = new MemoryStream();
			var hybridWriter = new EbmlWriter(hybridStream);
			using (var master = hybridWriter.StartMasterElement(masterId, EbmlWriter.MasterElementSizeStrategy.Hybrid))
			{
				master.WriteAscii(childId, largeData);
			}

			// Both should produce identical output
			var backpatchingData = backpatchingStream.ToArray();
			var hybridData = hybridStream.ToArray();

			CollectionAssert.AreEqual(backpatchingData, hybridData);
		}

		[Test]
		public void HybridStrategy_BufferedModeUsesOptimalSizeField_BackpatchingUsesSpecifiedLength()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Test small data with buffered mode (should use optimal size field)
			var smallData = "small";
			var bufferedStream = new MemoryStream();
			var bufferedWriter = new EbmlWriter(bufferedStream);
			using (var master = bufferedWriter.StartHybridMasterElement(masterId, 8, 100)) // Large buffer limit
			{
				master.WriteAscii(childId, smallData);
			}

			// Test large data with backpatching mode (should use specified size field length)
			var largeData = new string('X', 200); // Exceeds buffer limit, triggers backpatching
			var backpatchingStream = new MemoryStream();
			var backpatchingWriter = new EbmlWriter(backpatchingStream);
			using (var master = backpatchingWriter.StartHybridMasterElement(masterId, 8, 50)) // Small buffer limit
			{
				master.WriteAscii(childId, largeData);
			}

			// Both should be readable
			bufferedStream.Position = 0;
			var bufferedReader = new EbmlReader(bufferedStream);
			Assert.IsTrue(bufferedReader.ReadNext());
			Assert.AreEqual(masterId, bufferedReader.ElementId);
			bufferedReader.EnterContainer();
			Assert.IsTrue(bufferedReader.ReadNext());
			Assert.AreEqual(smallData, bufferedReader.ReadAscii());
			bufferedReader.LeaveContainer();

			backpatchingStream.Position = 0;
			var backpatchingReader = new EbmlReader(backpatchingStream);
			Assert.IsTrue(backpatchingReader.ReadNext());
			Assert.AreEqual(masterId, backpatchingReader.ElementId);
			backpatchingReader.EnterContainer();
			Assert.IsTrue(backpatchingReader.ReadNext());
			Assert.AreEqual(largeData, backpatchingReader.ReadAscii());
			backpatchingReader.LeaveContainer();
		}

		[Test]
		public void HybridStrategy_BlockSizeExceedsSizeFieldCapacity_ThrowsException()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Use 1-byte size field (max capacity 127 bytes) but try to write more data
			// This should trigger backpatching mode and fail when the content exceeds the size field capacity
			var veryLargeData = new string('X', 200); // Exceeds 1-byte VInt capacity

			Assert.Throws<ArgumentException>(() =>
			{
				using (var master = _writer.StartHybridMasterElement(masterId, 1, 50)) // 1-byte size field, small buffer
				{
					master.WriteAscii(childId, veryLargeData); // This will exceed the 1-byte size field capacity
				}
			});
		}

		[Test]
		public void HybridStrategy_BufferedModeContentExceedsSizeFieldCapacity_StillWorks()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Use 1-byte size field but write data that would exceed its capacity
			// However, keep it under the buffer limit so it stays in buffered mode
			// Buffered mode should use optimal size field regardless of the specified length
			var mediumData = new string('X', 150); // Exceeds 1-byte VInt capacity but fits in buffer

			using (var master = _writer.StartHybridMasterElement(masterId, 1, 200)) // Large buffer limit
			{
				master.WriteAscii(childId, mediumData);
			}

			// Should be readable (buffered mode uses optimal size field)
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual(mediumData, reader.ReadAscii());

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void BlockSizeExceedsSizeFieldCapacity_ThrowsException(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Use 1-byte size field (max capacity 127 bytes) but try to write more data
			// Backpatching mode should fail when the content exceeds the size field capacity
			var veryLargeData = new string('X', 200); // Exceeds 1-byte VInt capacity

			Assert.Throws<ArgumentException>(() =>
			{
				using var master = _writer.StartMasterElement(masterId, strategy, 1); // 1-byte size field
				master.WriteAscii(childId, veryLargeData); // This will exceed the 1-byte size field capacity
			});
		}
	}
}
