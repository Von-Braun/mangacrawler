namespace MangaCrawler
{
    partial class MangaCrawlerForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.seriesFilterTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.panel8 = new System.Windows.Forms.Panel();
            this.seriesURLButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.seriesListBox = new MangaCrawler.ListBoxEx();
            this.panel3 = new System.Windows.Forms.Panel();
            this.chapterURLButton = new System.Windows.Forms.Button();
            this.downloadButton = new System.Windows.Forms.Button();
            this.chaptersListBox = new MangaCrawler.ListBoxEx();
            this.label7 = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cbzCheckBox = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.serverURLButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.serversListBox = new MangaCrawler.ListBoxEx();
            this.directoryChooseButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.directoryPathTextBox = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tasksGridView = new System.Windows.Forms.DataGridView();
            this.Delete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Chapter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Progress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tasksGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(216, 53);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.panel2);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.panel3);
            this.splitContainer.Size = new System.Drawing.Size(713, 424);
            this.splitContainer.SplitterDistance = 362;
            this.splitContainer.SplitterWidth = 8;
            this.splitContainer.TabIndex = 37;
            this.splitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer_SplitterMoved);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel4);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.seriesListBox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(3);
            this.panel2.Size = new System.Drawing.Size(362, 424);
            this.panel2.TabIndex = 2;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 82F));
            this.tableLayoutPanel4.Controls.Add(this.panel7, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.panel8, 1, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 391);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(359, 28);
            this.tableLayoutPanel4.TabIndex = 35;
            // 
            // panel7
            // 
            this.panel7.AutoSize = true;
            this.panel7.Controls.Add(this.seriesFilterTextBox);
            this.panel7.Controls.Add(this.label6);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel7.Location = new System.Drawing.Point(0, 0);
            this.panel7.Margin = new System.Windows.Forms.Padding(0);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(277, 28);
            this.panel7.TabIndex = 0;
            // 
            // seriesFilterTextBox
            // 
            this.seriesFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.seriesFilterTextBox.Location = new System.Drawing.Point(27, 7);
            this.seriesFilterTextBox.Name = "seriesFilterTextBox";
            this.seriesFilterTextBox.Size = new System.Drawing.Size(250, 20);
            this.seriesFilterTextBox.TabIndex = 35;
            this.seriesFilterTextBox.TextChanged += new System.EventHandler(this.seriesFilterTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(0, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 34;
            this.label6.Text = "Filter:";
            // 
            // panel8
            // 
            this.panel8.AutoSize = true;
            this.panel8.Controls.Add(this.seriesURLButton);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel8.Location = new System.Drawing.Point(277, 0);
            this.panel8.Margin = new System.Windows.Forms.Padding(0);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(82, 28);
            this.panel8.TabIndex = 1;
            // 
            // seriesURLButton
            // 
            this.seriesURLButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.seriesURLButton.Location = new System.Drawing.Point(3, 5);
            this.seriesURLButton.Name = "seriesURLButton";
            this.seriesURLButton.Size = new System.Drawing.Size(79, 23);
            this.seriesURLButton.TabIndex = 35;
            this.seriesURLButton.Text = "Visit page";
            this.seriesURLButton.UseVisualStyleBackColor = true;
            this.seriesURLButton.Click += new System.EventHandler(this.seriesURLButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-3, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 31;
            this.label4.Text = "Series:";
            // 
            // seriesListBox
            // 
            this.seriesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.seriesListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.seriesListBox.FormattingEnabled = true;
            this.seriesListBox.HorizontalScrollbar = true;
            this.seriesListBox.ItemHeight = 15;
            this.seriesListBox.Location = new System.Drawing.Point(0, 19);
            this.seriesListBox.Name = "seriesListBox";
            this.seriesListBox.Size = new System.Drawing.Size(359, 368);
            this.seriesListBox.TabIndex = 30;
            this.seriesListBox.HorizontalScroll += new MangaCrawler.ListBoxEx.ListBoxScrollDelegate(this.seriesListBox_HorizontalScroll);
            this.seriesListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.seriesListBox_DrawItem);
            this.seriesListBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.seriesListBox_MeasureItem);
            this.seriesListBox.SelectedIndexChanged += new System.EventHandler(this.seriesListBox_SelectedIndexChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.chapterURLButton);
            this.panel3.Controls.Add(this.downloadButton);
            this.panel3.Controls.Add(this.chaptersListBox);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(3);
            this.panel3.Size = new System.Drawing.Size(343, 424);
            this.panel3.TabIndex = 3;
            // 
            // chapterURLButton
            // 
            this.chapterURLButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chapterURLButton.Location = new System.Drawing.Point(81, 396);
            this.chapterURLButton.Name = "chapterURLButton";
            this.chapterURLButton.Size = new System.Drawing.Size(75, 23);
            this.chapterURLButton.TabIndex = 36;
            this.chapterURLButton.Text = "Visit page";
            this.chapterURLButton.UseVisualStyleBackColor = true;
            this.chapterURLButton.Click += new System.EventHandler(this.chapterURLButton_Click);
            // 
            // downloadButton
            // 
            this.downloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.downloadButton.Location = new System.Drawing.Point(0, 396);
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.Size = new System.Drawing.Size(75, 23);
            this.downloadButton.TabIndex = 35;
            this.downloadButton.Text = "Download";
            this.downloadButton.UseVisualStyleBackColor = true;
            this.downloadButton.Click += new System.EventHandler(this.downloadButton_Click);
            // 
            // chaptersListBox
            // 
            this.chaptersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chaptersListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.chaptersListBox.FormattingEnabled = true;
            this.chaptersListBox.HorizontalScrollbar = true;
            this.chaptersListBox.ItemHeight = 15;
            this.chaptersListBox.Location = new System.Drawing.Point(0, 19);
            this.chaptersListBox.Name = "chaptersListBox";
            this.chaptersListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.chaptersListBox.Size = new System.Drawing.Size(341, 368);
            this.chaptersListBox.TabIndex = 34;
            this.chaptersListBox.HorizontalScroll += new MangaCrawler.ListBoxEx.ListBoxScrollDelegate(this.chaptersListBox_HorizontalScroll);
            this.chaptersListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.chaptersListBox_DrawItem);
            this.chaptersListBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.chaptersListBox_MeasureItem);
            this.chaptersListBox.SelectedIndexChanged += new System.EventHandler(this.chaptersListBox_SelectedIndexChanged);
            this.chaptersListBox.DoubleClick += new System.EventHandler(this.chaptersListBox_DoubleClick);
            this.chaptersListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.chaptersListBox_KeyDown);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(-3, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(52, 13);
            this.label7.TabIndex = 33;
            this.label7.Text = "Chapters:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(940, 506);
            this.tabControl1.TabIndex = 24;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cbzCheckBox);
            this.tabPage1.Controls.Add(this.splitContainer);
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Controls.Add(this.directoryChooseButton);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.directoryPathTextBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(932, 480);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Series";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // cbzCheckBox
            // 
            this.cbzCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbzCheckBox.AutoSize = true;
            this.cbzCheckBox.Location = new System.Drawing.Point(789, 7);
            this.cbzCheckBox.Name = "cbzCheckBox";
            this.cbzCheckBox.Size = new System.Drawing.Size(112, 17);
            this.cbzCheckBox.TabIndex = 38;
            this.cbzCheckBox.Text = "Zip chapter to cbz";
            this.cbzCheckBox.UseVisualStyleBackColor = true;
            this.cbzCheckBox.CheckedChanged += new System.EventHandler(this.cbzCheckBox_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Controls.Add(this.serverURLButton);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.serversListBox);
            this.panel1.Location = new System.Drawing.Point(6, 53);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(201, 425);
            this.panel1.TabIndex = 35;
            // 
            // serverURLButton
            // 
            this.serverURLButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.serverURLButton.Location = new System.Drawing.Point(0, 396);
            this.serverURLButton.Name = "serverURLButton";
            this.serverURLButton.Size = new System.Drawing.Size(120, 23);
            this.serverURLButton.TabIndex = 30;
            this.serverURLButton.Text = "Visit page";
            this.serverURLButton.UseVisualStyleBackColor = true;
            this.serverURLButton.Click += new System.EventHandler(this.serverURLButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(-3, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 29;
            this.label5.Text = "Server:";
            // 
            // serversListBox
            // 
            this.serversListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.serversListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.serversListBox.FormattingEnabled = true;
            this.serversListBox.HorizontalScrollbar = true;
            this.serversListBox.ItemHeight = 15;
            this.serversListBox.Location = new System.Drawing.Point(0, 19);
            this.serversListBox.Name = "serversListBox";
            this.serversListBox.Size = new System.Drawing.Size(201, 356);
            this.serversListBox.Sorted = true;
            this.serversListBox.TabIndex = 28;
            this.serversListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.serversListBox_DrawItem);
            this.serversListBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.serversListBox_MeasureItem);
            this.serversListBox.SelectedIndexChanged += new System.EventHandler(this.serversListBox_SelectedIndexChanged);
            // 
            // directoryChooseButton
            // 
            this.directoryChooseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.directoryChooseButton.Location = new System.Drawing.Point(907, 30);
            this.directoryChooseButton.Name = "directoryChooseButton";
            this.directoryChooseButton.Size = new System.Drawing.Size(22, 20);
            this.directoryChooseButton.TabIndex = 33;
            this.directoryChooseButton.Text = "...";
            this.directoryChooseButton.UseVisualStyleBackColor = true;
            this.directoryChooseButton.Click += new System.EventHandler(this.directoryChooseButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(392, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Save to directory (server, serie and chapter name will be added as subdirectories" +
                "):";
            // 
            // directoryPathTextBox
            // 
            this.directoryPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.directoryPathTextBox.Location = new System.Drawing.Point(6, 30);
            this.directoryPathTextBox.Name = "directoryPathTextBox";
            this.directoryPathTextBox.Size = new System.Drawing.Size(895, 20);
            this.directoryPathTextBox.TabIndex = 22;
            this.directoryPathTextBox.TextChanged += new System.EventHandler(this.directoryPathTextBox_TextChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tasksGridView);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(932, 480);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Downloading";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tasksGridView
            // 
            this.tasksGridView.AllowUserToAddRows = false;
            this.tasksGridView.AllowUserToResizeRows = false;
            this.tasksGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.tasksGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.tasksGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tasksGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.tasksGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tasksGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Delete,
            this.Chapter,
            this.Progress});
            this.tasksGridView.Location = new System.Drawing.Point(3, 3);
            this.tasksGridView.MultiSelect = false;
            this.tasksGridView.Name = "tasksGridView";
            this.tasksGridView.ReadOnly = true;
            this.tasksGridView.RowHeadersVisible = false;
            this.tasksGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tasksGridView.Size = new System.Drawing.Size(926, 474);
            this.tasksGridView.TabIndex = 32;
            this.tasksGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.tasksGridView_CellContentClick);
            // 
            // Delete
            // 
            this.Delete.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Delete.HeaderText = "Delete";
            this.Delete.Name = "Delete";
            this.Delete.ReadOnly = true;
            this.Delete.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Delete.Text = "Delete";
            this.Delete.UseColumnTextForButtonValue = true;
            this.Delete.Width = 44;
            // 
            // Chapter
            // 
            this.Chapter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Chapter.DataPropertyName = "Chapter";
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Chapter.DefaultCellStyle = dataGridViewCellStyle2;
            this.Chapter.HeaderText = "Chapter";
            this.Chapter.Name = "Chapter";
            this.Chapter.ReadOnly = true;
            // 
            // Progress
            // 
            this.Progress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Progress.DataPropertyName = "TaskProgress";
            this.Progress.HeaderText = "Progress";
            this.Progress.Name = "Progress";
            this.Progress.ReadOnly = true;
            this.Progress.Width = 200;
            // 
            // MangaCrawlerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(962, 534);
            this.Controls.Add(this.tabControl1);
            this.Name = "MangaCrawlerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manga Crawler";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MangaCrawlerForm_FormClosing);
            this.Load += new System.EventHandler(this.MangaShareCrawlerForm_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tasksGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox directoryPathTextBox;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button directoryChooseButton;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.TextBox seriesFilterTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Button seriesURLButton;
        private System.Windows.Forms.Label label4;
        private ListBoxEx seriesListBox;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button chapterURLButton;
        private System.Windows.Forms.Button downloadButton;
        private ListBoxEx chaptersListBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button serverURLButton;
        private System.Windows.Forms.Label label5;
        private ListBoxEx serversListBox;
        private System.Windows.Forms.DataGridView tasksGridView;
        private System.Windows.Forms.DataGridViewButtonColumn Delete;
        private System.Windows.Forms.DataGridViewTextBoxColumn Chapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn Progress;
        private System.Windows.Forms.CheckBox cbzCheckBox;
    }
}

