using NEbml.Core;

namespace NEbml.MkvTitleEdit.Matroska
{
	internal static class ReaderExtensions
	{
		public static bool LocateElement(this EbmlReader reader, ElementDescriptor descriptor)
		{
			while (reader.ReadNext())
			{
				var identifier = reader.ElementId;

				if (identifier == descriptor.Identifier)
				{
					return true;
				}
			}
			return false;
		}
	}
}