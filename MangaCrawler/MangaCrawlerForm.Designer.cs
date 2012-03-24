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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.versionPanel = new System.Windows.Forms.Panel();
            this.versionLinkLabel = new System.Windows.Forms.LinkLabel();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.bookmarksTabPage = new System.Windows.Forms.TabPage();
            this.splitBookmarksPanel = new System.Windows.Forms.Panel();
            this.splitterBookmarks = new System.Windows.Forms.Splitter();
            this.chapterBookmarksPanel = new System.Windows.Forms.Panel();
            this.viewChapterBoksButton = new System.Windows.Forms.Button();
            this.openChapterFolderBooksButton = new System.Windows.Forms.Button();
            this.visitChapterBooksButton = new System.Windows.Forms.Button();
            this.downloadChapterBooksButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.serieBookmarksPanel = new System.Windows.Forms.Panel();
            this.checkNowBookmarksButton = new System.Windows.Forms.Button();
            this.removeSerieBooksPanel = new System.Windows.Forms.Button();
            this.openSerieFolderBooksButton = new System.Windows.Forms.Button();
            this.visitSerieBooksButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.logTabPage = new System.Windows.Forms.TabPage();
            this.clearLogButton = new System.Windows.Forms.Button();
            this.logRichTextBox = new System.Windows.Forms.RichTextBox();
            this.optionsTabPage = new System.Windows.Forms.TabPage();
            this.showBaloonTipsCheckBox = new System.Windows.Forms.CheckBox();
            this.minimizeOnCloseCheckBox = new System.Windows.Forms.CheckBox();
            this.playSoundWhenDownloadedCheckBox = new System.Windows.Forms.CheckBox();
            this.optionslLabel = new System.Windows.Forms.Label();
            this.pageNamingStrategyComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbzCheckBox = new System.Windows.Forms.CheckBox();
            this.mangaRootDirChooseButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.mangaRootDirTextBox = new System.Windows.Forms.TextBox();
            this.worksTabPage = new System.Windows.Forms.TabPage();
            this.clearWorkButton = new System.Windows.Forms.Button();
            this.openFolderWorksButton = new System.Windows.Forms.Button();
            this.downloadWorkButton = new System.Windows.Forms.Button();
            this.cancelWorkButton = new System.Windows.Forms.Button();
            this.visitPageWorkButton = new System.Windows.Forms.Button();
            this.viewWorkButton = new System.Windows.Forms.Button();
            this.goToSeriesTabButton = new System.Windows.Forms.Button();
            this.worksGridView = new System.Windows.Forms.DataGridView();
            this.Chapter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Progress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.seriesTabPage = new System.Windows.Forms.TabPage();
            this.splitPanel = new System.Windows.Forms.Panel();
            this.splitter = new System.Windows.Forms.Splitter();
            this.chaptersPanel = new System.Windows.Forms.Panel();
            this.viewPagesButton = new System.Windows.Forms.Button();
            this.openPagesFolder = new System.Windows.Forms.Button();
            this.chapterURLButton = new System.Windows.Forms.Button();
            this.downloadButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.seriesPanel = new System.Windows.Forms.Panel();
            this.checkNowSerieButton = new System.Windows.Forms.Button();
            this.BookmarkSerieButton = new System.Windows.Forms.Button();
            this.seriesSearchTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.openSeriesFolderButton = new System.Windows.Forms.Button();
            this.seriesURLButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.serversPanel = new System.Windows.Forms.Panel();
            this.checkNowServerButton = new System.Windows.Forms.Button();
            this.openServerFolderButton = new System.Windows.Forms.Button();
            this.serverURLButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.debugContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetCheckDatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSerieFirsttoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSerieMiddleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSerieLastToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeSerieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addChapterFirstToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addChapterMiddleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addChapterLastToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeChapterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameSerieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameChapterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSerieURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeChapterURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forceBookmarksCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAllFromCatalogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitTrayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarksTimer = new System.Windows.Forms.Timer(this.components);
            this.chaptersListBox = new MangaCrawler.ListBoxEx();
            this.seriesListBox = new MangaCrawler.ListBoxEx();
            this.serversListBox = new MangaCrawler.ListBoxEx();
            this.chapterBookmarksListBox = new MangaCrawler.ListBoxEx();
            this.serieBookmarksListBox = new MangaCrawler.ListBoxEx();
            this.versionPanel.SuspendLayout();
            this.bookmarksTabPage.SuspendLayout();
            this.splitBookmarksPanel.SuspendLayout();
            this.chapterBookmarksPanel.SuspendLayout();
            this.serieBookmarksPanel.SuspendLayout();
            this.logTabPage.SuspendLayout();
            this.optionsTabPage.SuspendLayout();
            this.worksTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.worksGridView)).BeginInit();
            this.seriesTabPage.SuspendLayout();
            this.splitPanel.SuspendLayout();
            this.chaptersPanel.SuspendLayout();
            this.seriesPanel.SuspendLayout();
            this.serversPanel.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.debugContextMenuStrip.SuspendLayout();
            this.trayContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
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
            this.versionLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.versionLinkLabel_LinkClicked);
            // 
            // refreshTimer
            // 
            this.refreshTimer.Interval = 500;
            this.refreshTimer.Tick += new System.EventHandler(this.refreshTimer_Tick);
            // 
            // bookmarksTabPage
            // 
            this.bookmarksTabPage.Controls.Add(this.splitBookmarksPanel);
            resources.ApplyResources(this.bookmarksTabPage, "bookmarksTabPage");
            this.bookmarksTabPage.Name = "bookmarksTabPage";
            this.bookmarksTabPage.UseVisualStyleBackColor = true;
            // 
            // splitBookmarksPanel
            // 
            this.splitBookmarksPanel.Controls.Add(this.splitterBookmarks);
            this.splitBookmarksPanel.Controls.Add(this.chapterBookmarksPanel);
            this.splitBookmarksPanel.Controls.Add(this.serieBookmarksPanel);
            resources.ApplyResources(this.splitBookmarksPanel, "splitBookmarksPanel");
            this.splitBookmarksPanel.MinimumSize = new System.Drawing.Size(782, 0);
            this.splitBookmarksPanel.Name = "splitBookmarksPanel";
            // 
            // splitterBookmarks
            // 
            this.splitterBookmarks.BackColor = System.Drawing.SystemColors.Menu;
            resources.ApplyResources(this.splitterBookmarks, "splitterBookmarks");
            this.splitterBookmarks.Name = "splitterBookmarks";
            this.splitterBookmarks.TabStop = false;
            this.splitterBookmarks.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitterBookmarks_SplitterMoved);
            // 
            // chapterBookmarksPanel
            // 
            this.chapterBookmarksPanel.Controls.Add(this.viewChapterBoksButton);
            this.chapterBookmarksPanel.Controls.Add(this.openChapterFolderBooksButton);
            this.chapterBookmarksPanel.Controls.Add(this.visitChapterBooksButton);
            this.chapterBookmarksPanel.Controls.Add(this.downloadChapterBooksButton);
            this.chapterBookmarksPanel.Controls.Add(this.chapterBookmarksListBox);
            this.chapterBookmarksPanel.Controls.Add(this.label9);
            resources.ApplyResources(this.chapterBookmarksPanel, "chapterBookmarksPanel");
            this.chapterBookmarksPanel.MinimumSize = new System.Drawing.Size(439, 0);
            this.chapterBookmarksPanel.Name = "chapterBookmarksPanel";
            // 
            // viewChapterBoksButton
            // 
            resources.ApplyResources(this.viewChapterBoksButton, "viewChapterBoksButton");
            this.viewChapterBoksButton.Name = "viewChapterBoksButton";
            this.viewChapterBoksButton.UseVisualStyleBackColor = true;
            this.viewChapterBoksButton.Click += new System.EventHandler(this.viewChapterBoksButton_Click);
            // 
            // openChapterFolderBooksButton
            // 
            resources.ApplyResources(this.openChapterFolderBooksButton, "openChapterFolderBooksButton");
            this.openChapterFolderBooksButton.Name = "openChapterFolderBooksButton";
            this.openChapterFolderBooksButton.UseVisualStyleBackColor = true;
            this.openChapterFolderBooksButton.Click += new System.EventHandler(this.openChapterFolderBooksButton_Click);
            // 
            // visitChapterBooksButton
            // 
            resources.ApplyResources(this.visitChapterBooksButton, "visitChapterBooksButton");
            this.visitChapterBooksButton.Name = "visitChapterBooksButton";
            this.visitChapterBooksButton.UseVisualStyleBackColor = true;
            this.visitChapterBooksButton.Click += new System.EventHandler(this.visitChapterBooksButton_Click);
            // 
            // downloadChapterBooksButton
            // 
            resources.ApplyResources(this.downloadChapterBooksButton, "downloadChapterBooksButton");
            this.downloadChapterBooksButton.Name = "downloadChapterBooksButton";
            this.downloadChapterBooksButton.UseVisualStyleBackColor = true;
            this.downloadChapterBooksButton.Click += new System.EventHandler(this.downloadChapterBooksButton_Click);
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // serieBookmarksPanel
            // 
            this.serieBookmarksPanel.Controls.Add(this.checkNowBookmarksButton);
            this.serieBookmarksPanel.Controls.Add(this.removeSerieBooksPanel);
            this.serieBookmarksPanel.Controls.Add(this.openSerieFolderBooksButton);
            this.serieBookmarksPanel.Controls.Add(this.visitSerieBooksButton);
            this.serieBookmarksPanel.Controls.Add(this.label8);
            this.serieBookmarksPanel.Controls.Add(this.serieBookmarksListBox);
            resources.ApplyResources(this.serieBookmarksPanel, "serieBookmarksPanel");
            this.serieBookmarksPanel.MinimumSize = new System.Drawing.Size(343, 0);
            this.serieBookmarksPanel.Name = "serieBookmarksPanel";
            // 
            // checkNowBookmarksButton
            // 
            resources.ApplyResources(this.checkNowBookmarksButton, "checkNowBookmarksButton");
            this.checkNowBookmarksButton.Name = "checkNowBookmarksButton";
            this.checkNowBookmarksButton.UseVisualStyleBackColor = true;
            this.checkNowBookmarksButton.Click += new System.EventHandler(this.checkNowBookmarksButton_Click);
            // 
            // removeSerieBooksPanel
            // 
            resources.ApplyResources(this.removeSerieBooksPanel, "removeSerieBooksPanel");
            this.removeSerieBooksPanel.Name = "removeSerieBooksPanel";
            this.removeSerieBooksPanel.UseVisualStyleBackColor = true;
            this.removeSerieBooksPanel.Click += new System.EventHandler(this.removeSerieBooksPanel_Click);
            // 
            // openSerieFolderBooksButton
            // 
            resources.ApplyResources(this.openSerieFolderBooksButton, "openSerieFolderBooksButton");
            this.openSerieFolderBooksButton.Name = "openSerieFolderBooksButton";
            this.openSerieFolderBooksButton.UseVisualStyleBackColor = true;
            this.openSerieFolderBooksButton.Click += new System.EventHandler(this.openSerieFolderBooksButton_Click);
            // 
            // visitSerieBooksButton
            // 
            resources.ApplyResources(this.visitSerieBooksButton, "visitSerieBooksButton");
            this.visitSerieBooksButton.Name = "visitSerieBooksButton";
            this.visitSerieBooksButton.UseVisualStyleBackColor = true;
            this.visitSerieBooksButton.Click += new System.EventHandler(this.visitSerieBooksButton_Click);
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
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
            // optionsTabPage
            // 
            this.optionsTabPage.Controls.Add(this.showBaloonTipsCheckBox);
            this.optionsTabPage.Controls.Add(this.minimizeOnCloseCheckBox);
            this.optionsTabPage.Controls.Add(this.playSoundWhenDownloadedCheckBox);
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
            // showBaloonTipsCheckBox
            // 
            resources.ApplyResources(this.showBaloonTipsCheckBox, "showBaloonTipsCheckBox");
            this.showBaloonTipsCheckBox.Name = "showBaloonTipsCheckBox";
            this.showBaloonTipsCheckBox.UseVisualStyleBackColor = true;
            this.showBaloonTipsCheckBox.CheckedChanged += new System.EventHandler(this.showBaloonTipsCheckBox_CheckedChanged);
            // 
            // minimizeOnCloseCheckBox
            // 
            resources.ApplyResources(this.minimizeOnCloseCheckBox, "minimizeOnCloseCheckBox");
            this.minimizeOnCloseCheckBox.Name = "minimizeOnCloseCheckBox";
            this.minimizeOnCloseCheckBox.UseVisualStyleBackColor = true;
            this.minimizeOnCloseCheckBox.CheckedChanged += new System.EventHandler(this.minimizeOnCloseCheckBox_CheckedChanged);
            // 
            // playSoundWhenDownloadedCheckBox
            // 
            resources.ApplyResources(this.playSoundWhenDownloadedCheckBox, "playSoundWhenDownloadedCheckBox");
            this.playSoundWhenDownloadedCheckBox.Name = "playSoundWhenDownloadedCheckBox";
            this.playSoundWhenDownloadedCheckBox.UseVisualStyleBackColor = true;
            this.playSoundWhenDownloadedCheckBox.CheckedChanged += new System.EventHandler(this.playSoundWhenDownloadedCheckBox_CheckedChanged);
            // 
            // optionslLabel
            // 
            resources.ApplyResources(this.optionslLabel, "optionslLabel");
            this.optionslLabel.ForeColor = System.Drawing.Color.Red;
            this.optionslLabel.Name = "optionslLabel";
            // 
            // pageNamingStrategyComboBox
            // 
            this.pageNamingStrategyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pageNamingStrategyComboBox.FormattingEnabled = true;
            this.pageNamingStrategyComboBox.Items.AddRange(new object[] {
            resources.GetString("pageNamingStrategyComboBox.Items"),
            resources.GetString("pageNamingStrategyComboBox.Items1"),
            resources.GetString("pageNamingStrategyComboBox.Items2"),
            resources.GetString("pageNamingStrategyComboBox.Items3"),
            resources.GetString("pageNamingStrategyComboBox.Items4")});
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
            // worksTabPage
            // 
            this.worksTabPage.Controls.Add(this.clearWorkButton);
            this.worksTabPage.Controls.Add(this.openFolderWorksButton);
            this.worksTabPage.Controls.Add(this.downloadWorkButton);
            this.worksTabPage.Controls.Add(this.cancelWorkButton);
            this.worksTabPage.Controls.Add(this.visitPageWorkButton);
            this.worksTabPage.Controls.Add(this.viewWorkButton);
            this.worksTabPage.Controls.Add(this.goToSeriesTabButton);
            this.worksTabPage.Controls.Add(this.worksGridView);
            resources.ApplyResources(this.worksTabPage, "worksTabPage");
            this.worksTabPage.Name = "worksTabPage";
            this.worksTabPage.UseVisualStyleBackColor = true;
            // 
            // clearWorkButton
            // 
            resources.ApplyResources(this.clearWorkButton, "clearWorkButton");
            this.clearWorkButton.Name = "clearWorkButton";
            this.clearWorkButton.Tag = "Clear finished chapters";
            this.clearWorkButton.UseVisualStyleBackColor = true;
            this.clearWorkButton.Click += new System.EventHandler(this.clearWorkButton_Click);
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
            // cancelWorkButton
            // 
            resources.ApplyResources(this.cancelWorkButton, "cancelWorkButton");
            this.cancelWorkButton.Name = "cancelWorkButton";
            this.cancelWorkButton.Tag = "Cancel downloading";
            this.cancelWorkButton.UseVisualStyleBackColor = true;
            this.cancelWorkButton.Click += new System.EventHandler(this.cancelWorkButton_Click);
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
            // goToSeriesTabButton
            // 
            resources.ApplyResources(this.goToSeriesTabButton, "goToSeriesTabButton");
            this.goToSeriesTabButton.Name = "goToSeriesTabButton";
            this.goToSeriesTabButton.UseVisualStyleBackColor = true;
            this.goToSeriesTabButton.Click += new System.EventHandler(this.goToSeriesTabButton_Click);
            // 
            // worksGridView
            // 
            this.worksGridView.AllowUserToAddRows = false;
            this.worksGridView.AllowUserToDeleteRows = false;
            this.worksGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.worksGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            resources.ApplyResources(this.worksGridView, "worksGridView");
            this.worksGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.worksGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.worksGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.worksGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.worksGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.worksGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Chapter,
            this.Progress});
            this.worksGridView.Name = "worksGridView";
            this.worksGridView.ReadOnly = true;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.worksGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.worksGridView.RowHeadersVisible = false;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.worksGridView.RowsDefaultCellStyle = dataGridViewCellStyle10;
            this.worksGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.worksGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.worksGridView_KeyDown);
            this.worksGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.worksGridView_MouseDown);
            // 
            // Chapter
            // 
            this.Chapter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Chapter.DataPropertyName = "Info";
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Chapter.DefaultCellStyle = dataGridViewCellStyle8;
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
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // seriesPanel
            // 
            this.seriesPanel.Controls.Add(this.checkNowSerieButton);
            this.seriesPanel.Controls.Add(this.BookmarkSerieButton);
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
            // checkNowSerieButton
            // 
            resources.ApplyResources(this.checkNowSerieButton, "checkNowSerieButton");
            this.checkNowSerieButton.Name = "checkNowSerieButton";
            this.checkNowSerieButton.UseVisualStyleBackColor = true;
            this.checkNowSerieButton.Click += new System.EventHandler(this.checkNowSerieButton_Click);
            // 
            // BookmarkSerieButton
            // 
            resources.ApplyResources(this.BookmarkSerieButton, "BookmarkSerieButton");
            this.BookmarkSerieButton.Name = "BookmarkSerieButton";
            this.BookmarkSerieButton.UseVisualStyleBackColor = true;
            this.BookmarkSerieButton.Click += new System.EventHandler(this.BookmarkSerieButton_Click);
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
            // serversPanel
            // 
            resources.ApplyResources(this.serversPanel, "serversPanel");
            this.serversPanel.Controls.Add(this.checkNowServerButton);
            this.serversPanel.Controls.Add(this.openServerFolderButton);
            this.serversPanel.Controls.Add(this.serversListBox);
            this.serversPanel.Controls.Add(this.serverURLButton);
            this.serversPanel.Controls.Add(this.label5);
            this.serversPanel.Name = "serversPanel";
            // 
            // checkNowServerButton
            // 
            resources.ApplyResources(this.checkNowServerButton, "checkNowServerButton");
            this.checkNowServerButton.Name = "checkNowServerButton";
            this.checkNowServerButton.UseVisualStyleBackColor = true;
            this.checkNowServerButton.Click += new System.EventHandler(this.checkNowServerButton_Click);
            // 
            // openServerFolderButton
            // 
            resources.ApplyResources(this.openServerFolderButton, "openServerFolderButton");
            this.openServerFolderButton.Name = "openServerFolderButton";
            this.openServerFolderButton.UseVisualStyleBackColor = true;
            this.openServerFolderButton.Click += new System.EventHandler(this.openServerFolderButton_Click);
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
            // tabControl
            // 
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Controls.Add(this.seriesTabPage);
            this.tabControl.Controls.Add(this.worksTabPage);
            this.tabControl.Controls.Add(this.bookmarksTabPage);
            this.tabControl.Controls.Add(this.optionsTabPage);
            this.tabControl.Controls.Add(this.logTabPage);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // debugContextMenuStrip
            // 
            this.debugContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetCheckDatesToolStripMenuItem,
            this.addSerieFirsttoolStripMenuItem,
            this.addSerieMiddleToolStripMenuItem,
            this.addSerieLastToolStripMenuItem,
            this.removeSerieToolStripMenuItem,
            this.addChapterFirstToolStripMenuItem,
            this.addChapterMiddleToolStripMenuItem,
            this.addChapterLastToolStripMenuItem,
            this.removeChapterToolStripMenuItem,
            this.renameSerieToolStripMenuItem,
            this.renameChapterToolStripMenuItem,
            this.changeSerieURLToolStripMenuItem,
            this.changeChapterURLToolStripMenuItem,
            this.forceBookmarksCheckToolStripMenuItem,
            this.clearMemoryToolStripMenuItem,
            this.loadAllFromCatalogToolStripMenuItem});
            this.debugContextMenuStrip.Name = "debugContextMenuStrip";
            resources.ApplyResources(this.debugContextMenuStrip, "debugContextMenuStrip");
            // 
            // resetCheckDatesToolStripMenuItem
            // 
            this.resetCheckDatesToolStripMenuItem.Name = "resetCheckDatesToolStripMenuItem";
            resources.ApplyResources(this.resetCheckDatesToolStripMenuItem, "resetCheckDatesToolStripMenuItem");
            this.resetCheckDatesToolStripMenuItem.Click += new System.EventHandler(this.resetCheckDatesToolStripMenuItem_Click);
            // 
            // addSerieFirsttoolStripMenuItem
            // 
            this.addSerieFirsttoolStripMenuItem.Name = "addSerieFirsttoolStripMenuItem";
            resources.ApplyResources(this.addSerieFirsttoolStripMenuItem, "addSerieFirsttoolStripMenuItem");
            this.addSerieFirsttoolStripMenuItem.Click += new System.EventHandler(this.addSerieFirsttoolStripMenuItem_Click);
            // 
            // addSerieMiddleToolStripMenuItem
            // 
            this.addSerieMiddleToolStripMenuItem.Name = "addSerieMiddleToolStripMenuItem";
            resources.ApplyResources(this.addSerieMiddleToolStripMenuItem, "addSerieMiddleToolStripMenuItem");
            this.addSerieMiddleToolStripMenuItem.Click += new System.EventHandler(this.addSerieMiddleToolStripMenuItem_Click);
            // 
            // addSerieLastToolStripMenuItem
            // 
            this.addSerieLastToolStripMenuItem.Name = "addSerieLastToolStripMenuItem";
            resources.ApplyResources(this.addSerieLastToolStripMenuItem, "addSerieLastToolStripMenuItem");
            this.addSerieLastToolStripMenuItem.Click += new System.EventHandler(this.addSerieLastToolStripMenuItem_Click);
            // 
            // removeSerieToolStripMenuItem
            // 
            this.removeSerieToolStripMenuItem.Name = "removeSerieToolStripMenuItem";
            resources.ApplyResources(this.removeSerieToolStripMenuItem, "removeSerieToolStripMenuItem");
            this.removeSerieToolStripMenuItem.Click += new System.EventHandler(this.removeSerieToolStripMenuItem_Click);
            // 
            // addChapterFirstToolStripMenuItem
            // 
            this.addChapterFirstToolStripMenuItem.Name = "addChapterFirstToolStripMenuItem";
            resources.ApplyResources(this.addChapterFirstToolStripMenuItem, "addChapterFirstToolStripMenuItem");
            this.addChapterFirstToolStripMenuItem.Click += new System.EventHandler(this.addChapterFirstToolStripMenuItem_Click);
            // 
            // addChapterMiddleToolStripMenuItem
            // 
            this.addChapterMiddleToolStripMenuItem.Name = "addChapterMiddleToolStripMenuItem";
            resources.ApplyResources(this.addChapterMiddleToolStripMenuItem, "addChapterMiddleToolStripMenuItem");
            this.addChapterMiddleToolStripMenuItem.Click += new System.EventHandler(this.addChapterMiddleToolStripMenuItem_Click);
            // 
            // addChapterLastToolStripMenuItem
            // 
            this.addChapterLastToolStripMenuItem.Name = "addChapterLastToolStripMenuItem";
            resources.ApplyResources(this.addChapterLastToolStripMenuItem, "addChapterLastToolStripMenuItem");
            this.addChapterLastToolStripMenuItem.Click += new System.EventHandler(this.addChapterLastToolStripMenuItem_Click);
            // 
            // removeChapterToolStripMenuItem
            // 
            this.removeChapterToolStripMenuItem.Name = "removeChapterToolStripMenuItem";
            resources.ApplyResources(this.removeChapterToolStripMenuItem, "removeChapterToolStripMenuItem");
            this.removeChapterToolStripMenuItem.Click += new System.EventHandler(this.removeChapterToolStripMenuItem_Click);
            // 
            // renameSerieToolStripMenuItem
            // 
            this.renameSerieToolStripMenuItem.Name = "renameSerieToolStripMenuItem";
            resources.ApplyResources(this.renameSerieToolStripMenuItem, "renameSerieToolStripMenuItem");
            this.renameSerieToolStripMenuItem.Click += new System.EventHandler(this.renameSerieToolStripMenuItem_Click);
            // 
            // renameChapterToolStripMenuItem
            // 
            this.renameChapterToolStripMenuItem.Name = "renameChapterToolStripMenuItem";
            resources.ApplyResources(this.renameChapterToolStripMenuItem, "renameChapterToolStripMenuItem");
            this.renameChapterToolStripMenuItem.Click += new System.EventHandler(this.renameChapterToolStripMenuItem_Click);
            // 
            // changeSerieURLToolStripMenuItem
            // 
            this.changeSerieURLToolStripMenuItem.Name = "changeSerieURLToolStripMenuItem";
            resources.ApplyResources(this.changeSerieURLToolStripMenuItem, "changeSerieURLToolStripMenuItem");
            this.changeSerieURLToolStripMenuItem.Click += new System.EventHandler(this.changeSerieURLToolStripMenuItem_Click);
            // 
            // changeChapterURLToolStripMenuItem
            // 
            this.changeChapterURLToolStripMenuItem.Name = "changeChapterURLToolStripMenuItem";
            resources.ApplyResources(this.changeChapterURLToolStripMenuItem, "changeChapterURLToolStripMenuItem");
            this.changeChapterURLToolStripMenuItem.Click += new System.EventHandler(this.changeChapterURLToolStripMenuItem_Click);
            // 
            // forceBookmarksCheckToolStripMenuItem
            // 
            this.forceBookmarksCheckToolStripMenuItem.Name = "forceBookmarksCheckToolStripMenuItem";
            resources.ApplyResources(this.forceBookmarksCheckToolStripMenuItem, "forceBookmarksCheckToolStripMenuItem");
            this.forceBookmarksCheckToolStripMenuItem.Click += new System.EventHandler(this.forceBookmarksCheckToolStripMenuItem_Click);
            // 
            // clearMemoryToolStripMenuItem
            // 
            this.clearMemoryToolStripMenuItem.Name = "clearMemoryToolStripMenuItem";
            resources.ApplyResources(this.clearMemoryToolStripMenuItem, "clearMemoryToolStripMenuItem");
            this.clearMemoryToolStripMenuItem.Click += new System.EventHandler(this.clearMemoryToolStripMenuItem_Click);
            // 
            // loadAllFromCatalogToolStripMenuItem
            // 
            this.loadAllFromCatalogToolStripMenuItem.Name = "loadAllFromCatalogToolStripMenuItem";
            resources.ApplyResources(this.loadAllFromCatalogToolStripMenuItem, "loadAllFromCatalogToolStripMenuItem");
            this.loadAllFromCatalogToolStripMenuItem.Click += new System.EventHandler(this.loadAllFromCatalogToolStripMenuItem_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.trayContextMenuStrip;
            resources.ApplyResources(this.notifyIcon, "notifyIcon");
            this.notifyIcon.BalloonTipClicked += new System.EventHandler(this.notifyIcon_BalloonTipClicked);
            this.notifyIcon.BalloonTipClosed += new System.EventHandler(this.notifyIcon_BalloonTipClosed);
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            // 
            // trayContextMenuStrip
            // 
            this.trayContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitTrayToolStripMenuItem});
            this.trayContextMenuStrip.Name = "trayContextMenuStrip";
            resources.ApplyResources(this.trayContextMenuStrip, "trayContextMenuStrip");
            // 
            // exitTrayToolStripMenuItem
            // 
            this.exitTrayToolStripMenuItem.Name = "exitTrayToolStripMenuItem";
            resources.ApplyResources(this.exitTrayToolStripMenuItem, "exitTrayToolStripMenuItem");
            this.exitTrayToolStripMenuItem.Click += new System.EventHandler(this.exitTrayToolStripMenuItem_Click);
            // 
            // bookmarksTimer
            // 
            this.bookmarksTimer.Interval = 1000;
            this.bookmarksTimer.Tick += new System.EventHandler(this.bookmarksTimer_Tick);
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
            // chapterBookmarksListBox
            // 
            resources.ApplyResources(this.chapterBookmarksListBox, "chapterBookmarksListBox");
            this.chapterBookmarksListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.chapterBookmarksListBox.FormattingEnabled = true;
            this.chapterBookmarksListBox.Name = "chapterBookmarksListBox";
            this.chapterBookmarksListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.chapterBookmarksListBox.VerticalScroll += new MangaCrawler.ListBoxEx.ListBoxScrollDelegate(this.chapterBookmarksListBox_VerticalScroll);
            this.chapterBookmarksListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.chapterBookmarksListBox_DrawItem);
            this.chapterBookmarksListBox.SelectedIndexChanged += new System.EventHandler(this.chapterBookmarksListBox_SelectedIndexChanged);
            this.chapterBookmarksListBox.DoubleClick += new System.EventHandler(this.chapterBookmarksListBox_DoubleClick);
            this.chapterBookmarksListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.chapterBookmarksListBox_KeyDown);
            // 
            // serieBookmarksListBox
            // 
            resources.ApplyResources(this.serieBookmarksListBox, "serieBookmarksListBox");
            this.serieBookmarksListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.serieBookmarksListBox.FormattingEnabled = true;
            this.serieBookmarksListBox.Name = "serieBookmarksListBox";
            this.serieBookmarksListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.serieBookmarksListBox_DrawItem);
            this.serieBookmarksListBox.SelectedIndexChanged += new System.EventHandler(this.serieBookmarksListBox_SelectedIndexChanged);
            this.serieBookmarksListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.serieBookmarksListBox_KeyDown);
            // 
            // MangaCrawlerForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.debugContextMenuStrip;
            this.Controls.Add(this.versionPanel);
            this.Controls.Add(this.tabControl);
            this.Name = "MangaCrawlerForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MangaCrawlerForm_FormClosing);
            this.Load += new System.EventHandler(this.MangaShareCrawlerForm_Load);
            this.Shown += new System.EventHandler(this.MangaCrawlerForm_Shown);
            this.ResizeEnd += new System.EventHandler(this.MangaCrawlerForm_ResizeEnd);
            this.versionPanel.ResumeLayout(false);
            this.bookmarksTabPage.ResumeLayout(false);
            this.splitBookmarksPanel.ResumeLayout(false);
            this.chapterBookmarksPanel.ResumeLayout(false);
            this.chapterBookmarksPanel.PerformLayout();
            this.serieBookmarksPanel.ResumeLayout(false);
            this.serieBookmarksPanel.PerformLayout();
            this.logTabPage.ResumeLayout(false);
            this.optionsTabPage.ResumeLayout(false);
            this.optionsTabPage.PerformLayout();
            this.worksTabPage.ResumeLayout(false);
            this.worksTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.worksGridView)).EndInit();
            this.seriesTabPage.ResumeLayout(false);
            this.splitPanel.ResumeLayout(false);
            this.chaptersPanel.ResumeLayout(false);
            this.chaptersPanel.PerformLayout();
            this.seriesPanel.ResumeLayout(false);
            this.seriesPanel.PerformLayout();
            this.serversPanel.ResumeLayout(false);
            this.serversPanel.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.debugContextMenuStrip.ResumeLayout(false);
            this.trayContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Panel versionPanel;
        private System.Windows.Forms.LinkLabel versionLinkLabel;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.TabPage bookmarksTabPage;
        private System.Windows.Forms.Panel splitBookmarksPanel;
        private System.Windows.Forms.Splitter splitterBookmarks;
        private System.Windows.Forms.Panel chapterBookmarksPanel;
        private System.Windows.Forms.Button viewChapterBoksButton;
        private System.Windows.Forms.Button openChapterFolderBooksButton;
        private System.Windows.Forms.Button visitChapterBooksButton;
        private System.Windows.Forms.Button downloadChapterBooksButton;
        private ListBoxEx chapterBookmarksListBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel serieBookmarksPanel;
        private System.Windows.Forms.Button removeSerieBooksPanel;
        private System.Windows.Forms.Button openSerieFolderBooksButton;
        private System.Windows.Forms.Button visitSerieBooksButton;
        private System.Windows.Forms.Label label8;
        private ListBoxEx serieBookmarksListBox;
        private System.Windows.Forms.TabPage logTabPage;
        private System.Windows.Forms.Button clearLogButton;
        private System.Windows.Forms.RichTextBox logRichTextBox;
        private System.Windows.Forms.TabPage optionsTabPage;
        private System.Windows.Forms.Label optionslLabel;
        private System.Windows.Forms.ComboBox pageNamingStrategyComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbzCheckBox;
        private System.Windows.Forms.Button mangaRootDirChooseButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox mangaRootDirTextBox;
        private System.Windows.Forms.TabPage worksTabPage;
        private System.Windows.Forms.Button openFolderWorksButton;
        private System.Windows.Forms.Button downloadWorkButton;
        private System.Windows.Forms.Button cancelWorkButton;
        private System.Windows.Forms.Button visitPageWorkButton;
        private System.Windows.Forms.Button viewWorkButton;
        private System.Windows.Forms.Button goToSeriesTabButton;
        private System.Windows.Forms.DataGridView worksGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Chapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn Progress;
        private System.Windows.Forms.TabPage seriesTabPage;
        private System.Windows.Forms.Panel splitPanel;
        private System.Windows.Forms.Splitter splitter;
        private System.Windows.Forms.Panel chaptersPanel;
        private System.Windows.Forms.Button viewPagesButton;
        private System.Windows.Forms.Button openPagesFolder;
        private System.Windows.Forms.Button chapterURLButton;
        private System.Windows.Forms.Button downloadButton;
        private ListBoxEx chaptersListBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel seriesPanel;
        private System.Windows.Forms.TextBox seriesSearchTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button openSeriesFolderButton;
        private System.Windows.Forms.Button seriesURLButton;
        private System.Windows.Forms.Label label4;
        private ListBoxEx seriesListBox;
        private System.Windows.Forms.Panel serversPanel;
        private System.Windows.Forms.Button openServerFolderButton;
        private ListBoxEx serversListBox;
        private System.Windows.Forms.Button serverURLButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Button BookmarkSerieButton;
        private System.Windows.Forms.ContextMenuStrip debugContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem resetCheckDatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSerieFirsttoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSerieMiddleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSerieLastToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeSerieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addChapterFirstToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addChapterMiddleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addChapterLastToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeChapterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameSerieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameChapterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeSerieURLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeChapterURLToolStripMenuItem;
        private System.Windows.Forms.CheckBox playSoundWhenDownloadedCheckBox;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.CheckBox minimizeOnCloseCheckBox;
        private System.Windows.Forms.ContextMenuStrip trayContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exitTrayToolStripMenuItem;
        private System.Windows.Forms.CheckBox showBaloonTipsCheckBox;
        private System.Windows.Forms.Timer bookmarksTimer;
        private System.Windows.Forms.Button checkNowBookmarksButton;
        private System.Windows.Forms.ToolStripMenuItem forceBookmarksCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAllFromCatalogToolStripMenuItem;
        private System.Windows.Forms.Button clearWorkButton;
        private System.Windows.Forms.Button checkNowSerieButton;
        private System.Windows.Forms.Button checkNowServerButton;
    }
}

