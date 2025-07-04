﻿/* Copyright (c) 2011-2020 Oleg Zee

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
	/// Supplementary EbmlWriter implementation for use with StartMasterElement method
	/// </summary>
	public class MasterBlockWriter : EbmlWriter, IDisposable
	{
		private readonly Action _flushImpl;

		internal MasterBlockWriter(EbmlWriter writer, VInt elementId)
			: base(new MemoryStream())
		{
			var flushed = false;
			_flushImpl = () =>
				{
					if (flushed) return;
					var data = ((MemoryStream) _stream).ToArray();
					writer.Write(elementId, data);

					flushed = true;
					_stream.Close();
				};
		}

		/// <summary>
		/// Flushed the writer data to master stream
		/// </summary>
		public void FlushData()
		{
			_flushImpl();
		}

		/// <summary>
		/// Disposes the master block writer and flushes any pending data
		/// </summary>
		public void Dispose()
		{
			FlushData();
		}
	}
}