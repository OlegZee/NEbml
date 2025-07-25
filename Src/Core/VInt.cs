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
using System.Diagnostics;
using System.IO;

namespace NEbml.Core
{
	/// <summary>
	/// Variable size integer implementation as of http://www.matroska.org/technical/specs/rfc/index.html
	/// </summary>
	public readonly struct VInt
	{
		private readonly sbyte _length;

		private VInt(ulong encodedValue, int length)
		{
			if (length < 1 || length > 8)
				throw new ArgumentOutOfRangeException(nameof(length));

			EncodedValue = encodedValue;
			_length = (sbyte)length;
		}

		#region public API

		/// <summary>
		/// Gets the value
		/// </summary>
		public ulong Value => EncodedValue & DataBitsMask[_length];

		/// <summary>
		/// Gets true if value is reserved (i.e. all data bits are zeros or 1's)
		/// </summary>
		public bool IsReserved => Value == DataBitsMask[_length];

		/// <summary>
		/// Gets true if value is correct identifier
		/// </summary>
		public bool IsValidIdentifier
		{
			get
			{
				var isShortest = _length == 1 || Value > DataBitsMask[_length - 1];
				return isShortest && !IsReserved;
			}
		}

		/// <summary>
		/// Gets the encoded value.
		/// </summary>
		public ulong EncodedValue { get; }

		/// <summary>
		/// Gets the encoded value's length
		/// </summary>
		public int Length => _length;

		/// <summary>
		/// Implicitly converts VInt to nullable ulong, returning null for reserved values
		/// </summary>
		/// <param name="value">The VInt value to convert</param>
		/// <returns>The ulong value or null if reserved</returns>
		public static implicit operator ulong?(VInt value)
		{
			return !value.IsReserved ? value.Value : (ulong?)null;
		}

		#endregion

		#region constructors

		/// <summary>
		/// Returns VInt value with specific encoded value with a specific length.
		/// </summary>
		/// <param name="value">(Size) value to be encoded.</param>
		/// <param name="length">VInt length to store value to.</param>
		/// <returns></returns>
		public static VInt EncodeSize(ulong value, int length = 0)
		{
			if (value > MaxSizeValue)
				throw new ArgumentException("Value exceed VInt capacity", nameof(value));
			if (length < 0 || length > 8)
				throw new ArgumentOutOfRangeException(nameof(length));
			if (length > 0 && DataBitsMask[length] <= value)
				throw new ArgumentException("Specified width is not sufficient to encode value", nameof(value));

			if (length == 0)
			{
				while (DataBitsMask[++length] <= value) { }
			}

			var sizeMarker = 1UL << (7 * length);
			return new VInt(value | sizeMarker, length);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="elementId"></param>
		/// <returns></returns>
		public static VInt MakeId(uint elementId)
		{
			if (elementId > MaxElementIdValue)
				throw new ArgumentException("Value exceed VInt capacity", nameof(elementId));

			var id = EncodeSize(elementId);
			Debug.Assert(id._length <= 4);
			return id;
		}

		/// <summary>
		/// Creates VInt for unknown size (all databits are 1's)
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		public static VInt UnknownSize(int length)
		{
			if (length < 0 || length > 8)
				throw new ArgumentOutOfRangeException(nameof(length));

			var sizeMarker = 1UL << (7 * length);
			var dataBits = (1UL << (7 * length)) - 1;
			return new VInt(sizeMarker | dataBits, length);
		}

		/// <summary>
		/// Constructs the VInt value from its encoded form
		/// </summary>
		/// <param name="encodedValue"></param>
		/// <returns></returns>
		public static VInt FromEncoded(ulong encodedValue)
		{
			if (encodedValue == 0)
				throw new ArgumentException("Zero is not a correct value", nameof(encodedValue));

			var mostSignificantOctetIndex = 7;
			while ((encodedValue >> mostSignificantOctetIndex * 8) == 0x0)
			{
				mostSignificantOctetIndex--;
			}

			var marker = (byte)((encodedValue >> mostSignificantOctetIndex * 8) & 0xff);
			var extraBytes = (marker >> 4 > 0) ? ExtraBytesSize[marker >> 4] : 4 + ExtraBytesSize[marker];

			if (extraBytes != mostSignificantOctetIndex)
				throw new ArgumentException("Width marker does not match its position", nameof(encodedValue));

			return new VInt(encodedValue, extraBytes + 1);
		}

		#endregion

		#region Persisting

		/// <summary>
		/// Reads the value from the stream
		/// </summary>
		/// <param name="source"></param>
		/// <param name="maxLength">Maximal expected length (either 4 or 8)</param>
		/// <param name="buffer">The buffer for optimization purposes. Must match the maxlength</param>
		/// <returns></returns>
		public static VInt Read(Stream source, int maxLength, byte[] buffer)
		{
			buffer = buffer ?? new byte[maxLength];

			if (source.ReadFully(buffer, 0, 1) == 0)
			{
				throw new EndOfStreamException();
			}

			if (buffer[0] == 0)
				throw new EbmlDataFormatException("Length bigger than 8 is not supported");
			// Note: EBMLMaxSizeWidth is not currently supported - would require additional EBML header parsing

			var extraBytes = (buffer[0] & 0xf0) != 0
				? ExtraBytesSize[buffer[0] >> 4]
				: 4 + ExtraBytesSize[buffer[0]];

			if (extraBytes + 1 > maxLength)
				throw new EbmlDataFormatException($"Expected VInt with a max length of {maxLength}. Got {extraBytes + 1}");

			if (source.ReadFully(buffer, 1, extraBytes) != extraBytes)
			{
				throw new EndOfStreamException();
			}

			ulong encodedValue = buffer[0];
			for (var i = 0; i < extraBytes; i++)
			{
				encodedValue = encodedValue << 8 | buffer[i + 1];
			}
			return new VInt(encodedValue, extraBytes + 1);
		}

		/// <summary>
		/// Writes value to stream
		/// </summary>
		/// <param name="stream"></param>
		public int Write(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));

			var buffer = new byte[Length];

			int p = Length;
			for (var data = EncodedValue; --p >= 0; data >>= 8)
			{
				buffer[p] = (byte)(data & 0xff);
			}

			stream.Write(buffer, 0, buffer.Length);
			return buffer.Length;
		}

		private static readonly sbyte[] ExtraBytesSize =
		{ 4, 3, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };

		/// <summary>
		/// Maps length to data bits mask
		/// </summary>
		private static readonly ulong[] DataBitsMask =
		{
			(1L << 0) - 1,
			(1L << 7) - 1,
			(1L << 14) - 1,
			(1L << 21) - 1,
			(1L << 28) - 1,
			(1L << 35) - 1,
			(1L << 42) - 1,
			(1L << 49) - 1,
			(1L << 56) - 1
		};

		private const ulong MaxValue = (1L << 56) - 1;
		/// <summary>
		/// All 1's is reserved for unknown size
		/// </summary>
		private const ulong MaxSizeValue = MaxValue - 1;
		private const ulong MaxElementIdValue = (1 << 28) - 1;

		#endregion

		#region Equality members

		/// <summary>
		/// Returns the hash code for this VInt
		/// </summary>
		/// <returns>A hash code for the current object</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (EncodedValue.GetHashCode() * 397) ^ _length.GetHashCode();
			}
		}

		private bool Equals(VInt other)
		{
			return other.EncodedValue == EncodedValue && other._length == _length;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current VInt
		/// </summary>
		/// <param name="obj">The object to compare with the current VInt</param>
		/// <returns>true if the specified object is equal to the current VInt; otherwise, false</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is VInt i && Equals(i);
		}

		#endregion

		/// <summary>
		/// Returns a string representation of this VInt
		/// </summary>
		/// <returns>A string that represents the current VInt</returns>
		public override string ToString()
		{
			return $"VInt, value = {Value}, length = {Length}, encoded = {EncodedValue:X}";
		}
	}
}
