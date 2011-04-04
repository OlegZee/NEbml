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
		private readonly ElementDescriptor[] _descriptors;
		private readonly IDictionary<ulong, ElementDescriptor> _descriptorsMap;

		/// <summary>
		/// Initializes a new instance of the <code>DefaultElementDescriptorProvider</code> class.
		/// </summary>
		/// <param name="descriptors">the array of element descriptors that will be accessible through this provider</param>
		public DefaultElementDescriptorProvider(ElementDescriptor[] descriptors)
		{
			if (descriptors == null) throw new ArgumentNullException("descriptors");

			if (descriptors.Any(d => d == null))
				throw new ArgumentException("descriptors contains null");

			_descriptors = (ElementDescriptor[])descriptors.Clone();
			_descriptorsMap = new Dictionary<ulong, ElementDescriptor>();
			foreach (var descriptor in descriptors)
			{
				if (!descriptor.Identifier.IsValidIdentifier)
				{
					throw new ArgumentException("descriptors contains elements with invalid identifier");
				}

				if (_descriptorsMap.ContainsKey(descriptor.Identifier.EncodedValue))
				{
					throw new ArgumentException("descriptors contains elements with the same identifier");
				}
				_descriptorsMap.Add(descriptor.Identifier.EncodedValue, descriptor);
			}
		}


		public bool SupportsElementIdentifier(VInt identifier)
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
			return _descriptors.Cast<ElementDescriptor>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}