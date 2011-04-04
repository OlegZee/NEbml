using System;

namespace NEbml.Core
{
	/// <summary>
	/// Defines element type
	/// </summary>
	public class ElementTypeAttribute:Attribute
	{
		public ElementTypeAttribute(ElementType type)
		{
			ElementType = type;
		}

		public ElementType ElementType {get; private set;}
	}
}