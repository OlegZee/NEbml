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
