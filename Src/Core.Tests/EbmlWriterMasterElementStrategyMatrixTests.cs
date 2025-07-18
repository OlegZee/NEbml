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
using System.Collections.Generic;
using System.IO;
using NEbml.Core;
using NUnit.Framework;

namespace Core.Tests
{
	[TestFixture]
	public class EbmlWriterMasterElementStrategyMatrixTests : EbmlWriterTestBase
	{
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_SmallData_ProducesReadableOutput(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);
			var testData = "small test data";

			using (var master = _writer.StartMasterElement(masterId, strategy))
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

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_LargeData_ProducesReadableOutput(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);
			var largeData = new string('X', 1000); // Large data

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				master.WriteAscii(childId, largeData);
			}

			// Verify output is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual(largeData, reader.ReadAscii());

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_BinaryData_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var binaryId = VInt.MakeId(5678);
			var binaryData = new byte[500]; // Medium-sized binary data
			for (int i = 0; i < binaryData.Length; i++)
			{
				binaryData[i] = (byte)(i % 256);
			}

			using (var master = _writer.StartMasterElement(masterId, strategy))
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

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_EmptyContainer_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				// Write nothing - empty container
			}

			// Verify empty container is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			Assert.AreEqual(0, reader.ElementSize); // Should be empty
			reader.EnterContainer();

			// Should have no children
			Assert.IsFalse(reader.ReadNext());

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_MultipleElements_WritesInCorrectOrder(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1000);
			var childId1 = VInt.MakeId(1);
			var childId2 = VInt.MakeId(2);
			var childId3 = VInt.MakeId(3);
			var childId4 = VInt.MakeId(4);

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				master.WriteAscii(childId1, "first");
				master.Write(childId2, 42L);
				master.Write(childId3, 3.14159);
				master.Write(childId4, new byte[] { 0xAB, 0xCD, 0xEF });
			}

			// Verify all elements are readable in correct order
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			// First element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId1, reader.ElementId);
			Assert.AreEqual("first", reader.ReadAscii());

			// Second element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId2, reader.ElementId);
			Assert.AreEqual(42L, reader.ReadInt());

			// Third element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId3, reader.ElementId);
			Assert.AreEqual(3.14159, reader.ReadFloat(), 0.00001);

			// Fourth element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId4, reader.ElementId);
			CollectionAssert.AreEqual(new byte[] { 0xAB, 0xCD, 0xEF }, ReadAllBinary(reader));

			// No more elements
			Assert.IsFalse(reader.ReadNext());

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered, 1)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered, 2)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered, 4)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered, 8)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching, 1)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching, 2)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching, 4)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching, 8)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid, 1)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid, 2)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid, 4)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid, 8)]
		public void AllStrategies_VariousSizeFieldLengths_WorkCorrectly(EbmlWriter.MasterElementSizeStrategy strategy, int sizeFieldLength)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);
			var testData = "test data for size field length";

			using (var master = _writer.StartMasterElement(masterId, strategy, sizeFieldLength))
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

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_NestedContainers_WorkCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var rootId = VInt.MakeId(1000);
			var level1Id = VInt.MakeId(1001);
			var level2Id = VInt.MakeId(1002);
			var level3Id = VInt.MakeId(1003);
			var leafId = VInt.MakeId(1004);

			using (var root = _writer.StartMasterElement(rootId, strategy))
			{
				using (var level1 = root.StartMasterElement(level1Id, strategy))
				{
					using (var level2 = level1.StartMasterElement(level2Id, strategy))
					{
						using (var level3 = level2.StartMasterElement(level3Id, strategy))
						{
							level3.WriteAscii(leafId, "deeply nested");
						}
					}
				}
			}

			// Verify nested structure is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(level1Id, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(level2Id, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(level3Id, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(leafId, reader.ElementId);
			Assert.AreEqual("deeply nested", reader.ReadAscii());

			reader.LeaveContainer();
			reader.LeaveContainer();
			reader.LeaveContainer();
			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		public void SomeStrategies_MixedNestedStrategies_WorkCorrectly(EbmlWriter.MasterElementSizeStrategy outerStrategy)
		{
			var rootId = VInt.MakeId(1000);
			var bufferedId = VInt.MakeId(1001);
			var backpatchingId = VInt.MakeId(1002);
			var leafId = VInt.MakeId(1004);

			using (var root = _writer.StartMasterElement(rootId, outerStrategy))
			{
				// Mix different strategies in nested containers (avoid Hybrid nesting)
				using (var buffered = root.StartMasterElement(bufferedId, EbmlWriter.MasterElementSizeStrategy.Buffered))
				{
					buffered.WriteAscii(leafId, "buffered child");
				}

				using (var backpatching = root.StartMasterElement(backpatchingId, EbmlWriter.MasterElementSizeStrategy.Backpatching))
				{
					backpatching.WriteAscii(leafId, "backpatching child");
				}
			}

			// Verify all nested containers are readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			// Read buffered child
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(bufferedId, reader.ElementId);
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("buffered child", reader.ReadAscii());
			reader.LeaveContainer();

			// Read backpatching child
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(backpatchingId, reader.ElementId);
			reader.EnterContainer();
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("backpatching child", reader.ReadAscii());
			reader.LeaveContainer();

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_VeryLargeContent_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Create very large content (10KB)
			var largeData = new byte[10240];
			var random = new Random(42); // Use seed for reproducible test
			random.NextBytes(largeData);

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				master.Write(childId, largeData);
			}

			// Verify large data is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			var resultData = ReadAllBinary(reader);

			CollectionAssert.AreEqual(largeData, resultData);
			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_ManySmallElements_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);
			const int elementCount = 100;

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				for (int i = 0; i < elementCount; i++)
				{
					master.WriteAscii(childId, $"element_{i:D3}");
				}
			}

			// Verify all elements are readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			for (int i = 0; i < elementCount; i++)
			{
				Assert.IsTrue(reader.ReadNext(), $"Failed to read element {i}");
				Assert.AreEqual(childId, reader.ElementId);
				Assert.AreEqual($"element_{i:D3}", reader.ReadAscii());
			}

			// No more elements
			Assert.IsFalse(reader.ReadNext());

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_UnicodeContent_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			// Various Unicode strings
			var unicodeStrings = new[]
			{
				"Hello World",
				"🌟🎉💫",
				"Привет мир",
				"你好世界",
				"こんにちは",
				"مرحبا بالعالم",
				"🇺🇸🇬🇧🇫🇷🇩🇪🇯🇵",
				"Mixed: Hello 世界 🌍 Привет"
			};

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				foreach (var str in unicodeStrings)
				{
					master.WriteUtf(childId, str);
				}
			}

			// Verify all Unicode strings are readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			foreach (var expectedStr in unicodeStrings)
			{
				Assert.IsTrue(reader.ReadNext());
				Assert.AreEqual(childId, reader.ElementId);
				Assert.AreEqual(expectedStr, reader.ReadUtf());
			}

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_NumericEdgeCases_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var intId = VInt.MakeId(1);
			var uintId = VInt.MakeId(2);
			var floatId = VInt.MakeId(3);
			var doubleId = VInt.MakeId(4);

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				// Integer edge cases
				master.Write(intId, long.MinValue);
				master.Write(intId, long.MaxValue);
				master.Write(intId, 0L);
				master.Write(intId, -1L);
				master.Write(intId, 1L);

				// Unsigned integer edge cases
				master.Write(uintId, ulong.MinValue);
				master.Write(uintId, ulong.MaxValue);
				master.Write(uintId, 1UL);

				// Float edge cases
				master.Write(floatId, float.MinValue);
				master.Write(floatId, float.MaxValue);
				master.Write(floatId, float.Epsilon);
				master.Write(floatId, float.NaN);
				master.Write(floatId, float.PositiveInfinity);
				master.Write(floatId, float.NegativeInfinity);

				// Double edge cases
				master.Write(doubleId, double.MinValue);
				master.Write(doubleId, double.MaxValue);
				master.Write(doubleId, double.Epsilon);
				master.Write(doubleId, double.NaN);
				master.Write(doubleId, double.PositiveInfinity);
				master.Write(doubleId, double.NegativeInfinity);
			}

			// Verify all numeric values are readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			// Read integers
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(long.MinValue, reader.ReadInt());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(long.MaxValue, reader.ReadInt());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(0L, reader.ReadInt());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(-1L, reader.ReadInt());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(1L, reader.ReadInt());

			// Read unsigned integers
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(ulong.MinValue, reader.ReadUInt());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(ulong.MaxValue, reader.ReadUInt());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(1UL, reader.ReadUInt());

			// Read floats
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(float.MinValue, reader.ReadFloat());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(float.MaxValue, reader.ReadFloat());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(float.Epsilon, reader.ReadFloat());
			Assert.IsTrue(reader.ReadNext());
			Assert.IsTrue(double.IsNaN(reader.ReadFloat()));
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(float.PositiveInfinity, reader.ReadFloat());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(float.NegativeInfinity, reader.ReadFloat());

			// Read doubles
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(double.MinValue, reader.ReadFloat());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(double.MaxValue, reader.ReadFloat());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(double.Epsilon, reader.ReadFloat());
			Assert.IsTrue(reader.ReadNext());
			Assert.IsTrue(double.IsNaN(reader.ReadFloat()));
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(double.PositiveInfinity, reader.ReadFloat());
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(double.NegativeInfinity, reader.ReadFloat());

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_IdenticalOutput_ForSameInput(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);
			var testData = "identical test data";

			// Write once
			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				master.WriteAscii(childId, testData);
			}
			var firstOutput = ((MemoryStream)_stream).ToArray();

			// Reset and write again
			_stream.SetLength(0);
			_stream.Position = 0;

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				master.WriteAscii(childId, testData);
			}
			var secondOutput = ((MemoryStream)_stream).ToArray();

			// Outputs should be identical
			CollectionAssert.AreEqual(firstOutput, secondOutput);
		}
	}
}
