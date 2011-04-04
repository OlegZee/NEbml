using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NEbml.MkvTitleEdit.ViewModel;
using NEbml.MkvTitleEdit.Properties;

namespace NEbml.MkvTitleEdit
{
	/// <summary>
	/// MKV attributes editing form
	/// </summary>
	internal partial class EditMkvAttributesForm : Form
	{
		private EditMkvAttributesModel _model;

		public EditMkvAttributesForm()
		{
			InitializeComponent();
		}

		public void Bind(EditMkvAttributesModel model)
		{
			if (model == null) throw new ArgumentNullException("model");
			_model = model;

			breadCrumbsPath1.DataBindings.Add("Path", _model, "FolderPath", false, DataSourceUpdateMode.OnPropertyChanged);
			dataGridView1.DataBindings.Add("DataSource", _model, "Entries");
		}

		private bool NavigateToRow(DataGridViewRow dataRow)
		{
			if (dataRow == null)
				return false;

			var dataItem = (ListEntryViewModel)dataRow.DataBoundItem;
			if (dataItem.IsNavigable)
			{
				if(SavePendingChanges(true))
					_model.FolderPath = dataItem.FullName;
			}

			return true;
		}

		private bool SavePendingChanges(bool withDialog)
		{
			if(!_model.HasPendingChanges) return true;

			var result =
				withDialog
					? MessageBox.Show(this, "Save all pending updates?", "Confirm update files", MessageBoxButtons.YesNoCancel)
					: DialogResult.Yes;

			switch (result)
			{
				case DialogResult.Yes:
					var errors = _model.ApplyPendingChanges();

					if (errors.Any())
						DisplayErrors(errors);

					return !errors.Any();
				case DialogResult.No:
					return true;
			}
			return false;
		}

		private void DisplayErrors(IEnumerable<Exception> errors)
		{
			if(!errors.Any()) return;

			var message = string.Join(Environment.NewLine,
				(from e in errors select e.Message).Union(new[]
				{
					new string('=', 30),
					string.Format("{0} error(s) detected", errors.Count())
				}).ToArray());

			MessageBox.Show(this, message, "Errors while applying changes", MessageBoxButtons.OK, MessageBoxIcon.Error);

			// TODO implement retry
		}

		#region Events and handlers

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!SavePendingChanges(true))
				e.Cancel = true;

			_model.SaveSettings();
		}

		private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter && dataGridView1.CurrentRow != null)
			{
				e.Handled = NavigateToRow(dataGridView1.CurrentRow);
			}
		}

		private void dataGridView1_DoubleClick(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow != null)
			{
				NavigateToRow(dataGridView1.CurrentRow);
			}
		}

		private void buttonApplyChanges_Click(object sender, EventArgs e)
		{
			if(SavePendingChanges(false))
				_model.RefreshList();
		}

		private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex < 0) return;
			var dataRow = dataGridView1.Rows[e.RowIndex];
			if (e.ColumnIndex == -1 && ((ListEntryViewModel) dataRow.DataBoundItem).IsDirty)
			{
				var imgSize = Resources.Pen.Size;

				e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
				var cs = e.CellBounds.Size;
				var center = e.CellBounds.Location + new Size(cs.Width/2, cs.Height/2);
				var rect = Rectangle.Inflate(new Rectangle(center, Size.Empty), imgSize.Width/2, imgSize.Height/2);
				e.Graphics.DrawImageUnscaled(Resources.Pen, rect);
				e.Handled = true;
			}
		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			var form = new EditMkvAttributesForm();

			var model = new EditMkvAttributesModel();
			model.ReadSettings();

			var argv = Environment.GetCommandLineArgs();
			if (argv.Length > 1)
			{
				if (Directory.Exists(argv[1]))
				{
					model.FolderPath = argv[1];
				}
				else
				{
					MessageBox.Show("Folder '{0}' not found. Opening default one.", "Arguments error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			form.Bind(model);

			Application.Run(form);
		}

	}
}
