using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NEbml.MkvTitleEdit.Matroska;
using NEbml.MkvTitleEdit.Properties;

namespace NEbml.MkvTitleEdit.ViewModel
{
	/// <summary>
	/// MKV Attributes form model
	/// </summary>
	internal class EditMkvAttributesModel: INotifyPropertyChanged
	{
		private DirectoryInfo _currentDir;
		private IList<ListEntryViewModel> _entries;

		public EditMkvAttributesModel()
		{
			_entries = new List<ListEntryViewModel>();
		}

		#region API/Properties

		/// <summary>
		/// Instructs to update read-only files. Otherwise will treat as error
		/// </summary>
		public bool UpdateReadonlyFiles { get; set; }

		public object SelItem { get; set; }

		/// <summary>
		/// Gets or sets currently displayed folder
		/// </summary>
		public string FolderPath
		{
			get { return _currentDir == null ? string.Empty : _currentDir.FullName; }
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					_currentDir = null;
				}
				else
				{
					var dir = new DirectoryInfo(value);
					if(!dir.Exists)
						throw new ArgumentException(string.Format("Folder {0} not found", value), "value");

					_currentDir = dir;
				}

				RaisePropertyChanged(() => FolderPath);

				RefreshList();
			}
		}

		/// <summary>
		/// Gets true if any changes are made
		/// </summary>
		public bool HasPendingChanges
		{
			get { return _entries != null && _entries.Any(row => row.IsDirty); }
		}

		/// <summary>
		/// Gets true if any changes are made
		/// </summary>
		public IList<ListEntryViewModel> Entries
		{
			get { return _entries ?? (_entries = GetFolderItems(_currentDir)); }
		}

		/// <summary>
		/// Reloads item list
		/// </summary>
		public void RefreshList()
		{
			_entries = null;
			RaisePropertyChanged(() => Entries);
		}

		/// <summary>
		/// Applies changes to a files
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Exception> ApplyPendingChanges()
		{
			// don't want to introduce another class hierarchy so simply using exception
			var errors = new List<Exception>();

			foreach (var entry in _entries.Where(model => model.IsDirty))
			{
				var file = new FileInfo(entry.FullName);

				// check various error conditions
				if (!file.Exists)
				{
					errors.Add(new FileNotFoundException("", entry.FullName));
					continue;
				}

				var isReadOnly = file.IsReadOnly;

				try
				{

					if (isReadOnly)
					{
						if (UpdateReadonlyFiles)
						{
							file.IsReadOnly = false;
						}
						else
						{
							errors.Add(new Exception(String.Format("File is readonly: {0}", entry.FullName)));
							continue;
						}
					}

					using (var upd = new SegmentInfoUpdater())
					{
						upd.Open(file);
						upd.Title = entry.Title;
						upd.Write();
					}

					if (isReadOnly)
					{
						file.IsReadOnly = true;
					}
				}
				catch (Exception e)
				{
					errors.Add(e);
				}
			}

			return errors;

		}

		public void ReadSettings()
		{
			FolderPath = Settings.Default.LastOpenedFolder;
		}

		public void SaveSettings()
		{
			Settings.Default.LastOpenedFolder = FolderPath;
			Settings.Default.Save();
		}

		#endregion

		private static IList<ListEntryViewModel> GetFolderItems(DirectoryInfo dir)
		{
			if (dir == null)
			{
				return
					DriveInfo.GetDrives().Where(info => info.RootDirectory.Exists)
						.Select(info => ListEntryViewModel.CreateDrive(info.RootDirectory))
						.ToList();
			}
			
			var directories = dir.GetDirectories();
			var files = dir.GetFiles("*.mkv");

			var parent = dir.Parent != null ?
				new[] {ListEntryViewModel.CreateParentFolderLink(dir.Parent)} :
				new ListEntryViewModel[0];

			var result = parent
				.Union(
					directories.Select(d => ListEntryViewModel.CreateFolder(d))
				)
				.Union(
					files.Select(file => ListEntryViewModel.CreateMkvFile(file))
				);
			return result.ToList();
		}

		private void RaisePropertyChanged<T>(Expression<Func<T>> propertyGet)
		{
			var memberAssignment = (MemberExpression)propertyGet.Body;
			var propertyName = memberAssignment.Member.Name;

			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
