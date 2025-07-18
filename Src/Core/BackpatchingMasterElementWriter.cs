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
	/// Implements the Backpatching strategy for writing EBML master elements, where the size field is backpatched after writing content.
	/// Used by <see cref="EbmlWriter.StartMasterElement(VInt, EbmlWriter.MasterElementSizeStrategy, int)"/> when the Backpatching strategy is selected.
	/// </summary>
	internal class BackpatchingMasterElementWriter : MasterElementWriterBase
	{
		internal BackpatchingMasterElementWriter(EbmlWriter parentWriter, VInt elementId, int sizeFieldLength = 8)
			: base(parentWriter.BaseStream)
		{
			var stream = parentWriter.BaseStream;
			// Write element ID
			elementId.Write(stream);

			// Reserve space for size (configurable)
			var sizePos = stream.Position;
			var zeroSize = new byte[sizeFieldLength];
			stream.Write(zeroSize, 0, sizeFieldLength);

			var contentStart = stream.Position;
			FlushImpl = () =>
			{
				long endPosition = stream.Position;
				long contentLength = endPosition - contentStart;
				var sizeVInt = VInt.EncodeSize((ulong)contentLength, sizeFieldLength);
				long currentPos = stream.Position;
				stream.Seek(sizePos, SeekOrigin.Begin);
				sizeVInt.Write(stream);
				int written = (int)(stream.Position - sizePos);
				if (written < sizeFieldLength)
				{
					var pad = new byte[sizeFieldLength - written];
					stream.Write(pad, 0, pad.Length);
				}
				stream.Seek(currentPos, SeekOrigin.Begin);
			};
		}
	}
}
