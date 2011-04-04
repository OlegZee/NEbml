using System;
using System.Drawing;
using NEbml.MkvTitleEdit.Properties;

namespace NEbml.MkvTitleEdit.ViewModel
{
	abstract partial class ListEntryViewModel
	{
		protected ListEntryViewModel()
		{
			Duration = string.Empty;
			Name = FullName = string.Empty;
		}

		public string Name { get; protected set; }
		public string Duration { get; protected set; }
		public string FullName { get; protected set; }
		public bool IsDirty { get; protected set; }

		public abstract string Title { get; set; }

		public virtual Image EntryTypeImage
		{
			get { return Resources.MkvIcon; }
		}

		public bool IsNavigable
		{get; protected set; }
	}
}
