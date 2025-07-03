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
using System.Collections.Generic;
using System.IO;
using NEbml.Core;
using NUnit.Framework;

namespace Core.Tests
{
	[TestFixture]
	public class EbmlWriterTests
	{
		#region Setup/teardown

		private Stream _stream;
		private static readonly VInt ElementId = VInt.MakeId(123);
		private EbmlWriter _writer;

		[SetUp]
		public void Setup()
		{
			_stream = new MemoryStream();
			_writer = new EbmlWriter(_stream);
		}

		private EbmlReader StartRead()
		{
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(ElementId, reader.ElementId);

			return reader;
		}

		private byte[] ReadAllBinary(EbmlReader reader)
		{
			var data = new List<byte>();
			var buffer = new byte[1024];
			int bytesRead;
			while ((bytesRead = reader.ReadBinary(buffer, 0, buffer.Length)) > 0)
			{
				for (int i = 0; i < bytesRead; i++)
				{
					data.Add(buffer[i]);
				}
			}
			return data.ToArray();
		}
		#endregion

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
		[TestCaseSource("TestDatetimeData")]
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

		public enum WriteStrMode { Ascii, Utf };

		[TestCase(WriteStrMode.Ascii, (string)null)]
		[TestCase(WriteStrMode.Utf, (string)null)]
		public void WriteStringBadArg(WriteStrMode mode, string value)
		{
			Assert.Throws<ArgumentNullException>(() => {
				if(mode == WriteStrMode.Ascii) _writer.WriteAscii(ElementId, value);
				if(mode == WriteStrMode.Utf) _writer.WriteUtf(ElementId, value);

			});
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

		private static void AssertRead<T>(EbmlReader reader, uint elementId, T value, Func<EbmlReader, T> read)
		{
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(VInt.MakeId(elementId), reader.ElementId);
			Assert.AreEqual(value, read(reader));
		}

		#region test data

		private static readonly DateTime[] TestDatetimeData = new[]
			{
				new DateTime(2001, 01, 01),
				new DateTime(2001, 01, 01, 12, 10, 05, 123),
				new DateTime(2001, 10, 10),
				new DateTime(2101, 10, 10),
				new DateTime(1812, 10, 10),
				new DateTime(2001, 01, 01) + new TimeSpan(long.MaxValue/100),
				new DateTime(2001, 01, 01) + new TimeSpan(long.MinValue/100)
			};

		#endregion

		#region Constructor Tests

		[Test]
		public void Constructor_WithNullStream_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new EbmlWriter(null));
		}

		[Test]
		public void Constructor_WithValidStream_InitializesSuccessfully()
		{
			using (var stream = new MemoryStream())
			{
				var writer = new EbmlWriter(stream);
				Assert.IsNotNull(writer);
			}
		}

		#endregion

		#region String Validation Tests

		[Test]
		public void WriteAscii_WithNullString_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _writer.WriteAscii(ElementId, null));
		}

		[Test]
		public void WriteUtf_WithNullString_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _writer.WriteUtf(ElementId, null));
		}

		[TestCase("")]
		[TestCase("Hello World")]
		[TestCase("ASCII string with numbers 12345 and symbols !@#$%")]
		[TestCase("Special chars: \t\r\n")]
		public void WriteAscii_ValidStrings_RoundTrip(string value)
		{
			_writer.WriteAscii(ElementId, value);

			var reader = StartRead();
			var result = reader.ReadAscii();
			Assert.AreEqual(value, result);
		}

		[TestCase("")]
		[TestCase("Hello World")]
		[TestCase("UTF-8 string with unicode: 🌟🎉💫")]
		[TestCase("Cyrillic: Привет мир")]
		[TestCase("Chinese: 你好世界")]
		[TestCase("Japanese: こんにちは")]
		[TestCase("Arabic: مرحبا بالعالم")]
		[TestCase("Mixed: Hello 世界 🌍")]
		public void WriteUtf_ValidStrings_RoundTrip(string value)
		{
			_writer.WriteUtf(ElementId, value);

			var reader = StartRead();
			var result = reader.ReadUtf();
			Assert.AreEqual(value, result);
		}

		[Test]
		public void WriteUtf_NonNormalizedString_NormalizesToFormC()
		{
			// Create a non-normalized string (combining characters)
			var nonNormalized = "e\u0301"; // e + combining acute accent
			var expectedNormalized = "é"; // precomposed character

			_writer.WriteUtf(ElementId, nonNormalized);

			var reader = StartRead();
			var result = reader.ReadUtf();
			
			// Should be normalized to FormC
			Assert.AreEqual(expectedNormalized, result);
		}

		#endregion

		#region Binary Data Tests

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

		#endregion

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

		#region Master Element Tests

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

		#endregion

		#region Element Header Tests

		[Test]
		public void WriteElementHeader_WritesCorrectly()
		{
			var elementId = VInt.MakeId(123);
			var size = VInt.EncodeSize(456);
			
			var bytesWritten = _writer.WriteElementHeader(elementId, size);
			
			Assert.Greater(bytesWritten, 0);
			Assert.AreEqual(bytesWritten, _stream.Length);
		}

		#endregion

		#region Stream Position and Byte Count Tests

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

		#endregion

		#region Complex Scenarios Tests

		[Test]
		public void WriteMultipleElementTypes_InSequence_AllReadCorrectly()
		{
			// Write various element types in sequence
			_writer.Write(VInt.MakeId(1), 42L);
			_writer.Write(VInt.MakeId(2), 3.14159f);
			_writer.WriteAscii(VInt.MakeId(3), "ASCII");
			_writer.WriteUtf(VInt.MakeId(4), "UTF-8 🌟");
			_writer.Write(VInt.MakeId(5), new byte[] { 0xAB, 0xCD, 0xEF });
			_writer.Write(VInt.MakeId(6), DateTime.Now);

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Read and verify each element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(VInt.MakeId(1), reader.ElementId);
			Assert.AreEqual(42L, reader.ReadInt());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(VInt.MakeId(2), reader.ElementId);
			Assert.AreEqual(3.14159f, reader.ReadFloat(), 0.00001f);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(VInt.MakeId(3), reader.ElementId);
			Assert.AreEqual("ASCII", reader.ReadAscii());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(VInt.MakeId(4), reader.ElementId);
			Assert.AreEqual("UTF-8 🌟", reader.ReadUtf());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(VInt.MakeId(5), reader.ElementId);
			CollectionAssert.AreEqual(new byte[] { 0xAB, 0xCD, 0xEF }, ReadAllBinary(reader));

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(VInt.MakeId(6), reader.ElementId);
			// Just verify it's a valid DateTime (exact comparison might fail due to precision)
			Assert.DoesNotThrow(() => reader.ReadDate());
		}

		[Test]
		public void WriteEmptyStrings_BothEncodings_RoundTrip()
		{
			_writer.WriteAscii(VInt.MakeId(1), "");
			_writer.WriteUtf(VInt.MakeId(2), "");

			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("", reader.ReadAscii());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("", reader.ReadUtf());
		}

		#endregion
	}
}
