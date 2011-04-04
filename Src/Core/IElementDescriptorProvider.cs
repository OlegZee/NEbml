using System.Collections.Generic;

namespace NEbml.Core
{
	/// <summary>
	/// The service provider interface for a class that can convert element identifiers to element descriptors.
	/// </summary>
	public interface IElementDescriptorProvider : IEnumerable<ElementDescriptor>
	{
		/// <summary>
		/// Returns <code>true</code> if this provider supports an element with the specified identifier.
		/// </summary>
		/// <param name="identifier">the element identifier</param>
		/// <returns><code>true</code> if the specified identifier is supported; <code>false</code> otherwise</returns>
		bool HasElementIdentifier(VInt identifier);

		/// <summary>
		/// Returns the descriptor of an element with the specified identifier.
		/// </summary>
		/// <param name="identifier">the element identifier</param>
		/// <returns>the element descriptor or <code>null</code> if the specified identifier is not supported</returns>
		ElementDescriptor GetElementDescriptor(VInt identifier);
	}
}