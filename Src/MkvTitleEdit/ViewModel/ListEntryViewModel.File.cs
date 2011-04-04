using System;
using System.IO;
using NEbml.MkvTitleEdit.Matroska;

namespace NEbml.MkvTitleEdit.ViewModel
{
	partial class ListEntryViewModel
	{
		private class File : ListEntryViewModel
		{
			private string _title;

			public File(FileSystemInfo file, string title)
			{
				if (file == null) throw new ArgumentNullException("file");

				_title = title ?? string.Empty;
				Name = file.Name;
				FullName = file.FullName;
			}

			public override string Title
			{
				get { return _title; }
				set
				{
					if (_title == value) return;
					_title = value;
					IsDirty = true;
				}
			}
		}

		/// <summary>
		/// Creates new file entry
		/// </summary>
		/// <param name="file"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		public static ListEntryViewModel CreateFile(FileSystemInfo file, string title)
		{
			return new File(file, title);
		}

		/// <summary>
		/// Creates new entry for MKV file (reads attributes from file)
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static ListEntryViewModel CreateMkvFile(FileInfo file)
		{
			using (var upd = new SegmentInfoUpdater())
			{
				upd.OpenRead(file);

				var ts = upd.Duration;

				return new File(file, upd.Title)
				{
					Duration = string.Format("{0:00}:{1:00}:{2:00}", (int)ts.TotalHours, ts.Minutes, ts.Seconds)
				};
			}
		}

	}
}