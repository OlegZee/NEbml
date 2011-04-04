using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEbml.Core
{
	/// <summary>
	/// Provides a composite <code>ElementDescriptorProvider</code> that aggregates a number of other <code>ElementDescriptorProvider</code>s.
	/// </summary>
	public class CompositeElementDescriptorProvider : IElementDescriptorProvider
	{
		private readonly IElementDescriptorProvider[] _providers;

		/// <summary>
		/// Creates a new <code>CompositeElementDescriptorProvider</code> object.
		/// </summary>
		/// <param name="providers">the array of providers to aggregate</param>
		public CompositeElementDescriptorProvider(params IElementDescriptorProvider[] providers)
		{
			if (providers == null) throw new ArgumentNullException("providers");

			if (providers.All(provider => provider == null))
			{
				throw new ArgumentException("All providers are empty");
			}

			_providers = providers.Where(d => d != null).ToArray();
		}

		#region ElementDescriptorProvider Members

		public bool SupportsElementIdentifier(VInt identifier)
		{
			return _providers.Any(provider => provider.SupportsElementIdentifier(identifier));
		}

		public ElementDescriptor GetElementDescriptor(VInt identifier)
		{
			return _providers
				.Select(provider => provider.GetElementDescriptor(identifier))
				.FirstOrDefault(descriptor => descriptor != null);
		}

		#endregion

		#region Implementation of IEnumerable

		public IEnumerator<ElementDescriptor> GetEnumerator()
		{
			return _providers.SelectMany(provider => provider).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}