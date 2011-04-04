using System;
using System.Text;

namespace NEbml.Core
{
	/// <summary>
	/// Utility methods for EBML element types
	/// </summary>
	public static class EncodeImpl
	{
		/// <summary>
		/// Returns the minimum number of bytes required to encode the specified value as a signed integer.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>the minimum size of the value in the signed integer encoded form</returns>
		public static int GetMinimumSignedIntegerEncodedSize(long value)
		{
			if (value == 0L)
			{
				return 0;
			}

			unchecked
			{
				var mask = (long)0xffffffffffffff80L;
				var expected = value < 0L ? mask : 0L;
				var encodedSize = 1;
				while (encodedSize < 8 && (mask & value) != expected)
				{
					mask <<= 8;
					expected <<= 8;
					encodedSize++;
				}
				return encodedSize;
			}
		}

		/// <summary>
		/// Returns the minimum number of bytes required to encode the specified value as an unsigned integer.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>the minimum size of the value in the unsigned integer encoded form</returns>
		public static int GetMinimumUnsignedIntegerEncodedSize(ulong value)
		{
			var mask = 0xffffffffffffffffL;
			var encodedSize = 0;
			while (encodedSize < 8 && (mask & value) != 0L)
			{
				mask <<= 8;
				encodedSize++;
			}
			return encodedSize;
		}

		/// <summary>
		/// Returns the minimum number of bytes required to encode the specified value in the UTF-8 encoding.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>the minimum size of the value in the UTF-8 encoding</returns>
		public static long GetMinimumUtf8StringEncodedSize(String value)
		{
			if (value == null) throw new ArgumentNullException("value");

			if (value.Length == 0)
			{
				return 0;
			}

			if (!value.IsNormalized(NormalizationForm.FormC))
			{
				value = value.Normalize(NormalizationForm.FormC);
			}

			Encoding encoder = new UTF8Encoding(true, true);
			return encoder.GetByteCount(value);
		}

	}
}