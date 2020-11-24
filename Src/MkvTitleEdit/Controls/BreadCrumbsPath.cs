/* Copyright (c) 2011-2020 Oleg Zee

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NEbml.MkvTitleEdit.Controls
{
	/// <summary>
	/// Path navigation control.
	/// </summary>
	public class BreadCrumbsPath : FlowLayoutPanel
	{
		private readonly List<Pair<Control,Control>> _labels;
		private string _path = string.Empty;
		private readonly Font _webdings;
		private readonly Control _chooseFolder;

		private readonly List<Action> _unsubscribe;

		/// <summary>
		/// Initializes a new instance of the BreadCrumbsPath control
		/// </summary>
		public BreadCrumbsPath()
		{
			_labels = new List<Pair<Control, Control>>();
			_unsubscribe = new List<Action>();
			_webdings = new Font("Webdings", 9);

			_chooseFolder = new LinkLabel {Text = "...", AutoSize = true};
			((LinkLabel)_chooseFolder).LinkClicked += ChooseFolderOnClick;
			Controls.Add(_chooseFolder);
		}

		protected override void Dispose(bool disposing)
		{
			_unsubscribe.All(action => {action(); return true;});
			_unsubscribe.Clear();

			base.Dispose(disposing);
		}

		/// <summary>
		/// Gets or sets whether to add "Choose folder" button
		/// </summary>
		[DefaultValue(false)]
		public bool DisplayChooseFolder { get; set; }

		/// <summary>
		/// Gets or sets currently selected path
		/// </summary>
		[Bindable(true)]
		[DefaultValue("")]
		public string Path
		{
			get { return _path; }
			set
			{
				if(_path == value) return;

				_path = value;
				// no path validation since it might be virtual path

				SuspendLayout();
				var chunks = SplitPath(value);
				if (chunks.Count > _labels.Count)
				{
					var newLabels = Enumerable.Repeat(1, chunks.Count - _labels.Count).Select(_ =>
						new Pair<Control, Control>
							{
								Item1 = new LinkLabel
									{
										AutoSize = true, LinkColor = Color.FromKnownColor(KnownColor.WindowText),
										Margin = new Padding(0, 1, 0, 1), LinkBehavior =  LinkBehavior.HoverUnderline
									},
								Item2 = new Label { AutoSize = true, Font = _webdings, Text = "4", Margin = Padding.Empty}
							}).ToArray();

					_unsubscribe.AddRange(
						newLabels.Select(p => p.Item1).Cast<LinkLabel>().Select(label =>
							{
								label.LinkClicked += PathChunkClick;
								return new Action(() => label.LinkClicked -= PathChunkClick);
							}));

					Controls.AddRange(
						newLabels.SelectMany(pair => new[] {pair.Item2, pair.Item1}).ToArray()
						);
					_labels.AddRange(newLabels);
				}

				for(var i = 0; i < _labels.Count; i++)
				{
					var label = _labels[i].Item1;

					label.Visible = i < chunks.Count;
					_labels[i].Item2.Visible = label.Visible && i > 0;

					if (label.Visible)
					{
						label.Text = chunks[i].Item1;
						label.Tag = chunks[i].Item2;
					}
				}

				_chooseFolder.SendToBack();
				_chooseFolder.Visible = DisplayChooseFolder;

				ResumeLayout(true);

				if (PathChanged != null)
					PathChanged(this, new EventArgs());
			}
		}

		public event System.EventHandler PathChanged;

		#region implementation

		void PathChunkClick(object sender, EventArgs e)
		{
			var tag = ((Control)sender).Tag as DirectoryInfo;
			Path = tag == null ? string.Empty : tag.FullName;
		}

		private void ChooseFolderOnClick(object sender, EventArgs eventArgs)
		{
			var folderBrowserDialog = new FolderBrowserDialog {ShowNewFolderButton = false, SelectedPath = Path};

			if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
			{
				Path = folderBrowserDialog.SelectedPath;
			}
		}

		private struct Pair<T1,T2>
		{
			public T1 Item1;
			public T2 Item2;
		}

		private static IList<Pair<string,DirectoryInfo>> SplitPath(string value)
		{
			var list = new List<Pair<string, DirectoryInfo>> { new Pair<string, DirectoryInfo> { Item1 = "Root", Item2 = null } };

			if (!String.IsNullOrEmpty(value))
			{
				var chunks = new List<Pair<string, DirectoryInfo>>();

				for (var path = new DirectoryInfo(value); path != null; path = path.Parent)
				{
					chunks.Add(new Pair<string, DirectoryInfo> { Item1 = path.Name, Item2 = path });
				}

				list.AddRange(Enumerable.Reverse(chunks));
			}

			return list.AsReadOnly();
		}

		#endregion
	}
}
