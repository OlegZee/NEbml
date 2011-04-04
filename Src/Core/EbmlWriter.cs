using System;
using System.IO;
using System.Linq;
using System.Text;

namespace NEbml.Core
{
	/// <summary>
	/// Writes EBML file
	/// </summary>
	public class EbmlWriter
	{
		protected readonly Stream _stream;

		public EbmlWriter(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			_stream = stream;
		}

		public MasterBlockWriter StartMasterElement(VInt elementId)
		{
			return new MasterBlockWriter(this, elementId);
		}

		public int WriteElementHeader(VInt elementId, VInt size)
		{
			return elementId.Write(_stream) + size.Write(_stream);
		}

		#region Writing atomic elements

		/// <summary>
		/// Writes signed integer value
		/// </summary>
		/// <param name="elementId"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public int Write(VInt elementId, Int64 value)
		{
			var length = value == 0 ? 0u : 1u;
			unchecked
			{
				var mask = (long)0xffffffffffffff80L;
				for (var expected = value < 0L ? mask : 0L; length < 8 && (mask & value) != expected; mask <<= 8, expected <<= 8)
				{
					length++;
				}
			}

			return elementId.Write(_stream) + EncodeWidth(length).Write(_stream) + WriteInt(value, length);
		}

		/// <summary>
		/// Writes unsigned integer value
		/// </summary>
		/// <param name="elementId"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public int Write(VInt elementId, UInt64 value)
		{
			var length = 0u;
			for (var mask = UInt64.MaxValue; (value & mask) != 0 && length < 8; mask <<= 8)
			{
				length++;
			}

			return elementId.Write(_stream) + EncodeWidth(length).Write(_stream) + WriteUInt(value, length);
		}

		/// <summary>
		/// Writes datetime value
		/// </summary>
		/// <param name="elementId"></param>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public int Write(VInt elementId, DateTime dateTime)
		{
			var d = (dateTime - EbmlReader.MilleniumStart).Ticks * 100;
			return elementId.Write(_stream) + EncodeWidth(8).Write(_stream) + WriteInt(d, 8);
		}

		/// <summary>
		/// Writes floating point number
		/// </summary>
		/// <param name="elementId"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public unsafe int Write(VInt elementId, float value)
		{
			var u = *(((UInt32*)&value)); 
			return elementId.Write(_stream) + EncodeWidth(4).Write(_stream) + WriteInt(u, 4);
		}

		/// <summary>
		/// Writes floating point number with double precision
		/// </summary>
		/// <param name="elementId"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public unsafe int Write(VInt elementId, double value)
		{
			var u = *(((UInt64*)&value)); 
			return elementId.Write(_stream) + EncodeWidth(8).Write(_stream) + WriteUInt(u, 8);
		}

		/// <summary>
		/// Writes binary data
		/// </summary>
		/// <param name="elementId"></param>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public int Write(VInt elementId, byte[] data, int offset, int length)
		{
			var headLen = elementId.Write(_stream) + EncodeWidth((uint) length).Write(_stream);
			_stream.Write(data, offset, length);
			return headLen + length;
		}

		/// <summary>
		/// Writes binary data
		/// </summary>
		/// <param name="elementId"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public int Write(VInt elementId, byte[] data)
		{
			return Write(elementId, data, 0, data.Length);
		}

		/// <summary>
		/// Writes string in ASCII encoding
		/// </summary>
		/// <param name="elementId"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public int WriteAscii(VInt elementId, string value)
		{
			if (value == null) throw new ArgumentNullException("value");
			var buffer = string.IsNullOrEmpty(value) ? new byte[0] : Encoding.ASCII.GetBytes(value);
			return Write(elementId, buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Writes string in UTF-8 encoding
		/// </summary>
		/// <param name="elementId"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public int WriteUtf(VInt elementId, string value)
		{
			if (value == null) throw new ArgumentNullException("value");
			if (!value.IsNormalized(NormalizationForm.FormC))
			{
				value = value.Normalize(NormalizationForm.FormC);
			}
			var buffer = string.IsNullOrEmpty(value) ? new byte[0] : Encoding.UTF8.GetBytes(value);
			return Write(elementId, buffer, 0, buffer.Length);
		}

		#endregion

		public int Write(byte[] buffer, int offset, int length)
		{
			_stream.Write(buffer, offset, length);
			return length;
		}

		#region Implementation

		private int WriteInt(Int64 value, uint length)
		{
			if(length <0 || length > 8) throw new ArgumentOutOfRangeException("length");
			if (value == 0 && length == 0) return 0;

			var buffer = new byte[length];

			var p = (int)length;
			for (var data = value; --p >= 0; data >>= 8)
			{
				buffer[p] = (byte)(data & 0xff);
			}

			return Write(buffer, 0, (int)length);
		}

		private int WriteUInt(UInt64 value, uint length)
		{
			if (length < 0 || length > 8) throw new ArgumentOutOfRangeException("length");
			if (value == 0 && length == 0) return 0;

			var buffer = new byte[length];

			var p = (int)length;
			for (var data = value; --p >= 0; data >>= 8)
			{
				buffer[p] = (byte)(data & 0xff);
			}

			return Write(buffer, 0, (int) length);
		}

		[ThreadStatic] // avoid thread sync code
		private static VInt[] __smallSizes;

		private const int CachedWidthCount = 20;

		private static VInt EncodeWidth(uint width)
		{
			if (__smallSizes == null)
			{
				__smallSizes = Enumerable.Range(0, CachedWidthCount).Select(i => VInt.EncodeSize((ulong)i)).ToArray();
			}
			return width < CachedWidthCount ? __smallSizes[width] : VInt.EncodeSize(width);
		}

		#endregion
	}

	/// <summary>
	/// Supplementary EbmlWriter implementation for use with StartMasterElement method
	/// </summary>
	public class MasterBlockWriter : EbmlWriter, IDisposable
	{
		private readonly Action _flushImpl;

		internal MasterBlockWriter(EbmlWriter writer, VInt elementId)
			: base(new MemoryStream())
		{
			bool flushed = false;
			_flushImpl = () =>
				{
					if (!flushed)
					{
						var data = ((MemoryStream) _stream).ToArray();
						writer.Write(elementId, data);

						flushed = true;
						_stream.Close();
					}
				};

		}

		/// <summary>
		/// Flushed the writer data to master stream
		/// </summary>
		public void FlushData()
		{
			_flushImpl();
		}

		public void Dispose()
		{
			FlushData();
		}
	}

}
