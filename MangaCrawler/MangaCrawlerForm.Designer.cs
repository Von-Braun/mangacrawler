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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MangaCrawlerForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.seriesTabPage = new System.Windows.Forms.TabPage();
            this.splitPanel = new System.Windows.Forms.Panel();
            this.splitter = new System.Windows.Forms.Splitter();
            this.chaptersPanel = new System.Windows.Forms.Panel();
            this.viewPagesButton = new System.Windows.Forms.Button();
            this.openPagesFolder = new System.Windows.Forms.Button();
            this.chapterURLButton = new System.Windows.Forms.Button();
            this.downloadButton = new System.Windows.Forms.Button();
            this.chaptersListBox = new MangaCrawler.ListBoxEx();
            this.label7 = new System.Windows.Forms.Label();
            this.seriesPanel = new System.Windows.Forms.Panel();
            this.checkSeriesButton = new System.Windows.Forms.Button();
            this.seriesSearchTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.openSeriesFolderButton = new System.Windows.Forms.Button();
            this.seriesURLButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.seriesListBox = new MangaCrawler.ListBoxEx();
            this.serversPanel = new System.Windows.Forms.Panel();
            this.openServerFolderButton = new System.Windows.Forms.Button();
            this.serversListBox = new MangaCrawler.ListBoxEx();
            this.serverURLButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.worksTabPage = new System.Windows.Forms.TabPage();
            this.openFolderWorksButton = new System.Windows.Forms.Button();
            this.downloadWorkButton = new System.Windows.Forms.Button();
            this.deleteWorkButton = new System.Windows.Forms.Button();
            this.visitPageWorkButton = new System.Windows.Forms.Button();
            this.viewWorkButton = new System.Windows.Forms.Button();
            this.goToChaptersWorkButton = new System.Windows.Forms.Button();
            this.worksGridView = new System.Windows.Forms.DataGridView();
            this.Chapter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Progress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.optionsTabPage = new System.Windows.Forms.TabPage();
            this.optionslLabel = new System.Windows.Forms.Label();
            this.pageNamingStrategyComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbzCheckBox = new System.Windows.Forms.CheckBox();
            this.mangaRootDirChooseButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.mangaRootDirTextBox = new System.Windows.Forms.TextBox();
            this.logTabPage = new System.Windows.Forms.TabPage();
            this.clearLogButton = new System.Windows.Forms.Button();
            this.logRichTextBox = new System.Windows.Forms.RichTextBox();
            this.bookmarksTabPage = new System.Windows.Forms.TabPage();
            this.versionPanel = new System.Windows.Forms.Panel();
            this.versionLinkLabel = new System.Windows.Forms.LinkLabel();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.tabControl.SuspendLayout();
            this.seriesTabPage.SuspendLayout();
            this.splitPanel.SuspendLayout();
            this.chaptersPanel.SuspendLayout();
            this.seriesPanel.SuspendLayout();
            this.serversPanel.SuspendLayout();
            this.worksTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.worksGridView)).BeginInit();
            this.optionsTabPage.SuspendLayout();
            this.logTabPage.SuspendLayout();
            this.versionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Controls.Add(this.seriesTabPage);
            this.tabControl.Controls.Add(this.worksTabPage);
            this.tabControl.Controls.Add(this.optionsTabPage);
            this.tabControl.Controls.Add(this.logTabPage);
            this.tabControl.Controls.Add(this.bookmarksTabPage);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // seriesTabPage
            // 
            this.seriesTabPage.Controls.Add(this.splitPanel);
            this.seriesTabPage.Controls.Add(this.serversPanel);
            resources.ApplyResources(this.seriesTabPage, "seriesTabPage");
            this.seriesTabPage.Name = "seriesTabPage";
            this.seriesTabPage.UseVisualStyleBackColor = true;
            // 
            // splitPanel
            // 
            resources.ApplyResources(this.splitPanel, "splitPanel");
            this.splitPanel.Controls.Add(this.splitter);
            this.splitPanel.Controls.Add(this.chaptersPanel);
            this.splitPanel.Controls.Add(this.seriesPanel);
            this.splitPanel.Name = "splitPanel";
            // 
            // splitter
            // 
            this.splitter.BackColor = System.Drawing.SystemColors.Menu;
            resources.ApplyResources(this.splitter, "splitter");
            this.splitter.Name = "splitter";
            this.splitter.TabStop = false;
            this.splitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter1_SplitterMoved);
            // 
            // chaptersPanel
            // 
            this.chaptersPanel.Controls.Add(this.viewPagesButton);
            this.chaptersPanel.Controls.Add(this.openPagesFolder);
            this.chaptersPanel.Controls.Add(this.chapterURLButton);
            this.chaptersPanel.Controls.Add(this.downloadButton);
            this.chaptersPanel.Controls.Add(this.chaptersListBox);
            this.chaptersPanel.Controls.Add(this.label7);
            resources.ApplyResources(this.chaptersPanel, "chaptersPanel");
            this.chaptersPanel.MinimumSize = new System.Drawing.Size(334, 0);
            this.chaptersPanel.Name = "chaptersPanel";
            // 
            // viewPagesButton
            // 
            resources.ApplyResources(this.viewPagesButton, "viewPagesButton");
            this.viewPagesButton.Name = "viewPagesButton";
            this.viewPagesButton.UseVisualStyleBackColor = true;
            this.viewPagesButton.Click += new System.EventHandler(this.viewPagesButton_Click);
            // 
            // openPagesFolder
            // 
            resources.ApplyResources(this.openPagesFolder, "openPagesFolder");
            this.openPagesFolder.Name = "openPagesFolder";
            this.openPagesFolder.UseVisualStyleBackColor = true;
            this.openPagesFolder.Click += new System.EventHandler(this.openPagesFolder_Click);
            // 
            // chapterURLButton
            // 
            resources.ApplyResources(this.chapterURLButton, "chapterURLButton");
            this.chapterURLButton.Name = "chapterURLButton";
            this.chapterURLButton.UseVisualStyleBackColor = true;
            this.chapterURLButton.Click += new System.EventHandler(this.chapterURLButton_Click);
            // 
            // downloadButton
            // 
            resources.ApplyResources(this.downloadButton, "downloadButton");
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.UseVisualStyleBackColor = true;
            this.downloadButton.Click += new System.EventHandler(this.downloadButton_Click);
            // 
            // chaptersListBox
            // 
            resources.ApplyResources(this.chaptersListBox, "chaptersListBox");
            this.chaptersListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.chaptersListBox.FormattingEnabled = true;
            this.chaptersListBox.Name = "chaptersListBox";
            this.chaptersListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.chaptersListBox.VerticalScroll += new MangaCrawler.ListBoxEx.ListBoxScrollDelegate(this.chaptersListBox_VerticalScroll);
            this.chaptersListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.chaptersListBox_DrawItem);
            this.chaptersListBox.SelectedIndexChanged += new System.EventHandler(this.chaptersListBox_SelectedIndexChanged);
            this.chaptersListBox.DoubleClick += new System.EventHandler(this.chaptersListBox_DoubleClick);
            this.chaptersListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.chaptersListBox_KeyDown);
            this.chaptersListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.chaptersListBox_MouseDoubleClick);
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // seriesPanel
            // 
            this.seriesPanel.Controls.Add(this.checkSeriesButton);
            this.seriesPanel.Controls.Add(this.seriesSearchTextBox);
            this.seriesPanel.Controls.Add(this.label6);
            this.seriesPanel.Controls.Add(this.openSeriesFolderButton);
            this.seriesPanel.Controls.Add(this.seriesURLButton);
            this.seriesPanel.Controls.Add(this.label4);
            this.seriesPanel.Controls.Add(this.seriesListBox);
            resources.ApplyResources(this.seriesPanel, "seriesPanel");
            this.seriesPanel.MinimumSize = new System.Drawing.Size(234, 0);
            this.seriesPanel.Name = "seriesPanel";
            // 
            // checkSeriesButton
            // 
            resources.ApplyResources(this.checkSeriesButton, "checkSeriesButton");
            this.checkSeriesButton.Name = "checkSeriesButton";
            this.checkSeriesButton.UseVisualStyleBackColor = true;
            this.checkSeriesButton.Click += new System.EventHandler(this.checkSeriesButton_Click);
            // 
            // seriesSearchTextBox
            // 
            resources.ApplyResources(this.seriesSearchTextBox, "seriesSearchTextBox");
            this.seriesSearchTextBox.Name = "seriesSearchTextBox";
            this.seriesSearchTextBox.TextChanged += new System.EventHandler(this.seriesSearchTextBox_TextChanged);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // openSeriesFolderButton
            // 
            resources.ApplyResources(this.openSeriesFolderButton, "openSeriesFolderButton");
            this.openSeriesFolderButton.Name = "openSeriesFolderButton";
            this.openSeriesFolderButton.UseVisualStyleBackColor = true;
            this.openSeriesFolderButton.Click += new System.EventHandler(this.openSeriesFolderButton_Click);
            // 
            // seriesURLButton
            // 
            resources.ApplyResources(this.seriesURLButton, "seriesURLButton");
            this.seriesURLButton.Name = "seriesURLButton";
            this.seriesURLButton.UseVisualStyleBackColor = true;
            this.seriesURLButton.Click += new System.EventHandler(this.seriesURLButton_Click);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // seriesListBox
            // 
            resources.ApplyResources(this.seriesListBox, "seriesListBox");
            this.seriesListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.seriesListBox.FormattingEnabled = true;
            this.seriesListBox.Name = "seriesListBox";
            this.seriesListBox.VerticalScroll += new MangaCrawler.ListBoxEx.ListBoxScrollDelegate(this.seriesListBox_VerticalScroll);
            this.seriesListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.seriesListBox_DrawItem);
            this.seriesListBox.SelectedIndexChanged += new System.EventHandler(this.seriesListBox_SelectedIndexChanged);
            // 
            // serversPanel
            // 
            resources.ApplyResources(this.serversPanel, "serversPanel");
            this.serversPanel.Controls.Add(this.openServerFolderButton);
            this.serversPanel.Controls.Add(this.serversListBox);
            this.serversPanel.Controls.Add(this.serverURLButton);
            this.serversPanel.Controls.Add(this.label5);
            this.serversPanel.Name = "serversPanel";
            // 
            // openServerFolderButton
            // 
            resources.ApplyResources(this.openServerFolderButton, "openServerFolderButton");
            this.openServerFolderButton.Name = "openServerFolderButton";
            this.openServerFolderButton.UseVisualStyleBackColor = true;
            this.openServerFolderButton.Click += new System.EventHandler(this.openServerFolderButton_Click);
            // 
            // serversListBox
            // 
            resources.ApplyResources(this.serversListBox, "serversListBox");
            this.serversListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.serversListBox.FormattingEnabled = true;
            this.serversListBox.Name = "serversListBox";
            this.serversListBox.Sorted = true;
            this.serversListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.serversListBox_DrawItem);
            this.serversListBox.SelectedIndexChanged += new System.EventHandler(this.serversListBox_SelectedIndexChanged);
            // 
            // serverURLButton
            // 
            resources.ApplyResources(this.serverURLButton, "serverURLButton");
            this.serverURLButton.Name = "serverURLButton";
            this.serverURLButton.UseVisualStyleBackColor = true;
            this.serverURLButton.Click += new System.EventHandler(this.serverURLButton_Click);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // worksTabPage
            // 
            this.worksTabPage.Controls.Add(this.openFolderWorksButton);
            this.worksTabPage.Controls.Add(this.downloadWorkButton);
            this.worksTabPage.Controls.Add(this.deleteWorkButton);
            this.worksTabPage.Controls.Add(this.visitPageWorkButton);
            this.worksTabPage.Controls.Add(this.viewWorkButton);
            this.worksTabPage.Controls.Add(this.goToChaptersWorkButton);
            this.worksTabPage.Controls.Add(this.worksGridView);
            resources.ApplyResources(this.worksTabPage, "worksTabPage");
            this.worksTabPage.Name = "worksTabPage";
            this.worksTabPage.UseVisualStyleBackColor = true;
            // 
            // openFolderWorksButton
            // 
            resources.ApplyResources(this.openFolderWorksButton, "openFolderWorksButton");
            this.openFolderWorksButton.Name = "openFolderWorksButton";
            this.openFolderWorksButton.UseVisualStyleBackColor = true;
            this.openFolderWorksButton.Click += new System.EventHandler(this.openFolderWorksButton_Click);
            // 
            // downloadWorkButton
            // 
            resources.ApplyResources(this.downloadWorkButton, "downloadWorkButton");
            this.downloadWorkButton.Name = "downloadWorkButton";
            this.downloadWorkButton.UseVisualStyleBackColor = true;
            this.downloadWorkButton.Click += new System.EventHandler(this.downloadWorkButton_Click);
            // 
            // deleteWorkButton
            // 
            resources.ApplyResources(this.deleteWorkButton, "deleteWorkButton");
            this.deleteWorkButton.Name = "deleteWorkButton";
            this.deleteWorkButton.UseVisualStyleBackColor = true;
            this.deleteWorkButton.Click += new System.EventHandler(this.deleteWorkButton_Click);
            // 
            // visitPageWorkButton
            // 
            resources.ApplyResources(this.visitPageWorkButton, "visitPageWorkButton");
            this.visitPageWorkButton.Name = "visitPageWorkButton";
            this.visitPageWorkButton.UseVisualStyleBackColor = true;
            this.visitPageWorkButton.Click += new System.EventHandler(this.visitPageWorkButton_Click);
            // 
            // viewWorkButton
            // 
            resources.ApplyResources(this.viewWorkButton, "viewWorkButton");
            this.viewWorkButton.Name = "viewWorkButton";
            this.viewWorkButton.UseVisualStyleBackColor = true;
            this.viewWorkButton.Click += new System.EventHandler(this.viewWorkButton_Click);
            // 
            // goToChaptersWorkButton
            // 
            resources.ApplyResources(this.goToChaptersWorkButton, "goToChaptersWorkButton");
            this.goToChaptersWorkButton.Name = "goToChaptersWorkButton";
            this.goToChaptersWorkButton.UseVisualStyleBackColor = true;
            this.goToChaptersWorkButton.Click += new System.EventHandler(this.goToChaptersWorkButton_Click);
            // 
            // worksGridView
            // 
            this.worksGridView.AllowUserToAddRows = false;
            this.worksGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.worksGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.worksGridView, "worksGridView");
            this.worksGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.worksGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.worksGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.worksGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.worksGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.worksGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Chapter,
            this.Progress});
            this.worksGridView.Name = "worksGridView";
            this.worksGridView.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.worksGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.worksGridView.RowHeadersVisible = false;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.worksGridView.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.worksGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.worksGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.worksGridView_CellContentClick);
            this.worksGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.worksGridView_KeyDown);
            // 
            // Chapter
            // 
            this.Chapter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Chapter.DataPropertyName = "Info";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Chapter.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.Chapter, "Chapter");
            this.Chapter.Name = "Chapter";
            this.Chapter.ReadOnly = true;
            // 
            // Progress
            // 
            this.Progress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Progress.DataPropertyName = "Progress";
            resources.ApplyResources(this.Progress, "Progress");
            this.Progress.Name = "Progress";
            this.Progress.ReadOnly = true;
            // 
            // optionsTabPage
            // 
            this.optionsTabPage.Controls.Add(this.optionslLabel);
            this.optionsTabPage.Controls.Add(this.pageNamingStrategyComboBox);
            this.optionsTabPage.Controls.Add(this.label1);
            this.optionsTabPage.Controls.Add(this.cbzCheckBox);
            this.optionsTabPage.Controls.Add(this.mangaRootDirChooseButton);
            this.optionsTabPage.Controls.Add(this.label2);
            this.optionsTabPage.Controls.Add(this.mangaRootDirTextBox);
            resources.ApplyResources(this.optionsTabPage, "optionsTabPage");
            this.optionsTabPage.Name = "optionsTabPage";
            this.optionsTabPage.UseVisualStyleBackColor = true;
            // 
            // optionslLabel
            // 
            resources.ApplyResources(this.optionslLabel, "optionslLabel");
            this.optionslLabel.ForeColor = System.Drawing.Color.Red;
            this.optionslLabel.Name = "optionslLabel";
            // 
            // pageNamingStrategyComboBox
            // 
            this.pageNamingStrategyComboBox.FormattingEnabled = true;
            this.pageNamingStrategyComboBox.Items.AddRange(new object[] {
            resources.GetString("pageNamingStrategyComboBox.Items"),
            resources.GetString("pageNamingStrategyComboBox.Items1"),
            resources.GetString("pageNamingStrategyComboBox.Items2")});
            resources.ApplyResources(this.pageNamingStrategyComboBox, "pageNamingStrategyComboBox");
            this.pageNamingStrategyComboBox.Name = "pageNamingStrategyComboBox";
            this.pageNamingStrategyComboBox.SelectedIndexChanged += new System.EventHandler(this.pageNamingStrategyComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cbzCheckBox
            // 
            resources.ApplyResources(this.cbzCheckBox, "cbzCheckBox");
            this.cbzCheckBox.Name = "cbzCheckBox";
            this.cbzCheckBox.UseVisualStyleBackColor = true;
            this.cbzCheckBox.Click += new System.EventHandler(this.cbzCheckBox_CheckedChanged);
            // 
            // mangaRootDirChooseButton
            // 
            resources.ApplyResources(this.mangaRootDirChooseButton, "mangaRootDirChooseButton");
            this.mangaRootDirChooseButton.Name = "mangaRootDirChooseButton";
            this.mangaRootDirChooseButton.UseVisualStyleBackColor = true;
            this.mangaRootDirChooseButton.Click += new System.EventHandler(this.mangaRootDirChooseButton_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // mangaRootDirTextBox
            // 
            resources.ApplyResources(this.mangaRootDirTextBox, "mangaRootDirTextBox");
            this.mangaRootDirTextBox.Name = "mangaRootDirTextBox";
            this.mangaRootDirTextBox.TextChanged += new System.EventHandler(this.mangaRootDirTextBox_TextChanged);
            // 
            // logTabPage
            // 
            this.logTabPage.Controls.Add(this.clearLogButton);
            this.logTabPage.Controls.Add(this.logRichTextBox);
            resources.ApplyResources(this.logTabPage, "logTabPage");
            this.logTabPage.Name = "logTabPage";
            this.logTabPage.UseVisualStyleBackColor = true;
            // 
            // clearLogButton
            // 
            resources.ApplyResources(this.clearLogButton, "clearLogButton");
            this.clearLogButton.Name = "clearLogButton";
            this.clearLogButton.UseVisualStyleBackColor = true;
            this.clearLogButton.Click += new System.EventHandler(this.clearLogButton_Click);
            // 
            // logRichTextBox
            // 
            resources.ApplyResources(this.logRichTextBox, "logRichTextBox");
            this.logRichTextBox.Name = "logRichTextBox";
            // 
            // bookmarksTabPage
            // 
            resources.ApplyResources(this.bookmarksTabPage, "bookmarksTabPage");
            this.bookmarksTabPage.Name = "bookmarksTabPage";
            this.bookmarksTabPage.UseVisualStyleBackColor = true;
            // 
            // versionPanel
            // 
            resources.ApplyResources(this.versionPanel, "versionPanel");
            this.versionPanel.Controls.Add(this.versionLinkLabel);
            this.versionPanel.Name = "versionPanel";
            // 
            // versionLinkLabel
            // 
            resources.ApplyResources(this.versionLinkLabel, "versionLinkLabel");
            this.versionLinkLabel.Name = "versionLinkLabel";
            this.versionLinkLabel.TabStop = true;
            this.versionLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // refreshTimer
            // 
            this.refreshTimer.Interval = 500;
            this.refreshTimer.Tick += new System.EventHandler(this.refreshTimer_Tick);
            // 
            // MangaCrawlerForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.versionPanel);
            this.Controls.Add(this.tabControl);
            this.Name = "MangaCrawlerForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MangaCrawlerForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MangaCrawlerForm_FormClosed);
            this.Load += new System.EventHandler(this.MangaShareCrawlerForm_Load);
            this.Shown += new System.EventHandler(this.MangaCrawlerForm_Shown);
            this.ResizeEnd += new System.EventHandler(this.MangaCrawlerForm_ResizeEnd);
            this.tabControl.ResumeLayout(false);
            this.seriesTabPage.ResumeLayout(false);
            this.splitPanel.ResumeLayout(false);
            this.chaptersPanel.ResumeLayout(false);
            this.chaptersPanel.PerformLayout();
            this.seriesPanel.ResumeLayout(false);
            this.seriesPanel.PerformLayout();
            this.serversPanel.ResumeLayout(false);
            this.serversPanel.PerformLayout();
            this.worksTabPage.ResumeLayout(false);
            this.worksTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.worksGridView)).EndInit();
            this.optionsTabPage.ResumeLayout(false);
            this.optionsTabPage.PerformLayout();
            this.logTabPage.ResumeLayout(false);
            this.versionPanel.ResumeLayout(false);
            this.versionPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage seriesTabPage;
        private System.Windows.Forms.TabPage worksTabPage;
        private System.Windows.Forms.Panel serversPanel;
        private System.Windows.Forms.Button serverURLButton;
        private System.Windows.Forms.Label label5;
        private ListBoxEx serversListBox;
        private System.Windows.Forms.DataGridView worksGridView;
        private System.Windows.Forms.Panel versionPanel;
        private System.Windows.Forms.LinkLabel versionLinkLabel;
        private System.Windows.Forms.Panel splitPanel;
        private System.Windows.Forms.Panel chaptersPanel;
        private System.Windows.Forms.Button chapterURLButton;
        private System.Windows.Forms.Button downloadButton;
        private ListBoxEx chaptersListBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel seriesPanel;
        private System.Windows.Forms.TextBox seriesSearchTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private ListBoxEx seriesListBox;
        private System.Windows.Forms.Splitter splitter;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.TabPage logTabPage;
        private System.Windows.Forms.Button clearLogButton;
        private System.Windows.Forms.RichTextBox logRichTextBox;
        private System.Windows.Forms.Button viewPagesButton;
        private System.Windows.Forms.Button openPagesFolder;
        private System.Windows.Forms.Button openSeriesFolderButton;
        private System.Windows.Forms.Button seriesURLButton;
        private System.Windows.Forms.Button openServerFolderButton;
        private System.Windows.Forms.TabPage optionsTabPage;
        private System.Windows.Forms.CheckBox cbzCheckBox;
        private System.Windows.Forms.Button mangaRootDirChooseButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox mangaRootDirTextBox;
        private System.Windows.Forms.ComboBox pageNamingStrategyComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label optionslLabel;
        private System.Windows.Forms.Button downloadWorkButton;
        private System.Windows.Forms.Button deleteWorkButton;
        private System.Windows.Forms.Button visitPageWorkButton;
        private System.Windows.Forms.Button viewWorkButton;
        private System.Windows.Forms.Button goToChaptersWorkButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn Chapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn Progress;
        private System.Windows.Forms.Button openFolderWorksButton;
        private System.Windows.Forms.TabPage bookmarksTabPage;
        private System.Windows.Forms.Button checkSeriesButton;
    }
}

