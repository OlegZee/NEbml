using System;
using System.Text;

namespace NEbml.Core
{
	/// <summary>
	/// Defined the EBML element description.
	/// </summary>
	public class ElementDescriptor
	{
		/// <summary>
		/// Initializes a new instance of the <code>ElementDescriptor</code> class.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		public ElementDescriptor(long identifier, string name, ElementType type)
			: this(VInt.FromEncoded((ulong)identifier), name, type)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <code>ElementDescriptor</code> class.
		/// </summary>
		/// <param name="identifier">the element identifier</param>
		/// <param name="name">the element name or <code>null</code> if the name is not known</param>
		/// <param name="type">the element type or <code>null</code> if the type is not known</param>
		/// <exception cref="ArgumentNullException">if <code>identifier</code> is <code>null</code></exception>
		private ElementDescriptor(VInt identifier, string name, ElementType type)
		{
			if (!identifier.IsValidIdentifier)
				throw new ArgumentException("Value is not valid identifier", nameof(identifier));

			Identifier = identifier;
			Name = name;
			Type = type;
		}

		/// <summary>
		/// Returns the element identifier.
		/// </summary>
		/// <value>the element identifier in the encoded form</value>
		public VInt Identifier { get; }

		/// <summary>
		/// Returns the element name.
		/// </summary>
		/// <value>the element name or &lt;code&gt;null&lt;/code&gt; if the name is not known</value>
		public string Name { get; }

		/// <summary>
		/// Returns the element type.
		/// </summary>
		/// <value>the element type or &lt;code&gt;null&lt;/code&gt; if the type is not known</value>
		public ElementType Type { get; }

		public override int GetHashCode()
		{
			int result = 17;
			result = 37 * result + Identifier.GetHashCode();
			result = 37 * result + (Name == null ? 0 : Name.GetHashCode());
			result = 37 * result + (Type == ElementType.None ? 0 : Type.GetHashCode());
			return result;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj is ElementDescriptor o2)
			{
				return Equals(Identifier, o2.Identifier)
					&& Equals(Name, o2.Name)
						&& Equals(Type, o2.Type);
			}
			return false;
		}

		public override string ToString()
		{
			var buffer = new StringBuilder();
			buffer.Append("ElementDescriptor(");
			buffer.Append("identifier=").Append(Identifier);
			if (Name != null)
			{
				buffer.Append(',').Append("name=").Append(Name);
			}
			if (Type != ElementType.None)
			{
				buffer.Append(',').Append("type=").Append(Type);
			}
			buffer.Append(')');
			return buffer.ToString();
		}
	}
}