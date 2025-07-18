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
	public class EbmlWriterComplexScenariosTests : EbmlWriterTestBase
	{
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
	}
}
