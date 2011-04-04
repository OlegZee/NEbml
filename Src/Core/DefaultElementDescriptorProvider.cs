using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NEbml.Core
{
	/// <summary>
	/// The default implementation of the <code>ElementDescriptorProvider</code> interface.
	/// </summary>
	public class DefaultElementDescriptorProvider : IElementDescriptorProvider
	{
		private readonly IDictionary<ulong, ElementDescriptor> _descriptorsMap;

		/// <summary>
		/// Initializes a new instance of the <code>DefaultElementDescriptorProvider</code> class.
		/// </summary>
		/// <param name="descriptors">the array of element descriptors that will be accessible through this provider</param>
		public DefaultElementDescriptorProvider(IEnumerable<ElementDescriptor> descriptors)
		{
			if (descriptors == null) throw new ArgumentNullException("descriptors");

			if (descriptors.Any(d => d == null))
				throw new ArgumentException("Descriptors contain null", "descriptors");
			if (descriptors.Any(d => !d.Identifier.IsValidIdentifier))
				throw new ArgumentException("Descriptors contain elements with invalid identifier");

			_descriptorsMap = descriptors.ToDictionary(d => d.Identifier.EncodedValue, d => d);
		}

		public bool HasElementIdentifier(VInt identifier)
		{
			return _descriptorsMap.ContainsKey(identifier.EncodedValue);
		}

		public ElementDescriptor GetElementDescriptor(VInt identifier)
		{
			ElementDescriptor value;
			return _descriptorsMap.TryGetValue(identifier.EncodedValue, out value) ? value: null;
		}

		#region Implementation of IEnumerable

		public IEnumerator<ElementDescriptor> GetEnumerator()
		{
			return _descriptorsMap.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}