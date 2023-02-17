using MiniEngine.Tools.AssetsViewer.Properties;
using MiniEngine.Tools.Compression;

namespace MiniEngine.Tools.AssetsViewer
{
    public partial class Viewer : Form
    {
        private TreeData _data;
        private TreeNode? _selectedNode;
        private string? _selectedPack;
        private string? _selectedFile;
        private string _relativePath = string.Empty;

        public Viewer()
        {
            InitializeComponent();
            var imageList = new ImageList();
            imageList.Images.Add(Resources.FileIcon);
            imageList.Images.Add(Resources.FolderIcon);
            ListView_Content.LargeImageList = imageList;
            TreeView_Browser.ImageList = imageList;
            ResetView();
        }

        private void MenuStripButton_OpenFile_Click(object sender, EventArgs e)
        {
            var result = OpenFileDialog_SelectAssetsFile.ShowDialog();
            if (result != DialogResult.OK)
                return;
            _selectedPack = OpenFileDialog_SelectAssetsFile.FileName;
            var files = PackProber.Probe(_selectedPack).ToArray();
            ResetView();
            foreach (var file in files)
            {
                _data.Register(file);
            }

            ToolStripLabel_Status.Text = $"Loaded asset pack. ({files.Length} files(s))";
            TextBox_CurrentLocation.Text = _selectedPack;
            MenuStripButton_ExtractAllFiles.Enabled = true;
            UpdateView();
        }

        private void ResetView()
        {
            _data = new TreeData();

            TreeView_Browser.BeginUpdate();
            TreeView_Browser.Nodes.Clear();
            TreeView_Browser.EndUpdate();

            ListView_Content.BeginUpdate();
            ListView_Content.Clear();
            ListView_Content.EndUpdate();
        }

        public void UpdateView()
        {
            var rootNode = new TreeNode(".", 1, 1);
            TreeView_Browser.BeginUpdate();
            TreeView_Browser.Nodes.Add(rootNode);
            UpdateByNode(rootNode, _data.Root);
            TreeView_Browser.SelectedNode = rootNode;
            TreeView_Browser.EndUpdate();

            UpdateWithNode(_data.Root);
        }

        private void UpdateWithNode(TreeDataNode node)
        {
            ListView_Content.BeginUpdate();
            ListView_Content.Clear();
            foreach (var item in node.Files)
            {
                ListView_Content.Items.Add(new ListViewItem(item.Name, 0));
            }

            ListView_Content.EndUpdate();
        }

        private static void UpdateByNode(TreeNode node, TreeDataNode data)
        {
            foreach (var childNode in data.Children)
            {
                var newNode = new TreeNode(childNode.Metadata, 1, 1);
                node.Nodes.Add(newNode);
                UpdateByNode(newNode, childNode);
            }
        }

        private void TreeView_Browser_Select(object sender, TreeViewEventArgs e)
        {
            var selectedNode = TreeView_Browser.SelectedNode;
            if (selectedNode == _selectedNode)
                return;
            if (selectedNode != null)
            {
                var fullPath = selectedNode.FullPath == "." ? string.Empty : selectedNode.FullPath.Remove(0, 2);
                _relativePath = fullPath;
                TextBox_CurrentLocation.Text = _selectedPack + "\\" + fullPath;
                var dataNode = _data.Root.Get(fullPath);
                UpdateWithNode(dataNode);
            }

            _selectedNode = selectedNode;
        }

        private void MenuStripButton_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ListView_Content_ItemActivate(object sender, EventArgs e)
        {
            ExtractCurrentlySelectedFile();
        }

        private void ListView_Content_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (ListView_Content.SelectedItems.Count == 0)
            {
                _selectedFile = null;
                MenuStripButton_ExtractFile.Enabled = false;
                return;
            }

            MenuStripButton_ExtractFile.Enabled = true;
            var relativePath = _relativePath != string.Empty ? _relativePath + "\\" : string.Empty;
            _selectedFile = relativePath + e.Item.Text;
        }

        private void ExtractCurrentlySelectedFile()
        {
            var dirPath = Path.GetDirectoryName(_selectedFile) ?? string.Empty;
            var fileName = Path.GetFileName(_selectedFile)!;
            var dataNode = _data.Root.Get(dirPath);
            var fileOffset = dataNode.Files.First(offset => offset.Name == fileName);
            SaveFileDialog_ExtractFile.FileName = fileOffset.Name;
            var result = SaveFileDialog_ExtractFile.ShowDialog();
            if (result != DialogResult.OK || _selectedPack == null)
                return;

            var file = SaveFileDialog_ExtractFile.OpenFile();
            var pack = File.OpenRead(_selectedPack);
            Compression.Compression.DecompressTo(
                pack, fileOffset.Offset, fileOffset.UncompressedSize,
                file);
            file.Close();
            pack.Close();

            ToolStripLabel_Status.Text = "Extracted 1 file.";
        }

        private void ExtractAllFiles(string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
            {
                MessageBox.Show("Directory not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ExtractNode(destinationFolder, _data.Root);
            ToolStripLabel_Status.Text = "Extracted all files.";
        }

        private void ExtractNode(string parentDir, TreeDataNode node)
        {
            if (_selectedPack is null)
                return;
            var nodePath = node.Metadata == "." ? string.Empty : node.Metadata;
            var folderPath = Path.Join(parentDir, nodePath);

            Directory.CreateDirectory(folderPath);
            var sourceStream = File.OpenRead(_selectedPack);
            foreach (var file in node.Files)
            {
                var filePath = Path.Join(folderPath, file.Name);
                var destinationStream = File.OpenWrite(filePath);
                Compression.Compression.DecompressTo(
                    sourceStream, file.Offset, file.UncompressedSize,
                    destinationStream);
                destinationStream.Close();
            }
            sourceStream.Close();
            foreach (var subNode in node.Children)
                ExtractNode(folderPath, subNode);
        }

        private void MenuStripButton_ExtractFile_Click(object sender, EventArgs e)
        {
            ExtractCurrentlySelectedFile();
        }

        private void MenuStripButton_ExtractAllFiles_Click(object sender, EventArgs e)
        {
            var result = FolderBrowserDialog_ExtractAll.ShowDialog();
            if (result == DialogResult.OK)
            {
                ExtractAllFiles(FolderBrowserDialog_ExtractAll.SelectedPath);
            }
        }
    }
}