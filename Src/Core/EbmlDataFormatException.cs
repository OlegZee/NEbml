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
