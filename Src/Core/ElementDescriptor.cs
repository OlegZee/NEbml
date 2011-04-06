using System;
using System.Text;

namespace NEbml.Core
{
	/// <summary>
	/// Defined the EBML element description.
	/// </summary>
	public class ElementDescriptor
	{
		private readonly VInt _identifier;
		private readonly String _name;
		private readonly ElementType _type;

		/// <summary>
		/// Initializes a new instance of the <code>ElementDescriptor</code> class.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		public ElementDescriptor(long identifier, String name, ElementType type)
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
		public ElementDescriptor(VInt identifier, String name, ElementType type)
		{
			if (!identifier.IsValidIdentifier)
				throw new ArgumentException("Value is not valid identifier", "identifier");

			_identifier = identifier;
			_name = name;
			_type = type;
		}

		/// <summary>
		/// Returns the element identifier.
		/// </summary>
		/// <value>the element identifier in the encoded form</value>
		public VInt Identifier
		{
			get { return _identifier; }
		}

		/// <summary>
		/// Returns the element name.
		/// </summary>
		/// <value>the element name or &lt;code&gt;null&lt;/code&gt; if the name is not known</value>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Returns the element type.
		/// </summary>
		/// <value>the element type or &lt;code&gt;null&lt;/code&gt; if the type is not known</value>
		public ElementType Type
		{
			get { return _type; }
		}

		public override int GetHashCode()
		{
			int result = 17;
			result = 37*result + _identifier.GetHashCode();
			result = 37*result + (_name == null ? 0 : _name.GetHashCode());
			result = 37*result + (_type == ElementType.None ? 0 : _type.GetHashCode());
			return result;
		}

		public override bool Equals(Object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj is ElementDescriptor)
			{
				var o2 = (ElementDescriptor) obj;
				return Equals(_identifier, o2._identifier)
					&& Equals(_name, o2._name)
						&& Equals(_type, o2._type);
			}
			return false;
		}

		public override String ToString()
		{
			var buffer = new StringBuilder();
			buffer.Append("ElementDescriptor(");
			buffer.Append("identifier=").Append(_identifier);
			if (_name != null)
			{
				buffer.Append(',').Append("name=").Append(_name);
			}
			if (_type != ElementType.None)
			{
				buffer.Append(',').Append("type=").Append(_type);
			}
			buffer.Append(')');
			return buffer.ToString();
		}
	}
}