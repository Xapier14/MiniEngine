using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Tools.Compression;

namespace MiniEngine.Tools.AssetsViewer
{
    internal class TreeDataNode
    {
        private readonly List<TreeDataNode> _children = new();
        private readonly List<FileOffset> _files = new();

        public IReadOnlyList<TreeDataNode> Children => _children;
        public TreeDataNode? Parent { get; }
        public string Metadata { get; }
        public IList<FileOffset> Files => _files;

        public TreeDataNode(string metadata, TreeDataNode? parent = null)
        {
            Metadata = metadata;
            Parent = parent;
        }

        public TreeDataNode Get(string path)
        {
            if (path == string.Empty)
                return this;
            var dirPath = path.Split('\\');
            var currentNode = this;
            foreach (var dirPart in dirPath)
            {
                var checkNode = currentNode.Children.FirstOrDefault(node => node?.Metadata == dirPart, null);
                if (checkNode is null)
                {
                    checkNode = new TreeDataNode(dirPart, currentNode);
                    currentNode.AddChild(checkNode);
                }
                currentNode = checkNode;
            }
            return currentNode;
        }

        internal void AddChild(TreeDataNode node)
            => _children.Add(node);
    }
    internal class TreeData
    {
        public TreeDataNode Root { get; private set; } = new(".");

        public void Clear()
        {
            Root = new TreeDataNode(".");
        }

        public void Register(FileOffset offset)
        {
            var dirPath = Path.GetDirectoryName(offset.RelativePath) ?? string.Empty;
            var dirNode = Root.Get(dirPath);
            dirNode.Files.Add(offset);
        }
    }
}
