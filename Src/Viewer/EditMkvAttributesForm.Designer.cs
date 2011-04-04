using NEbml.MkvTitleEdit.Controls;

namespace NEbml.MkvTitleEdit
{
	partial class EditMkvAttributesForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditMkvAttributesForm));
			this.buttonApplyChanges = new System.Windows.Forms.Button();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.colEntryTypeImage = new System.Windows.Forms.DataGridViewImageColumn();
			this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDuration = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDirty = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colEntryType = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colFullPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colIsNavigable = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.breadCrumbsPath1 = new BreadCrumbsPath();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonApplyChanges
			// 
			this.buttonApplyChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonApplyChanges.Location = new System.Drawing.Point(593, 7);
			this.buttonApplyChanges.Name = "buttonApplyChanges";
			this.buttonApplyChanges.Size = new System.Drawing.Size(114, 33);
			this.buttonApplyChanges.TabIndex = 0;
			this.buttonApplyChanges.Text = "Apply changes";
			this.buttonApplyChanges.UseVisualStyleBackColor = true;
			this.buttonApplyChanges.Click += new System.EventHandler(this.buttonApplyChanges_Click);
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colEntryTypeImage,
            this.colName,
            this.colDuration,
            this.colTitle,
            this.colDirty,
            this.colEntryType,
            this.colFullPath,
            this.colIsNavigable});
			this.dataGridView1.GridColor = System.Drawing.SystemColors.ControlLight;
			this.dataGridView1.Location = new System.Drawing.Point(12, 53);
			this.dataGridView1.MultiSelect = false;
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.RowHeadersWidth = 28;
			this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.dataGridView1.RowTemplate.Height = 30;
			this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView1.Size = new System.Drawing.Size(695, 481);
			this.dataGridView1.TabIndex = 2;
			this.dataGridView1.DoubleClick += new System.EventHandler(this.dataGridView1_DoubleClick);
			this.dataGridView1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView1_CellPainting);
			this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyDown);
			// 
			// colEntryTypeImage
			// 
			this.colEntryTypeImage.DataPropertyName = "EntryTypeImage";
			this.colEntryTypeImage.HeaderText = "Type";
			this.colEntryTypeImage.Name = "colEntryTypeImage";
			this.colEntryTypeImage.ReadOnly = true;
			this.colEntryTypeImage.Width = 35;
			// 
			// colName
			// 
			this.colName.DataPropertyName = "Name";
			this.colName.HeaderText = "File name";
			this.colName.Name = "colName";
			this.colName.ReadOnly = true;
			this.colName.Width = 150;
			// 
			// colDuration
			// 
			this.colDuration.DataPropertyName = "Duration";
			this.colDuration.HeaderText = "Duration";
			this.colDuration.Name = "colDuration";
			this.colDuration.ReadOnly = true;
			this.colDuration.Width = 60;
			// 
			// colTitle
			// 
			this.colTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colTitle.DataPropertyName = "Title";
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.colTitle.DefaultCellStyle = dataGridViewCellStyle1;
			this.colTitle.HeaderText = "Title";
			this.colTitle.Name = "colTitle";
			// 
			// colDirty
			// 
			this.colDirty.DataPropertyName = "IsDirty";
			this.colDirty.HeaderText = ".isdirty";
			this.colDirty.Name = "colDirty";
			this.colDirty.ReadOnly = true;
			this.colDirty.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.colDirty.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colDirty.Visible = false;
			this.colDirty.Width = 40;
			// 
			// colEntryType
			// 
			this.colEntryType.DataPropertyName = "EntryType";
			this.colEntryType.HeaderText = ".entry type";
			this.colEntryType.Name = "colEntryType";
			this.colEntryType.Visible = false;
			// 
			// colFullPath
			// 
			this.colFullPath.DataPropertyName = "FullName";
			this.colFullPath.HeaderText = ".full path";
			this.colFullPath.Name = "colFullPath";
			this.colFullPath.Visible = false;
			// 
			// colIsNavigable
			// 
			this.colIsNavigable.DataPropertyName = "IsNavigable";
			this.colIsNavigable.HeaderText = ".is_navigable";
			this.colIsNavigable.Name = "colIsNavigable";
			this.colIsNavigable.Visible = false;
			// 
			// breadCrumbsPath1
			// 
			this.breadCrumbsPath1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.breadCrumbsPath1.DisplayChooseFolder = true;
			this.breadCrumbsPath1.Location = new System.Drawing.Point(13, 12);
			this.breadCrumbsPath1.Name = "breadCrumbsPath1";
			this.breadCrumbsPath1.Size = new System.Drawing.Size(574, 41);
			this.breadCrumbsPath1.TabIndex = 3;
			// 
			// EditMkvAttributesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(719, 546);
			this.Controls.Add(this.breadCrumbsPath1);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.buttonApplyChanges);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "EditMkvAttributesForm";
			this.Text = "MKV Attributes Editor";
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonApplyChanges;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.DataGridViewImageColumn colEntryTypeImage;
		private System.Windows.Forms.DataGridViewTextBoxColumn colName;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDuration;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTitle;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDirty;
		private System.Windows.Forms.DataGridViewTextBoxColumn colEntryType;
		private System.Windows.Forms.DataGridViewTextBoxColumn colFullPath;
		private System.Windows.Forms.DataGridViewTextBoxColumn colIsNavigable;
		private BreadCrumbsPath breadCrumbsPath1;
	}
}