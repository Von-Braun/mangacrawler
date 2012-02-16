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
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel3 = new System.Windows.Forms.Panel();
            this.chapterURLButton = new System.Windows.Forms.Button();
            this.downloadButton = new System.Windows.Forms.Button();
            this.chaptersListBox = new MangaCrawlerControls.ListBoxEx();
            this.label7 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.seriesSearchTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.panel8 = new System.Windows.Forms.Panel();
            this.seriesURLButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.seriesListBox = new MangaCrawlerControls.ListBoxEx();
            this.cbzCheckBox = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.serversListBox = new MangaCrawlerControls.ListBoxEx();
            this.serverURLButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.mangaRootDirChooseButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.mangaRootDirTextBox = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tasksGridView = new System.Windows.Forms.DataGridView();
            this.Delete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Chapter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Progress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.logTabPage = new System.Windows.Forms.TabPage();
            this.clearLogButton = new System.Windows.Forms.Button();
            this.logRichTextBox = new System.Windows.Forms.RichTextBox();
            this.versionPanel = new System.Windows.Forms.Panel();
            this.versionLinkLabel = new System.Windows.Forms.LinkLabel();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tasksGridView)).BeginInit();
            this.logTabPage.SuspendLayout();
            this.versionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.logTabPage);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel4);
            this.tabPage1.Controls.Add(this.cbzCheckBox);
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Controls.Add(this.mangaRootDirChooseButton);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.mangaRootDirTextBox);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Controls.Add(this.splitter1);
            this.panel4.Controls.Add(this.panel3);
            this.panel4.Controls.Add(this.panel2);
            this.panel4.Name = "panel4";
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.Menu;
            resources.ApplyResources(this.splitter1, "splitter1");
            this.splitter1.Name = "splitter1";
            this.splitter1.TabStop = false;
            this.splitter1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter1_SplitterMoved);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.chapterURLButton);
            this.panel3.Controls.Add(this.downloadButton);
            this.panel3.Controls.Add(this.chaptersListBox);
            this.panel3.Controls.Add(this.label7);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.MinimumSize = new System.Drawing.Size(177, 0);
            this.panel3.Name = "panel3";
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
            this.chaptersListBox.MinimumSize = new System.Drawing.Size(164, 4);
            this.chaptersListBox.Name = "chaptersListBox";
            this.chaptersListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.chaptersListBox.VerticalScroll += new MangaCrawlerControls.ListBoxScroll.ListBoxScrollDelegate(this.chaptersListBox_VerticalScroll);
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
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel4);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.seriesListBox);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.MinimumSize = new System.Drawing.Size(199, 0);
            this.panel2.Name = "panel2";
            // 
            // tableLayoutPanel4
            // 
            resources.ApplyResources(this.tableLayoutPanel4, "tableLayoutPanel4");
            this.tableLayoutPanel4.Controls.Add(this.panel7, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.panel8, 1, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            // 
            // panel7
            // 
            resources.ApplyResources(this.panel7, "panel7");
            this.panel7.Controls.Add(this.seriesSearchTextBox);
            this.panel7.Controls.Add(this.label6);
            this.panel7.Name = "panel7";
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
            // panel8
            // 
            resources.ApplyResources(this.panel8, "panel8");
            this.panel8.Controls.Add(this.seriesURLButton);
            this.panel8.Name = "panel8";
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
            this.seriesListBox.VerticalScroll += new MangaCrawlerControls.ListBoxScroll.ListBoxScrollDelegate(this.seriesListBox_VerticalScroll);
            this.seriesListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.seriesListBox_DrawItem);
            this.seriesListBox.SelectedIndexChanged += new System.EventHandler(this.seriesListBox_SelectedIndexChanged);
            // 
            // cbzCheckBox
            // 
            resources.ApplyResources(this.cbzCheckBox, "cbzCheckBox");
            this.cbzCheckBox.Name = "cbzCheckBox";
            this.cbzCheckBox.UseVisualStyleBackColor = true;
            this.cbzCheckBox.CheckedChanged += new System.EventHandler(this.cbzCheckBox_CheckedChanged);
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.serversListBox);
            this.panel1.Controls.Add(this.serverURLButton);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Name = "panel1";
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
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tasksGridView);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tasksGridView
            // 
            this.tasksGridView.AllowUserToAddRows = false;
            this.tasksGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.tasksGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.tasksGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.tasksGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.tasksGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tasksGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.tasksGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tasksGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Delete,
            this.Chapter,
            this.Progress});
            resources.ApplyResources(this.tasksGridView, "tasksGridView");
            this.tasksGridView.MultiSelect = false;
            this.tasksGridView.Name = "tasksGridView";
            this.tasksGridView.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tasksGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.tasksGridView.RowHeadersVisible = false;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Comic Sans MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.tasksGridView.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.tasksGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tasksGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.tasksGridView_CellContentClick);
            // 
            // Delete
            // 
            this.Delete.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            resources.ApplyResources(this.Delete, "Delete");
            this.Delete.Name = "Delete";
            this.Delete.ReadOnly = true;
            this.Delete.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Delete.Text = "Usuń";
            this.Delete.UseColumnTextForButtonValue = true;
            // 
            // Chapter
            // 
            this.Chapter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Chapter.DataPropertyName = "TaskTitle";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Chapter.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.Chapter, "Chapter");
            this.Chapter.Name = "Chapter";
            this.Chapter.ReadOnly = true;
            // 
            // Progress
            // 
            this.Progress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Progress.DataPropertyName = "TaskProgress";
            resources.ApplyResources(this.Progress, "Progress");
            this.Progress.Name = "Progress";
            this.Progress.ReadOnly = true;
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
            this.ResizeEnd += new System.EventHandler(this.MangaCrawlerForm_ResizeEnd);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tasksGridView)).EndInit();
            this.logTabPage.ResumeLayout(false);
            this.versionPanel.ResumeLayout(false);
            this.versionPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox mangaRootDirTextBox;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button mangaRootDirChooseButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button serverURLButton;
        private System.Windows.Forms.Label label5;
        private MangaCrawlerControls.ListBoxEx serversListBox;
        private System.Windows.Forms.DataGridView tasksGridView;
        private System.Windows.Forms.CheckBox cbzCheckBox;
        private System.Windows.Forms.Panel versionPanel;
        private System.Windows.Forms.LinkLabel versionLinkLabel;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button chapterURLButton;
        private System.Windows.Forms.Button downloadButton;
        private MangaCrawlerControls.ListBoxEx chaptersListBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.TextBox seriesSearchTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Button seriesURLButton;
        private System.Windows.Forms.Label label4;
        private MangaCrawlerControls.ListBoxEx seriesListBox;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.DataGridViewButtonColumn Delete;
        private System.Windows.Forms.DataGridViewTextBoxColumn Chapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn Progress;
        private System.Windows.Forms.TabPage logTabPage;
        private System.Windows.Forms.Button clearLogButton;
        private System.Windows.Forms.RichTextBox logRichTextBox;
    }
}

