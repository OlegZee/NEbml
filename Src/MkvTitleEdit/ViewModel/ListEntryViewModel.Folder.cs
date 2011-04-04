using System.IO;
using NEbml.MkvTitleEdit.Properties;

namespace NEbml.MkvTitleEdit.ViewModel
{
	partial class ListEntryViewModel
	{
		private class Folder : ListEntryViewModel
		{
			public Folder(FileSystemInfo dir)
			{
				IsNavigable = true;
				Name = dir.Name;
				FullName = dir.FullName;
			}

			public override string Title
			{
				get { return string.Empty; }
				set { }
			}

			public override System.Drawing.Image EntryTypeImage
			{
				get {return Resources.Folder;}
			}
		}

		private class DriveFolder:Folder
		{
			public DriveFolder(FileSystemInfo dir) : base(dir)
			{
			}

			public override System.Drawing.Image EntryTypeImage
			{
				get {return Resources.Drive;}
			}
		}

		/// <summary>
		/// Creates new folder entry
		/// </summary>
		/// <returns></returns>
		public static ListEntryViewModel CreateFolder(DirectoryInfo dir)
		{
			return new Folder(dir);
		}

		public static ListEntryViewModel CreateDrive(DirectoryInfo dir)
		{
			return new DriveFolder(dir);
		}

	}
}