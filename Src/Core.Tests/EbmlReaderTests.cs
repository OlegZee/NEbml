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
using System.Linq;
using System.Text;
using NEbml.Core;
using NUnit.Framework;

namespace Core.Tests
{
	[TestFixture]
	public class EbmlReaderTests
	{
		private MemoryStream _stream;
		private EbmlReader _reader;

		[SetUp]
		public void Setup()
		{
			_stream = new MemoryStream();
		}

		[TearDown]
		public void TearDown()
		{
			_reader?.Dispose();
			_stream?.Dispose();
		}

		private void WriteElementHeader(VInt elementId, VInt size)
		{
			elementId.Write(_stream);
			size.Write(_stream);
		}

		private void WriteElement(VInt elementId, byte[] data)
		{
			WriteElementHeader(elementId, VInt.EncodeSize((ulong)data.Length));
			_stream.Write(data, 0, data.Length);
		}

		private void WriteElement(VInt elementId, long value)
		{
			var data = new byte[8];
			for (int i = 7; i >= 0; i--)
			{
				data[i] = (byte)(value & 0xFF);
				value >>= 8;
			}

			// Find the minimal representation
			int startIndex = 0;
			bool isNegative = (data[0] & 0x80) != 0;

			// For negative numbers, we need to preserve the sign bit
			if (isNegative)
			{
				while (startIndex < 7 && data[startIndex] == 0xFF && (data[startIndex + 1] & 0x80) != 0)
					startIndex++;
			}
			else
			{
				while (startIndex < 7 && data[startIndex] == 0x00 && (data[startIndex + 1] & 0x80) == 0)
					startIndex++;
			}

			var length = 8 - startIndex;
			var minimalData = new byte[length];
			Array.Copy(data, startIndex, minimalData, 0, length);

			WriteElement(elementId, minimalData);
		}

		private void WriteElement(VInt elementId, ulong value)
		{
			var data = new byte[8];
			for (int i = 7; i >= 0; i--)
			{
				data[i] = (byte)(value & 0xFF);
				value >>= 8;
			}

			// Find the minimal representation - skip leading zeros
			int startIndex = 0;
			while (startIndex < 7 && data[startIndex] == 0x00)
				startIndex++;

			var length = 8 - startIndex;
			var minimalData = new byte[length];
			Array.Copy(data, startIndex, minimalData, 0, length);

			WriteElement(elementId, minimalData);
		}

		private void WriteElement(VInt elementId, double value)
		{
			var data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			WriteElement(elementId, data);
		}

		private void WriteElement(VInt elementId, float value)
		{
			var data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			WriteElement(elementId, data);
		}

		private void WriteElement(VInt elementId, string value, Encoding encoding)
		{
			var data = encoding.GetBytes(value);
			WriteElement(elementId, data);
		}

		private void WriteElement(VInt elementId, DateTime value)
		{
			var ticks = (value - new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100; // Convert to nanoseconds
			WriteElement(elementId, ticks);
		}

		private void StartReader()
		{
			_stream.Position = 0;
			_reader = new EbmlReader(_stream);
		}

		#region Basic Functionality Tests

		[Test]
		public void Constructor_WithStream_SetsCorrectState()
		{
			var stream = new MemoryStream();
			using var reader = new EbmlReader(stream);

			Assert.DoesNotThrow(() => reader.ReadNext());
		}

		[Test]
		public void Constructor_WithStreamAndSize_SetsCorrectState()
		{
			var stream = new MemoryStream();
			using var reader = new EbmlReader(stream, 1000);

			Assert.DoesNotThrow(() => reader.ReadNext());
		}

		[Test]
		public void Constructor_WithNullStream_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new EbmlReader(null));
		}

		[Test]
		public void Constructor_WithNegativeSize_ThrowsArgumentException()
		{
			var stream = new MemoryStream();
			Assert.Throws<ArgumentException>(() => new EbmlReader(stream, -1));
		}

		[Test]
		public void ReadNext_WithEmptyStream_ReturnsFalse()
		{
			StartReader();
			Assert.IsFalse(_reader.ReadNext());
		}

		#endregion

		#region Element ID Tests

		[Test]
		public void ReadNext_WithValidElementId_ReturnsTrue()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, new byte[] { 0x01, 0x02, 0x03 });

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(elementId, _reader.ElementId);
		}

		[Test]
		public void ReadNext_WithReservedElementId_ThrowsEbmlDataFormatException()
		{
			// Write a reserved element ID (all VINT_DATA bits set to 1)
			_stream.WriteByte(0xFF); // 1-byte VINT with all data bits set to 1
			_stream.WriteByte(0x81); // Size = 1
			_stream.WriteByte(0x00); // Data

			StartReader();
			Assert.Throws<EbmlDataFormatException>(() => _reader.ReadNext());
		}

		[Test]
		public void ElementId_WhenNoElementRead_ThrowsInvalidOperationException()
		{
			StartReader();
			Assert.Throws<InvalidOperationException>(() => { var _ = _reader.ElementId; });
		}

		[Test]
		public void ElementSize_WhenNoElementRead_ThrowsInvalidOperationException()
		{
			StartReader();
			Assert.Throws<InvalidOperationException>(() => { var _ = _reader.ElementSize; });
		}

		#endregion

		#region Element Data Size Tests

		[Test]
		public void ReadNext_WithValidElementDataSize_ReturnsCorrectSize()
		{
			var elementId = VInt.MakeId(0x80);
			var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
			WriteElement(elementId, data);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(data.Length, _reader.ElementSize);
		}

		[Test]
		public void ReadNext_WithUnknownSize_ThrowsExceptionAtRootLevel()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElementHeader(elementId, VInt.UnknownSize(1)); // Unknown size with 1-byte VINT
			var data = new byte[] { 0x01, 0x02, 0x03 };
			_stream.Write(data, 0, data.Length);

			StartReader();
			// Unknown-size elements are not allowed at root level per EBML specification
			var ex = Assert.Throws<EbmlDataFormatException>(() => _reader.ReadNext());
			Assert.IsTrue(ex.Message.Contains("unknown-size elements are not allowed at root level"));
		}

		[Test]
		public void ReadNext_WithUnknownSizeDifferentLengths_ThrowsExceptionAtRootLevel()
		{
			// Test that unknown-size elements with different VINT lengths (1-8 bytes) are rejected at root level
			var testCases = new[]
			{
				new { Length = 1, Name = "1-byte" },
				new { Length = 2, Name = "2-byte" },
				new { Length = 3, Name = "3-byte" },
				new { Length = 4, Name = "4-byte" },
				new { Length = 5, Name = "5-byte" },
				new { Length = 6, Name = "6-byte" },
				new { Length = 7, Name = "7-byte" },
				new { Length = 8, Name = "8-byte" }
			};

			foreach (var testCase in testCases)
				using (var stream = new MemoryStream())
				using (var reader = new EbmlReader(stream, 1000)) // Container with 1000 bytes
				{
					// Create element with unknown size
					var elementId = VInt.MakeId(0x80);
					var unknownSizeVint = VInt.UnknownSize(testCase.Length);

					elementId.Write(stream);
					unknownSizeVint.Write(stream);

					// Add some data content
					var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
					stream.Write(data, 0, data.Length);

					stream.Position = 0;

					// Read and verify that it throws exception at root level
					var ex = Assert.Throws<EbmlDataFormatException>(() => reader.ReadNext(),
						$"Should throw exception for {testCase.Name} unknown-size VINT at root level");
					Assert.IsTrue(ex.Message.Contains("unknown-size elements are not allowed at root level"),
						$"Exception message should mention root level restriction for {testCase.Name} VINT");
				}
		}

		[TestCase(1, 127UL)]
		[TestCase(2, 16383UL)]
		[TestCase(3, 2097151UL)]
		[TestCase(4, 268435455UL)]
		[TestCase(5, 34359738367UL)]
		[TestCase(6, 4398046511103UL)]
		[TestCase(7, 562949953421311UL)]
		[TestCase(8, 72057594037927935UL)]
		public void ReadNext_WithUnknownSizeAllVintLengthsSupported_ThrowsExceptionAtRootLevel(int vintLength, ulong maxValue)
		{
			using var stream = new MemoryStream();
			using var reader = new EbmlReader(stream, 1000);

			var elementId = VInt.MakeId(0x80);
			var unknownSizeVint = VInt.UnknownSize(vintLength);

			// Verify the unknown size VINT properties
			Assert.AreEqual(maxValue, unknownSizeVint.Value);
			Assert.IsTrue(unknownSizeVint.IsReserved);

			// Write element header and data
			elementId.Write(stream);
			unknownSizeVint.Write(stream);
			stream.Write(new byte[] { 0x01, 0x02, 0x03 }, 0, 3);

			stream.Position = 0;

			// Should throw exception because unknown-size elements are not allowed at root level
			var ex = Assert.Throws<EbmlDataFormatException>(() => reader.ReadNext());
			Assert.IsTrue(ex.Message.Contains("unknown-size elements are not allowed at root level"));
		}

		#endregion

		#region Data Type Reading Tests

		[TestCase(0L)]
		[TestCase(123L)]
		[TestCase(-123L)]
		[TestCase(long.MaxValue)]
		[TestCase(long.MinValue)]
		public void ReadInt_WithValidSignedInteger_ReturnsCorrectValue(long value)
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, value);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(value, _reader.ReadInt());
		}

		[TestCase(0UL)]
		[TestCase(123UL)]
		[TestCase(ulong.MaxValue)]
		public void ReadUInt_WithValidUnsignedInteger_ReturnsCorrectValue(ulong value)
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, value);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(value, _reader.ReadUInt());
		}

		[TestCase(0.0f)]
		[TestCase(1.0f)]
		[TestCase(-1.0f)]
		[TestCase(3.14159f)]
		[TestCase(float.MaxValue)]
		[TestCase(float.MinValue)]
		public void ReadFloat_WithValidFloat_ReturnsCorrectValue(float value)
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, value);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(value, _reader.ReadFloat(), 0.00001);
		}

		[TestCase(0.0)]
		[TestCase(1.0)]
		[TestCase(-1.0)]
		[TestCase(3.141592653589793)]
		[TestCase(double.MaxValue)]
		[TestCase(double.MinValue)]
		public void ReadFloat_WithValidDouble_ReturnsCorrectValue(double value)
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, value);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(value, _reader.ReadFloat(), 0.000000000001);
		}

		[Test]
		public void ReadDate_WithValidDate_ReturnsCorrectValue()
		{
			var elementId = VInt.MakeId(0x80);
			var date = new DateTime(2023, 6, 15, 14, 30, 25, DateTimeKind.Utc);
			WriteElement(elementId, date);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			var result = _reader.ReadDate();

			// Allow for small precision differences due to nanosecond conversion
			Assert.IsTrue(Math.Abs((date - result).TotalMilliseconds) < 1);
		}

		[TestCase("")]
		[TestCase("Hello")]
		[TestCase("ASCII string with numbers 123")]
		public void ReadAscii_WithValidAsciiString_ReturnsCorrectValue(string value)
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, value, Encoding.ASCII);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(value, _reader.ReadAscii());
		}

		[TestCase("")]
		[TestCase("Hello")]
		[TestCase("UTF-8 string with unicode: 🌟")]
		[TestCase("Cyrillic: Привет")]
		public void ReadUtf_WithValidUtf8String_ReturnsCorrectValue(string value)
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, value, Encoding.UTF8);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(value, _reader.ReadUtf());
		}

		[Test]
		public void ReadBinary_WithValidBinaryData_ReturnsCorrectData()
		{
			var elementId = VInt.MakeId(0x80);
			var data = new byte[] { 0x01, 0x02, 0x03, 0xFF, 0xAB, 0xCD };
			WriteElement(elementId, data);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());

			var buffer = new byte[data.Length];
			var bytesRead = _reader.ReadBinary(buffer, 0, buffer.Length);

			Assert.AreEqual(data.Length, bytesRead);
			CollectionAssert.AreEqual(data, buffer);
		}

		[Test]
		public void ReadBinary_WithPartialRead_ReturnsCorrectData()
		{
			var elementId = VInt.MakeId(0x80);
			var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
			WriteElement(elementId, data);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());

			var buffer = new byte[3];
			var bytesRead = _reader.ReadBinary(buffer, 0, buffer.Length);

			Assert.AreEqual(3, bytesRead);
			CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0x03 }, buffer);

			// Read remaining data
			buffer = new byte[2];
			bytesRead = _reader.ReadBinary(buffer, 0, buffer.Length);

			Assert.AreEqual(2, bytesRead);
			CollectionAssert.AreEqual(new byte[] { 0x04, 0x05 }, buffer);

			// Try to read beyond end
			buffer = new byte[1];
			bytesRead = _reader.ReadBinary(buffer, 0, buffer.Length);
			Assert.AreEqual(-1, bytesRead);
		}

		#endregion

		#region Container Tests

		[Test]
		public void EnterContainer_WithMasterElement_AllowsReadingChildElements()
		{
			var containerId = VInt.MakeId(0x80);
			var childId = VInt.MakeId(0x81);
			var childData = new byte[] { 0x12, 0x34 };

			// Create container with child element
			var containerData = new MemoryStream();
			var childIdBytes = new byte[childId.Length];
			var childSizeBytes = new byte[VInt.EncodeSize((ulong)childData.Length).Length];

			// Write child element to container
			childId.Write(containerData);
			VInt.EncodeSize((ulong)childData.Length).Write(containerData);
			containerData.Write(childData, 0, childData.Length);

			var containerBytes = containerData.ToArray();
			WriteElement(containerId, containerBytes);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());

			_reader.EnterContainer();

			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(childId, _reader.ElementId);

			var buffer = new byte[childData.Length];
			var bytesRead = _reader.ReadBinary(buffer, 0, buffer.Length);
			Assert.AreEqual(childData.Length, bytesRead);
			CollectionAssert.AreEqual(childData, buffer);
		}

		[Test]
		public void LeaveContainer_AfterEnteringContainer_ReturnsToParentLevel()
		{
			var containerId = VInt.MakeId(0x80);
			var childId = VInt.MakeId(0x81);
			var nextElementId = VInt.MakeId(0x82);

			var containerData = new MemoryStream();
			childId.Write(containerData);
			VInt.EncodeSize(1UL).Write(containerData); // Size = 1
			containerData.WriteByte(0x12); // Child data

			var containerBytes = containerData.ToArray();
			WriteElement(containerId, containerBytes);
			WriteElement(nextElementId, new byte[] { 0x34 });

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(containerId, _reader.ElementId);

			_reader.EnterContainer();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(childId, _reader.ElementId);

			_reader.LeaveContainer();

			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(nextElementId, _reader.ElementId);
		}

		[Test]
		public void EnterContainer_WhenNotMasterElement_ThrowsInvalidOperationException()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, 12345L);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			_reader.ReadInt(); // Mark as non-master element

			Assert.Throws<InvalidOperationException>(() => _reader.EnterContainer());
		}

		[Test]
		public void LeaveContainer_WhenAtRootLevel_ThrowsInvalidOperationException()
		{
			StartReader();
			Assert.Throws<InvalidOperationException>(() => _reader.LeaveContainer());
		}

		[Test]
		public void LeaveContainer_WithPartiallyReadElement_SkipsRemainingDataAndReturnsToParent()
		{
			var containerId = VInt.MakeId(0x80);
			var childId = VInt.MakeId(0x81);
			var nextElementId = VInt.MakeId(0x82);
			var childData = new byte[] { 0x12, 0x34, 0x56, 0x78 };

			var containerData = new MemoryStream();
			childId.Write(containerData);
			VInt.EncodeSize((ulong)childData.Length).Write(containerData);
			containerData.Write(childData, 0, childData.Length);

			var containerBytes = containerData.ToArray();
			WriteElement(containerId, containerBytes);
			WriteElement(nextElementId, new byte[] { 0x99 });

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(containerId, _reader.ElementId);

			_reader.EnterContainer();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(childId, _reader.ElementId);
			Assert.AreEqual(childData.Length, _reader.ElementSize);

			// Partially read the element (only 2 bytes out of 4)
			var buffer = new byte[2];
			int bytesRead = _reader.ReadBinary(buffer, 0, 2);
			Assert.AreEqual(2, bytesRead);
			Assert.AreEqual(0x12, buffer[0]);
			Assert.AreEqual(0x34, buffer[1]);

			// Leave container should skip remaining 2 bytes and return to parent
			_reader.LeaveContainer();

			// Should now be able to read the next element at parent level
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(nextElementId, _reader.ElementId);
			Assert.AreEqual(1, _reader.ElementSize);
			var singleByteBuffer = new byte[1];
			Assert.AreEqual(1, _reader.ReadBinary(singleByteBuffer, 0, 1));
			Assert.AreEqual(0x99, singleByteBuffer[0]);
		}

		[Test]
		public void LeaveContainer_WithNestedContainers_ReturnsToCorrectParentLevel()
		{
			var level1Id = VInt.MakeId(0x80);
			var level2Id = VInt.MakeId(0x81);
			var level3Id = VInt.MakeId(0x82);
			var leafId = VInt.MakeId(0x83);
			var nextElementId = VInt.MakeId(0x84);

			// Build nested structure: level1 -> level2 -> level3 -> leaf
			var level3Data = new MemoryStream();
			leafId.Write(level3Data);
			VInt.EncodeSize(1UL).Write(level3Data);
			level3Data.WriteByte(0x42);

			var level2Data = new MemoryStream();
			level3Id.Write(level2Data);
			VInt.EncodeSize((ulong)level3Data.Length).Write(level2Data);
			level2Data.Write(level3Data.ToArray(), 0, (int)level3Data.Length);

			var level1Data = new MemoryStream();
			level2Id.Write(level1Data);
			VInt.EncodeSize((ulong)level2Data.Length).Write(level1Data);
			level1Data.Write(level2Data.ToArray(), 0, (int)level2Data.Length);

			WriteElement(level1Id, level1Data.ToArray());
			WriteElement(nextElementId, new byte[] { 0x99 });

			StartReader();

			// Navigate to level 1
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(level1Id, _reader.ElementId);
			_reader.EnterContainer();

			// Navigate to level 2
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(level2Id, _reader.ElementId);
			_reader.EnterContainer();

			// Navigate to level 3
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(level3Id, _reader.ElementId);
			_reader.EnterContainer();

			// Read the leaf element
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(leafId, _reader.ElementId);
			var leafBuffer = new byte[1];
			Assert.AreEqual(1, _reader.ReadBinary(leafBuffer, 0, 1));
			Assert.AreEqual(0x42, leafBuffer[0]);

			// Leave from level 3 back to level 2
			_reader.LeaveContainer();
			Assert.IsFalse(_reader.ReadNext()); // No more elements in level 2

			// Leave from level 2 back to level 1
			_reader.LeaveContainer();
			Assert.IsFalse(_reader.ReadNext()); // No more elements in level 1

			// Leave from level 1 back to root
			_reader.LeaveContainer();

			// Should now be able to read the next element at root level
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(nextElementId, _reader.ElementId);
			var nextBuffer = new byte[1];
			Assert.AreEqual(1, _reader.ReadBinary(nextBuffer, 0, 1));
			Assert.AreEqual(0x99, nextBuffer[0]);
		}

		[Test]
		public void LeaveContainer_WithoutReadingAllChildren_SkipsRemainingElements()
		{
			var containerId = VInt.MakeId(0x80);
			var child1Id = VInt.MakeId(0x81);
			var child2Id = VInt.MakeId(0x82);
			var nextElementId = VInt.MakeId(0x83);

			var containerData = new MemoryStream();

			// Child 1
			child1Id.Write(containerData);
			VInt.EncodeSize(1UL).Write(containerData);
			containerData.WriteByte(0x11);

			// Child 2
			child2Id.Write(containerData);
			VInt.EncodeSize(1UL).Write(containerData);
			containerData.WriteByte(0x22);

			var containerBytes = containerData.ToArray();
			WriteElement(containerId, containerBytes);
			WriteElement(nextElementId, [0x99]);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(containerId, _reader.ElementId);

			_reader.EnterContainer();

			// Read only the first child
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(child1Id, _reader.ElementId);
			var child1Buffer = new byte[1];
			Assert.AreEqual(1, _reader.ReadBinary(child1Buffer, 0, 1));
			Assert.AreEqual(0x11, child1Buffer[0]);

			// Leave container without reading child2
			_reader.LeaveContainer();

			// Should be able to read the next element at root level
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(nextElementId, _reader.ElementId);
			var nextBuffer = new byte[1];
			Assert.AreEqual(1, _reader.ReadBinary(nextBuffer, 0, 1));
			Assert.AreEqual(0x99, nextBuffer[0]);
		}

		[Test]
		public void LeaveContainer_AfterReadingPartialChild_SkipsRemainingChildData()
		{
			var containerId = VInt.MakeId(0x80);
			var childId = VInt.MakeId(0x81);
			var nextElementId = VInt.MakeId(0x82);

			var containerData = new MemoryStream();
			childId.Write(containerData);
			VInt.EncodeSize(4UL).Write(containerData); // Child has 4 bytes
			containerData.WriteByte(0x11);
			containerData.WriteByte(0x22);
			containerData.WriteByte(0x33);
			containerData.WriteByte(0x44);

			var containerBytes = containerData.ToArray();
			WriteElement(containerId, containerBytes);
			WriteElement(nextElementId, new byte[] { 0x99 });

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(containerId, _reader.ElementId);

			_reader.EnterContainer();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(childId, _reader.ElementId);
			Assert.AreEqual(4, _reader.ElementSize);

			// Read only first 2 bytes of the 4-byte child
			var partialBuffer = new byte[2];
			Assert.AreEqual(2, _reader.ReadBinary(partialBuffer, 0, 2));
			Assert.AreEqual(0x11, partialBuffer[0]);
			Assert.AreEqual(0x22, partialBuffer[1]);

			// Leave container - should skip remaining 2 bytes of child and exit container
			_reader.LeaveContainer();

			// Should be able to read next element at root level
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(nextElementId, _reader.ElementId);
			var nextBuffer = new byte[1];
			Assert.AreEqual(1, _reader.ReadBinary(nextBuffer, 0, 1));
			Assert.AreEqual(0x99, nextBuffer[0]);
		}

		[Test]
		public void LeaveContainer_WithMultipleUnreadChildren_SkipsAllRemainingChildren()
		{
			var containerId = VInt.MakeId(0x80);
			var child1Id = VInt.MakeId(0x81);
			var child2Id = VInt.MakeId(0x82);
			var nextElementId = VInt.MakeId(0x83);

			var containerData = new MemoryStream();

			// Child 1 - will be read
			child1Id.Write(containerData);
			VInt.EncodeSize(1UL).Write(containerData);
			containerData.WriteByte(0x11);

			// Child 2 - will be skipped
			child2Id.Write(containerData);
			VInt.EncodeSize(2UL).Write(containerData);
			containerData.WriteByte(0x22);
			containerData.WriteByte(0x33);

			var containerBytes = containerData.ToArray();
			WriteElement(containerId, containerBytes);
			WriteElement(nextElementId, new byte[] { 0x99 });

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(containerId, _reader.ElementId);

			_reader.EnterContainer();

			// Read first child completely
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(child1Id, _reader.ElementId);
			var child1Buffer = new byte[1];
			Assert.AreEqual(1, _reader.ReadBinary(child1Buffer, 0, 1));
			Assert.AreEqual(0x11, child1Buffer[0]);

			// Leave container without reading child2
			_reader.LeaveContainer();

			// Should be able to read next element at root level
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(nextElementId, _reader.ElementId);
			var nextBuffer = new byte[1];
			Assert.AreEqual(1, _reader.ReadBinary(nextBuffer, 0, 1));
			Assert.AreEqual(0x99, nextBuffer[0]);
		}

		[Test]
		public void LeaveContainer_WithEmptyContainer_ReturnsToParentSuccessfully()
		{
			var containerId = VInt.MakeId(0x80);
			var nextElementId = VInt.MakeId(0x81);

			// Empty container
			WriteElement(containerId, new byte[0]);
			WriteElement(nextElementId, new byte[] { 0x99 });

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(containerId, _reader.ElementId);
			Assert.AreEqual(0, _reader.ElementSize);

			_reader.EnterContainer();
			Assert.IsFalse(_reader.ReadNext()); // No children in empty container

			_reader.LeaveContainer();

			// Should be able to read next element at root level
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(nextElementId, _reader.ElementId);
			var nextBuffer = new byte[1];
			Assert.AreEqual(1, _reader.ReadBinary(nextBuffer, 0, 1));
			Assert.AreEqual(0x99, nextBuffer[0]);
		}

		#endregion

		#region Position Tests

		[Test]
		public void ElementPosition_AfterReadingElement_ReturnsCorrectPosition()
		{
			var elementId = VInt.MakeId(0x80);
			var data = new byte[] { 0x01, 0x02, 0x03 };

			WriteElement(elementId, data);

			StartReader();

			var initialPosition = _stream.Position;
			Assert.IsTrue(_reader.ReadNext());

			// ElementPosition should be where the element started
			Assert.AreEqual(initialPosition, _reader.ElementPosition);
		}

		[Test]
		public void ReadAt_WithValidPosition_ReadsElementAtPosition()
		{
			var elementId1 = VInt.MakeId(0x80);
			var elementId2 = VInt.MakeId(0x81);
			var data1 = new byte[] { 0x01, 0x02 };
			var data2 = new byte[] { 0x03, 0x04 };

			WriteElement(elementId1, data1);
			var element2Position = _stream.Position;
			WriteElement(elementId2, data2);

			StartReader();

			Assert.IsTrue(_reader.ReadAt(element2Position));
			Assert.AreEqual(elementId2, _reader.ElementId);
		}

		[Test]
		public void ReadAt_WithNegativePosition_ThrowsEbmlDataFormatException()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, new byte[] { 0x01 });

			StartReader();
			Assert.IsTrue(_reader.ReadNext());

			Assert.Throws<EbmlDataFormatException>(() => _reader.ReadAt(-1));
		}

		#endregion

		#region Error Handling Tests

		[Test]
		public void ReadInt_WithInvalidElementSize_ThrowsEbmlDataFormatException()
		{
			var elementId = VInt.MakeId(0x80);
			var data = new byte[9]; // Too large for signed integer
			WriteElement(elementId, data);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.Throws<EbmlDataFormatException>(() => _reader.ReadInt());
		}

		[Test]
		public void ReadUInt_WithInvalidElementSize_ThrowsEbmlDataFormatException()
		{
			var elementId = VInt.MakeId(0x80);
			var data = new byte[9]; // Too large for unsigned integer
			WriteElement(elementId, data);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.Throws<EbmlDataFormatException>(() => _reader.ReadUInt());
		}

		[Test]
		public void ReadFloat_WithInvalidElementSize_ThrowsEbmlDataFormatException()
		{
			var elementId = VInt.MakeId(0x80);
			var data = new byte[6]; // Invalid size for float (must be 4 or 8)
			WriteElement(elementId, data);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.Throws<EbmlDataFormatException>(() => _reader.ReadFloat());
		}

		[Test]
		public void ReadDate_WithInvalidElementSize_ThrowsEbmlDataFormatException()
		{
			var elementId = VInt.MakeId(0x80);
			var data = new byte[6]; // Invalid size for date (must be 8 or 0)
			WriteElement(elementId, data);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.Throws<EbmlDataFormatException>(() => _reader.ReadDate());
		}

		[Test]
		public void ReadBinary_WhenElementAlreadyAccessedAsOtherType_ThrowsInvalidOperationException()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, 12345L);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			_reader.ReadInt(); // Access as integer

			var buffer = new byte[8];
			Assert.Throws<InvalidOperationException>(() => _reader.ReadBinary(buffer, 0, buffer.Length));
		}

		#endregion

		#region Edge Cases

		[Test]
		public void ReadNext_WithZeroSizeElement_HandlesCorrectly()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, new byte[0]);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(0, _reader.ElementSize);
		}

		[Test]
		public void ReadInt_WithZeroSizeElement_ReturnsZero()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, new byte[0]);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(0L, _reader.ReadInt());
		}

		[Test]
		public void ReadUInt_WithZeroSizeElement_ReturnsZero()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, new byte[0]);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(0UL, _reader.ReadUInt());
		}

		[Test]
		public void ReadFloat_WithZeroSizeElement_ReturnsZero()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, new byte[0]);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(0.0, _reader.ReadFloat());
		}

		[Test]
		public void ReadDate_WithZeroSizeElement_ReturnsMilleniumStart()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, new byte[0]);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc), _reader.ReadDate());
		}

		[Test]
		public void ReadAscii_WithZeroSizeElement_ReturnsEmptyString()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, new byte[0]);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual("", _reader.ReadAscii());
		}

		[Test]
		public void ReadUtf_WithZeroSizeElement_ReturnsEmptyString()
		{
			var elementId = VInt.MakeId(0x80);
			WriteElement(elementId, new byte[0]);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			Assert.AreEqual("", _reader.ReadUtf());
		}

		#endregion

		#region EBML Specification Compliance Tests

		[Test]
		public void ReadNext_WithNonShortestElementIdEncoding_ThrowsEbmlDataFormatException()
		{
			// Create an Element ID that is not encoded in the shortest possible form
			// This violates EBML specification
			_stream.WriteByte(0x40); // 2-byte VINT marker
			_stream.WriteByte(0x7F); // Data that could be encoded as 1-byte VINT (since 0x7F < 0x80)
			_stream.WriteByte(0x81); // Size = 1
			_stream.WriteByte(0x00); // Data

			StartReader();
			Assert.Throws<EbmlDataFormatException>(() => _reader.ReadNext());
		}

		[Test]
		public void ReadNext_WithAllZeroElementId_ThrowsEbmlDataFormatException()
		{
			// Element ID with all VINT_DATA bits set to zero is invalid
			_stream.WriteByte(0x80); // 1-byte VINT with all data bits as zero
			_stream.WriteByte(0x81); // Size = 1
			_stream.WriteByte(0x00); // Data

			StartReader();
			Assert.Throws<EbmlDataFormatException>(() => _reader.ReadNext());
		}

		[Test]
		public void ReadAscii_WithNullTerminator_HandlesCorrectly()
		{
			var elementId = VInt.MakeId(0x80);
			var data = Encoding.ASCII.GetBytes("Hello\0World\0");
			WriteElement(elementId, data);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			// According to EBML spec, string should be terminated at first null octet
			Assert.AreEqual("Hello", _reader.ReadAscii());
		}

		[Test]
		public void ReadUtf_WithNullTerminator_HandlesCorrectly()
		{
			var elementId = VInt.MakeId(0x80);
			var data = Encoding.UTF8.GetBytes("Hello\0World\0");
			WriteElement(elementId, data);

			StartReader();
			Assert.IsTrue(_reader.ReadNext());
			// According to EBML spec, string should be terminated at first null octet
			Assert.AreEqual("Hello", _reader.ReadUtf());
		}

		#endregion

		#region Helper Methods for Advanced Testing

		private void WriteInvalidElementId(byte[] invalidId)
		{
			_stream.Write(invalidId, 0, invalidId.Length);
			_stream.WriteByte(0x81); // Size = 1
			_stream.WriteByte(0x00); // Data
		}

		private void WriteElementWithCustomSize(VInt elementId, byte[] sizeBytes, byte[] data)
		{
			elementId.Write(_stream);
			_stream.Write(sizeBytes, 0, sizeBytes.Length);
			_stream.Write(data, 0, data.Length);
		}

		private void WriteRawBytes(params byte[] bytes)
		{
			_stream.Write(bytes, 0, bytes.Length);
		}

		#endregion
	}
}
