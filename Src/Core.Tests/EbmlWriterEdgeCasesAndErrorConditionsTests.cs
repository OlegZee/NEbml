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
	public class EbmlWriterEdgeCasesAndErrorConditionsTests : EbmlWriterTestBase
	{
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_DisposedTwice_DoesNotThrow(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			var master = _writer.StartMasterElement(masterId, strategy);
			master.WriteAscii(childId, "test");

			// Dispose once
			master.Dispose();

			// Dispose again - should not throw
			Assert.DoesNotThrow(() => master.Dispose());
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		public void SomeStrategies_WriteAfterDispose_ThrowsObjectDisposedException(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			var master = _writer.StartMasterElement(masterId, strategy);
			master.Dispose();

			// Writing after dispose should throw
			Assert.Throws<ObjectDisposedException>(() => master.WriteAscii(childId, "test"));
		}

		[Test]
		public void HybridStrategy_WriteAfterDispose_ThrowsException()
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			var master = _writer.StartMasterElement(masterId, EbmlWriter.MasterElementSizeStrategy.Hybrid);
			master.Dispose();

			// Writing after dispose should throw some exception (NullReferenceException for Hybrid due to disposed buffer)
			Assert.Throws<NullReferenceException>(() => master.WriteAscii(childId, "test"));
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_ZeroSizeElements_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(1234);
			var childId = VInt.MakeId(5678);

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				master.WriteAscii(childId, "");  // Empty string
				master.Write(childId, new byte[0]);  // Empty binary
			}

			// Verify zero-size elements are readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			// Empty string
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual("", reader.ReadAscii());

			// Empty binary
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			var buffer = new byte[1];
			Assert.AreEqual(-1, reader.ReadBinary(buffer, 0, 1)); // Should indicate no data

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_VariousElementIds_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			// Test with various valid element ID sizes (within VInt capacity)
			var validIds = new[]
			{
				VInt.MakeId(0x80),           // 2-byte ID
				VInt.MakeId(0x4000),         // 3-byte ID
				VInt.MakeId(0x200000),       // 4-byte ID (max for this test)
			};

			var masterId = VInt.MakeId(5000);

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				foreach (var id in validIds)
				{
					master.WriteAscii(id, $"content_for_{id.EncodedValue:X}");
				}
			}

			// Verify IDs are handled correctly
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			foreach (var expectedId in validIds)
			{
				Assert.IsTrue(reader.ReadNext());
				Assert.AreEqual(expectedId, reader.ElementId);
				Assert.AreEqual($"content_for_{expectedId.EncodedValue:X}", reader.ReadAscii());
			}

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_MemoryStreamResize_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(6000);
			var childId = VInt.MakeId(6001);

			// Start with a small capacity stream to force resizing
			var smallStream = new MemoryStream(capacity: 10);
			var writer = new EbmlWriter(smallStream);

			// Write content that will exceed initial capacity
			var largeContent = new string('X', 1000);

			using (var master = writer.StartMasterElement(masterId, strategy))
			{
				master.WriteAscii(childId, largeContent);
			}

			// Verify content is readable after stream resizing
			smallStream.Position = 0;
			var reader = new EbmlReader(smallStream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			Assert.AreEqual(largeContent, reader.ReadAscii());

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_ExceptionDuringWrite_HandlesGracefully(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(7000);
			var childId = VInt.MakeId(7001);

			// Create a stream that will throw after a certain amount of data
			var throwingStream = new ThrowingMemoryStream(throwAfterBytes: 100);
			var writer = new EbmlWriter(throwingStream);

			// Attempt to write data that will trigger the exception
			Assert.Throws<InvalidOperationException>(() =>
			{
				using (var master = writer.StartMasterElement(masterId, strategy))
				{
					var largeData = new string('X', 200);
					master.WriteAscii(childId, largeData);
				}
			});
		}

		[Test]
		public void AllStrategies_PerformanceComparison_CompletesInReasonableTime()
		{
			const int elementCount = 1000;
			var masterId = VInt.MakeId(8000);
			var childId = VInt.MakeId(8001);
			var testData = "performance test data";

			var strategies = new[]
			{
				EbmlWriter.MasterElementSizeStrategy.Buffered,
				EbmlWriter.MasterElementSizeStrategy.Backpatching,
				EbmlWriter.MasterElementSizeStrategy.Hybrid
			};

			foreach (var strategy in strategies)
			{
				var stream = new MemoryStream();
				var writer = new EbmlWriter(stream);
				var stopwatch = System.Diagnostics.Stopwatch.StartNew();

				using (var master = writer.StartMasterElement(masterId, strategy))
				{
					for (int i = 0; i < elementCount; i++)
					{
						master.WriteAscii(childId, $"{testData}_{i}");
					}
				}

				stopwatch.Stop();

				// Performance should be reasonable (less than 1 second for 1000 elements)
				Assert.Less(stopwatch.ElapsedMilliseconds, 1000,
					$"Strategy {strategy} took too long: {stopwatch.ElapsedMilliseconds}ms");

				// Verify output is correct
				stream.Position = 0;
				var reader = new EbmlReader(stream);
				Assert.IsTrue(reader.ReadNext());
				reader.EnterContainer();

				int count = 0;
				while (reader.ReadNext())
				{
					Assert.AreEqual($"{testData}_{count}", reader.ReadAscii());
					count++;
				}

				Assert.AreEqual(elementCount, count, $"Strategy {strategy} produced wrong element count");
				reader.LeaveContainer();
			}
		}

		// Helper class for exception testing
		private class ThrowingMemoryStream : MemoryStream
		{
			private readonly int _throwAfterBytes;
			private int _bytesWritten = 0;

			public ThrowingMemoryStream(int throwAfterBytes)
			{
				_throwAfterBytes = throwAfterBytes;
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				_bytesWritten += count;
				if (_bytesWritten > _throwAfterBytes)
				{
					throw new InvalidOperationException("Simulated stream exception");
				}
				base.Write(buffer, offset, count);
			}
		}
	}
}
