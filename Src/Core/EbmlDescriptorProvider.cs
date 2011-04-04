namespace NEbml.Core
{
	public class EbmlDescriptorProvider : DefaultElementDescriptorProvider
	{
		public static readonly ElementDescriptor Void = new ElementDescriptor(0xec, "Void", ElementType.Binary);

		private static readonly ElementDescriptor[] ElementDescriptors = {
			new ElementDescriptor(0x1a45dfa3, "EBML", ElementType.MasterElement),
			new ElementDescriptor(0x4286, "EBMLVersion", ElementType.UnsignedInteger),
			new ElementDescriptor(0x42f7, "EBMLReadVersion", ElementType.UnsignedInteger),
			new ElementDescriptor(0x42f2, "EBMLMaxIDLength", ElementType.UnsignedInteger),
			new ElementDescriptor(0x42f3, "EBMLMaxSizeLength", ElementType.UnsignedInteger),
			new ElementDescriptor(0x4282, "DocType", ElementType.AsciiString),
			new ElementDescriptor(0x4287, "DocTypeVersion", ElementType.UnsignedInteger),
			new ElementDescriptor(0x4285, "DocTypeReadVersion", ElementType.UnsignedInteger),
			new ElementDescriptor(0xbf, "CRC-32", ElementType.Binary),
			Void,
			new ElementDescriptor(0x1b538667, "SignatureSlot", ElementType.MasterElement),
			new ElementDescriptor(0x7e8a, "SignatureAlgo", ElementType.UnsignedInteger),
			new ElementDescriptor(0x7e9a, "SignatureHash", ElementType.UnsignedInteger),
			new ElementDescriptor(0x7ea5, "SignaturePublicKey", ElementType.Binary),
			new ElementDescriptor(0x7eb5, "Signature", ElementType.Binary),
			new ElementDescriptor(0x7e5b, "SignatureElements", ElementType.MasterElement),
			new ElementDescriptor(0x7e7b, "SignatureElementList", ElementType.MasterElement),
			new ElementDescriptor(0x6532, "SignedElement", ElementType.Binary),
		};

		public EbmlDescriptorProvider()
			: base(ElementDescriptors)
		{
		}
	}
}