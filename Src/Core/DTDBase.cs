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

namespace NEbml.Core
{
	/// <summary>
	/// Root class for Ebml type schema
	/// </summary>
	public class DTDBase
	{
		/// <summary>
		/// Creates a master element descriptor (container)
		/// </summary>
		/// <param name="id">Element identifier</param>
		/// <param name="name">Element name</param>
		/// <returns>Element descriptor for a master element</returns>
		protected static ElementDescriptor Container(long id, string name = "")
		{
			return new ElementDescriptor(id, name, ElementType.MasterElement);
		}

		/// <summary>
		/// Creates a binary element descriptor
		/// </summary>
		/// <param name="id">Element identifier</param>
		/// <param name="name">Element name</param>
		/// <returns>Element descriptor for a binary element</returns>
		protected static ElementDescriptor Binary(long id, string name = "")
		{
			return new ElementDescriptor(id, name, ElementType.Binary);
		}

		/// <summary>
		/// Creates an unsigned integer element descriptor
		/// </summary>
		/// <param name="id">Element identifier</param>
		/// <param name="name">Element name</param>
		/// <returns>Element descriptor for an unsigned integer element</returns>
		protected static ElementDescriptor Uint(long id, string name = "")
		{
			return new ElementDescriptor(id, name, ElementType.UnsignedInteger);
		}

		/// <summary>
		/// Creates a signed integer element descriptor
		/// </summary>
		/// <param name="id">Element identifier</param>
		/// <param name="name">Element name</param>
		/// <returns>Element descriptor for a signed integer element</returns>
		protected static ElementDescriptor Int(long id, string name = "")
		{
			return new ElementDescriptor(id, name, ElementType.SignedInteger);
		}

		/// <summary>
		/// Creates an ASCII string element descriptor
		/// </summary>
		/// <param name="id">Element identifier</param>
		/// <param name="name">Element name</param>
		/// <returns>Element descriptor for an ASCII string element</returns>
		protected static ElementDescriptor Ascii(long id, string name = "")
		{
			return new ElementDescriptor(id, name, ElementType.AsciiString);
		}

		/// <summary>
		/// Creates a UTF-8 string element descriptor
		/// </summary>
		/// <param name="id">Element identifier</param>
		/// <param name="name">Element name</param>
		/// <returns>Element descriptor for a UTF-8 string element</returns>
		protected static ElementDescriptor Utf8(long id, string name = "")
		{
			return new ElementDescriptor(id, name, ElementType.Utf8String);
		}

		/// <summary>
		/// Creates a floating point element descriptor
		/// </summary>
		/// <param name="id">Element identifier</param>
		/// <param name="name">Element name</param>
		/// <returns>Element descriptor for a floating point element</returns>
		protected static ElementDescriptor Float(long id, string name = "")
		{
			return new ElementDescriptor(id, name, ElementType.Float);
		}

		/// <summary>
		/// Creates a date element descriptor
		/// </summary>
		/// <param name="id">Element identifier</param>
		/// <param name="name">Element name</param>
		/// <returns>Element descriptor for a date element</returns>
		protected static ElementDescriptor Date(long id, string name = "")
		{
			return new ElementDescriptor(id, name, ElementType.Date);
		}

		/// <summary>
		/// Specialized element descriptor for master elements (containers)
		/// </summary>
		public class MasterElementDescriptor : ElementDescriptor
		{
			/// <summary>
			/// Initializes a new instance of the MasterElementDescriptor class
			/// </summary>
			/// <param name="identifier">Element identifier</param>
			/// <param name="name">Element name</param>
			protected MasterElementDescriptor(long identifier, string name = "") : base(identifier, name, ElementType.MasterElement) { }
		}
	}

	/// <summary>
	/// Appendix C. EBML Standard definitions.
	/// </summary>
	public class StandardDtd : DTDBase
	{
		private StandardDtd() { }

		/// <summary>
		/// EBML header element descriptor
		/// </summary>
		public static readonly EBMLDesc
			EBML = new EBMLDesc(0x1a45dfa3);
		/// <summary>
		/// Signature slot element descriptor for digital signatures
		/// </summary>
		public static readonly SignatureSlotDesc
			SignatureSlot = new SignatureSlotDesc(0x1b538667);
		/// <summary>
		/// CRC-32 checksum element descriptor
		/// </summary>
		/// <summary>
		/// Void element descriptor for padding
		/// </summary>
		public static readonly ElementDescriptor
			CRC32 = Container(0xc3).Named(nameof(CRC32)),
			Void = Binary(0xec).Named(nameof(Void));

		/// <summary>
		/// EBML header element with standard sub-elements
		/// </summary>
		public sealed class EBMLDesc : MasterElementDescriptor
		{
			internal EBMLDesc(long id) : base(id, "EBML") { }

			/// <summary>EBML version number</summary>
			/// <summary>Minimum EBML version required to read this file</summary>
			/// <summary>Maximum length of element IDs in this file</summary>
			/// <summary>Maximum length of element sizes in this file</summary>
			/// <summary>Document type string identifier</summary>
			/// <summary>Document type version number</summary>
			/// <summary>Minimum document type version required to read this file</summary>
			public readonly ElementDescriptor
				EBMLVersion = Uint(0x4286, nameof(EBMLVersion)),
				EBMLReadVersion = Uint(0x42f7, nameof(EBMLReadVersion)),
				EBMLMaxIDLength = Uint(0x42f2, nameof(EBMLMaxIDLength)),
				EBMLMaxSizeLength = Uint(0x42f3, nameof(EBMLMaxSizeLength)),
				DocType = Ascii(0x4282, nameof(DocType)),
				DocTypeVersion = Uint(0x4287, nameof(DocTypeVersion)),
				DocTypeReadVersion = Uint(0x4285, nameof(DocTypeReadVersion));
		}

		/// <summary>
		/// Digital signature slot element with cryptographic sub-elements
		/// </summary>
		public sealed class SignatureSlotDesc : MasterElementDescriptor
		{
			internal SignatureSlotDesc(long id) : base(id, "SignatureSlot") { }

			/// <summary>Signature algorithm identifier</summary>
			/// <summary>Hash algorithm identifier</summary>
			/// <summary>Public key used for signature verification</summary>
			/// <summary>Digital signature data</summary>
			/// <summary>Container for signature-related elements</summary>
			/// <summary>List of signed elements</summary>
			/// <summary>Individual signed element reference</summary>
			public readonly ElementDescriptor
				SignatureAlgo = Uint(0x7e8a, nameof(SignatureAlgo)),
				SignatureHash = Uint(0x7e9a, nameof(SignatureHash)),
				SignaturePublicKey = Binary(0x7ea5, nameof(SignaturePublicKey)),
				Signature = Binary(0x7eb5, nameof(Signature)),
				SignatureElements = Container(0x7e5b, nameof(SignatureElements)),
				SignatureElementList = Container(0x7e7b, nameof(SignatureElementList)),
				SignedElement = Binary(0x6532, nameof(SignedElement));
		}
	}
}
