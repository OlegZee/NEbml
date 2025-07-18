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

namespace NEbml.Core
{
	/// <summary>
	/// Abstract base class for EBML master element writing strategies (Buffered, Backpatching, etc).
	/// Provides the contract for flushing and disposing master element writers.
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the <see cref="MasterElementWriterBase"/> class.
	/// </remarks>
	/// <param name="stream">The stream to write to.</param>
	public abstract class MasterElementWriterBase(Stream stream) : EbmlWriter(stream), IDisposable
	{
		private bool _flushed;
		/// <summary>
		/// Delegate that implements the flush logic for the master element writer.
		/// </summary>
		protected Action FlushImpl { get; set; }

		/// <summary>
		/// Gets the buffer stream used for writing the master element.
		/// </summary>
		protected Stream BufferStream => BaseStream;

		/// <summary>
		/// Disposes the master element writer and flushes any pending data.
		/// </summary>
		public void Dispose()
		{
			if (_flushed) return;
			FlushImpl();
			_flushed = true;
		}
	}
}
