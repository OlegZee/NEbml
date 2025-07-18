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
	/// <summary>
	/// Base class for EbmlWriter test fixtures providing common setup and utility methods.
	/// </summary>
	public abstract class EbmlWriterTestBase
	{
		protected Stream _stream;
		protected static readonly VInt ElementId = VInt.MakeId(123);
		protected EbmlWriter _writer;

		[SetUp]
		public virtual void Setup()
		{
			_stream = new MemoryStream();
			_writer = new EbmlWriter(_stream);
		}

		protected EbmlReader StartRead()
		{
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(ElementId, reader.ElementId);

			return reader;
		}

		protected byte[] ReadAllBinary(EbmlReader reader)
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

		protected static void AssertRead<T>(EbmlReader reader, uint elementId, T value, Func<EbmlReader, T> read)
		{
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(VInt.MakeId(elementId), reader.ElementId);
			Assert.AreEqual(value, read(reader));
		}

		#region Test Data

		protected static readonly DateTime[] TestDatetimeData = new[]
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
	}
}
