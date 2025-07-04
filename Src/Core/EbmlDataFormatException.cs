using System;

namespace NEbml.Core
{
	/// <summary>
	/// Thrown to indicate the EBML data format violation.
	/// </summary>
	public class EbmlDataFormatException : System.IO.IOException
	{
		/// <summary>
		/// Initializes a new instance of the EbmlDataFormatException class
		/// </summary>
		public EbmlDataFormatException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the EbmlDataFormatException class with a specified error message
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public EbmlDataFormatException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the EbmlDataFormatException class with a specified error message and a reference to the inner exception
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		/// <param name="cause">The exception that is the cause of the current exception</param>
		public EbmlDataFormatException(string message, Exception cause)
			: base(message, cause)
		{
		}
	}
}
