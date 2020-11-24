/* Copyright (c) 2011-2020 Oleg Zee

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

namespace NEbml.Core
{
	/// <summary>
	/// Root class for Ebml type schema
	/// </summary>
	public class DTDBase
	{
		protected static ElementDescriptor Container(long id)
		{
			return new ElementDescriptor(id, "", ElementType.MasterElement);
		}

		protected static ElementDescriptor Binary(long id)
		{
			return new ElementDescriptor(id, "", ElementType.Binary);
		}

		protected static ElementDescriptor Uint(long id)
		{
			return new ElementDescriptor(id, "", ElementType.UnsignedInteger);
		}

		protected static ElementDescriptor Int(long id)
		{
			return new ElementDescriptor(id, "", ElementType.SignedInteger);
		}

		protected static ElementDescriptor Ascii(long id)
		{
			return new ElementDescriptor(id, "", ElementType.AsciiString);
		}

		protected static ElementDescriptor Utf8(long id)
		{
			return new ElementDescriptor(id, "", ElementType.Utf8String);
		}

		protected static ElementDescriptor Float(long id)
		{
			return new ElementDescriptor(id, "", ElementType.Float);
		}

		protected static ElementDescriptor Date(long id)
		{
			return new ElementDescriptor(id, "", ElementType.Date);
		}

		public class MasterElementDescriptor:ElementDescriptor
		{
			protected MasterElementDescriptor(long identifier) : base(identifier, "", ElementType.MasterElement)
			{
			}
		}
	}

	/// <summary>
	/// Appendix C. EBML Standard definitions.
	/// </summary>
	public class StandardDtd:DTDBase
	{
		private StandardDtd() {}

		public static readonly ElementDescriptor
			EBML = new EBMLDesc(0x1a45dfa3);
		public static readonly ElementDescriptor
			SignatureSlot = new SignatureSlotDesc(0x1b538667);

		public static readonly ElementDescriptor
			CRC32 = Container(0xc3),
			Void = Binary(0xec);

		public sealed class EBMLDesc : MasterElementDescriptor
		{
			internal EBMLDesc(long id) : base(id) { }

			public static readonly ElementDescriptor
				EBMLVersion          = Uint(0x4286).Named("EBMLVersion"),
				EBMLReadVersion      = Uint(0x42f7).Named("EBMLReadVersion"),
				EBMLMaxIDLength      = Uint(0x42f2),
				EBMLMaxSizeLength    = Uint(0x42f3),
				DocType              = Ascii(0x4282).Named("DocType"),
				DocTypeVersion       = Uint(0x4287).Named("DocTypeVersion"),
				DocTypeReadVersion   = Uint(0x4285).Named("DocTypeReadVersion");
		}

		public sealed class SignatureSlotDesc : MasterElementDescriptor
		{
			internal SignatureSlotDesc(long id) : base(id) { }

			public readonly ElementDescriptor
				SignatureAlgo = Uint(0x7e8a),
				SignatureHash = Uint(0x7e9a),
				SignaturePublicKey = Binary(0x7ea5),
				Signature = Binary(0x7eb5),
				SignatureElements = Container(0x7e5b),
				SignatureElementList = Container(0x7e7b),
				SignedElement = Binary(0x6532);
		}
	}
}
