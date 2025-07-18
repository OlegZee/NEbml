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
	/// Extension methods for Stream operations
	/// </summary>
	public static class StreamExtensions
	{
		/// <summary>
		/// Reads data from a stream until the requested number of bytes is read or end of stream is reached
		/// </summary>
		/// <param name="stream">The stream to read from</param>
		/// <param name="buffer">The buffer to store read data</param>
		/// <param name="offset">The offset in the buffer to start storing data</param>
		/// <param name="count">The number of bytes to read</param>
		/// <returns>The total number of bytes read</returns>
		public static int ReadFully(this Stream stream, byte[] buffer, int offset, int count)
		{
			int bytesRead = 0;
			int totalBytesRead = 0;

			do
			{
				bytesRead = stream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
				totalBytesRead += bytesRead;
			} while (bytesRead > 0 && totalBytesRead < count);

			return totalBytesRead;
		}
	}
}
