namespace MiniEngine.Tools.AssetsViewer
{
    partial class Viewer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("audio");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("textures");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode(".", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Item 1", 0);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Item 2", 0);
            this.TreeView_Browser = new System.Windows.Forms.TreeView();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuStripButton_OpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuStripButton_ExtractFile = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuStripButton_ExtractAllFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuStripButton_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.ToolStripLabel_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.Panel_Content = new System.Windows.Forms.Panel();
            this.ListView_Content = new System.Windows.Forms.ListView();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.OpenFileDialog_SelectAssetsFile = new System.Windows.Forms.OpenFileDialog();
            this.TextBox_CurrentLocation = new System.Windows.Forms.TextBox();
            this.SaveFileDialog_ExtractFile = new System.Windows.Forms.SaveFileDialog();
            this.FolderBrowserDialog_ExtractAll = new System.Windows.Forms.FolderBrowserDialog();
            this.MenuStrip.SuspendLayout();
            this.StatusStrip.SuspendLayout();
            this.Panel_Content.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeView_Browser
            // 
            this.TreeView_Browser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TreeView_Browser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TreeView_Browser.Location = new System.Drawing.Point(0, 0);
            this.TreeView_Browser.Name = "TreeView_Browser";
            treeNode1.Name = "Node1";
            treeNode1.Text = "audio";
            treeNode2.Name = "Node2";
            treeNode2.Text = "textures";
            treeNode3.Checked = true;
            treeNode3.Name = "RootNode";
            treeNode3.Text = ".";
            this.TreeView_Browser.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3});
            this.TreeView_Browser.Size = new System.Drawing.Size(200, 382);
            this.TreeView_Browser.TabIndex = 0;
            this.TreeView_Browser.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_Browser_Select);
            // 
            // MenuStrip
            // 
            this.MenuStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Padding = new System.Windows.Forms.Padding(0);
            this.MenuStrip.Size = new System.Drawing.Size(800, 24);
            this.MenuStrip.TabIndex = 1;
            this.MenuStrip.Text = "MenuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuStripButton_OpenFile,
            this.MenuStripButton_ExtractFile,
            this.MenuStripButton_ExtractAllFiles,
            this.MenuStripButton_Exit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // MenuStripButton_OpenFile
            // 
            this.MenuStripButton_OpenFile.Name = "MenuStripButton_OpenFile";
            this.MenuStripButton_OpenFile.Size = new System.Drawing.Size(156, 22);
            this.MenuStripButton_OpenFile.Text = "Open assets file";
            this.MenuStripButton_OpenFile.Click += new System.EventHandler(this.MenuStripButton_OpenFile_Click);
            // 
            // MenuStripButton_ExtractFile
            // 
            this.MenuStripButton_ExtractFile.Enabled = false;
            this.MenuStripButton_ExtractFile.Name = "MenuStripButton_ExtractFile";
            this.MenuStripButton_ExtractFile.Size = new System.Drawing.Size(156, 22);
            this.MenuStripButton_ExtractFile.Text = "Extract file";
            this.MenuStripButton_ExtractFile.Click += new System.EventHandler(this.MenuStripButton_ExtractFile_Click);
            // 
            // MenuStripButton_ExtractAllFiles
            // 
            this.MenuStripButton_ExtractAllFiles.Enabled = false;
            this.MenuStripButton_ExtractAllFiles.Name = "MenuStripButton_ExtractAllFiles";
            this.MenuStripButton_ExtractAllFiles.Size = new System.Drawing.Size(156, 22);
            this.MenuStripButton_ExtractAllFiles.Text = "Extract all files";
            this.MenuStripButton_ExtractAllFiles.Click += new System.EventHandler(this.MenuStripButton_ExtractAllFiles_Click);
            // 
            // MenuStripButton_Exit
            // 
            this.MenuStripButton_Exit.Name = "MenuStripButton_Exit";
            this.MenuStripButton_Exit.Size = new System.Drawing.Size(156, 22);
            this.MenuStripButton_Exit.Text = "Exit";
            this.MenuStripButton_Exit.Click += new System.EventHandler(this.MenuStripButton_Exit_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripLabel_Status});
            this.StatusStrip.Location = new System.Drawing.Point(0, 428);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StatusStrip.Size = new System.Drawing.Size(800, 22);
            this.StatusStrip.TabIndex = 2;
            this.StatusStrip.Text = "StatusStrip";
            // 
            // ToolStripLabel_Status
            // 
            this.ToolStripLabel_Status.Name = "ToolStripLabel_Status";
            this.ToolStripLabel_Status.Size = new System.Drawing.Size(84, 17);
            this.ToolStripLabel_Status.Text = "No file loaded.";
            // 
            // Panel_Content
            // 
            this.Panel_Content.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel_Content.BackColor = System.Drawing.SystemColors.Window;
            this.Panel_Content.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Panel_Content.Controls.Add(this.ListView_Content);
            this.Panel_Content.Location = new System.Drawing.Point(0, 0);
            this.Panel_Content.Name = "Panel_Content";
            this.Panel_Content.Size = new System.Drawing.Size(596, 382);
            this.Panel_Content.TabIndex = 3;
            // 
            // ListView_Content
            // 
            this.ListView_Content.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.ListView_Content.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListView_Content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListView_Content.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ListView_Content.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2});
            this.ListView_Content.Location = new System.Drawing.Point(0, 0);
            this.ListView_Content.Name = "ListView_Content";
            this.ListView_Content.Size = new System.Drawing.Size(594, 380);
            this.ListView_Content.TabIndex = 0;
            this.ListView_Content.TileSize = new System.Drawing.Size(64, 64);
            this.ListView_Content.UseCompatibleStateImageBehavior = false;
            this.ListView_Content.ItemActivate += new System.EventHandler(this.ListView_Content_ItemActivate);
            this.ListView_Content.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ListView_Content_ItemSelectionChanged);
            // 
            // SplitContainer
            // 
            this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SplitContainer.Location = new System.Drawing.Point(0, 46);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.TreeView_Browser);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.Panel_Content);
            this.SplitContainer.Size = new System.Drawing.Size(800, 382);
            this.SplitContainer.SplitterDistance = 200;
            this.SplitContainer.TabIndex = 4;
            // 
            // OpenFileDialog_SelectAssetsFile
            // 
            this.OpenFileDialog_SelectAssetsFile.DefaultExt = "mea";
            this.OpenFileDialog_SelectAssetsFile.FileName = "assets.mea";
            this.OpenFileDialog_SelectAssetsFile.Filter = "MiniEngine Assets File|*.mea";
            this.OpenFileDialog_SelectAssetsFile.Title = "Select Assets File";
            // 
            // TextBox_CurrentLocation
            // 
            this.TextBox_CurrentLocation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBox_CurrentLocation.Location = new System.Drawing.Point(0, 24);
            this.TextBox_CurrentLocation.Name = "TextBox_CurrentLocation";
            this.TextBox_CurrentLocation.ReadOnly = true;
            this.TextBox_CurrentLocation.Size = new System.Drawing.Size(800, 23);
            this.TextBox_CurrentLocation.TabIndex = 5;
            // 
            // SaveFileDialog_ExtractFile
            // 
            this.SaveFileDialog_ExtractFile.Title = "Extract File";
            // 
            // Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TextBox_CurrentLocation);
            this.Controls.Add(this.SplitContainer);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.MenuStrip);
            this.MainMenuStrip = this.MenuStrip;
            this.Name = "Viewer";
            this.Text = "MiniEngine - Assets Viewer";
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.Panel_Content.ResumeLayout(false);
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeView TreeView_Browser;
        private MenuStrip MenuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private StatusStrip StatusStrip;
        private Panel Panel_Content;
        private ToolStripStatusLabel ToolStripLabel_Status;
        private ToolStripMenuItem MenuStripButton_OpenFile;
        private ToolStripMenuItem MenuStripButton_ExtractFile;
        private ToolStripMenuItem MenuStripButton_ExtractAllFiles;
        private ToolStripMenuItem MenuStripButton_Exit;
        private SplitContainer SplitContainer;
        private OpenFileDialog OpenFileDialog_SelectAssetsFile;
        private ListView ListView_Content;
        private TextBox TextBox_CurrentLocation;
        private SaveFileDialog SaveFileDialog_ExtractFile;
        private FolderBrowserDialog FolderBrowserDialog_ExtractAll;
    }
}