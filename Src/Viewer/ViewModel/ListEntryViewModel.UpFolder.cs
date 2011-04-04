using System.IO;

namespace NEbml.MkvTitleEdit.ViewModel
{
	partial class ListEntryViewModel
	{
		private class UpFolder : Folder
		{
			public UpFolder(DirectoryInfo dir)
				: base(dir)
			{
				Name = "..";
			}
		}

		/// <summary>
		/// Creates new "up folder" entry
		/// </summary>
		/// <returns></returns>
		public static ListEntryViewModel CreateParentFolderLink(DirectoryInfo dir)
		{
			return new UpFolder(dir);
		}
	}
}