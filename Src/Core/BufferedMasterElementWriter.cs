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

using System.IO;

namespace NEbml.Core
{
	/// <summary>
	/// Implements the buffered (legacy) strategy for writing EBML master elements.
	/// Used by <see cref="EbmlWriter.StartMasterElement(VInt, EbmlWriter.MasterElementSizeStrategy, int)"/> when the Buffered strategy is selected.
	/// </summary>
	internal class BufferedMasterElementWriter : MasterElementWriterBase
	{
		internal BufferedMasterElementWriter(EbmlWriter writer, VInt elementId)
			: base(new MemoryStream())
		{
			FlushImpl = () =>
			{
				var buffer = (MemoryStream)BufferStream;

				// Get the buffered data from the memory stream
				var data = buffer.ToArray();
				// Write the element header with optimal size field length
				var sizeVInt = VInt.EncodeSize((ulong)data.Length);
				elementId.Write(writer.BaseStream);
				sizeVInt.Write(writer.BaseStream);
				// Write the buffered content
				writer.BaseStream.Write(data, 0, data.Length);
				buffer.Close();
			};
		}
	}
}
