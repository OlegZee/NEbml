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
	public class EbmlWriterConstructorAndValidationTests : EbmlWriterTestBase
	{
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
	}
}
