using MiniEngine.Tools.Compression;
using MiniEngine.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiniEngine
{
    public static class Resources
    {
        private class Pack
        {
            private readonly Dictionary<string, MemoryResource> _resourceCache = new();
            private readonly Dictionary<string, FileOffset> _offsets = new();
            public IDictionary<string, MemoryResource> ResourceCache => _resourceCache;
            public IDictionary<string, FileOffset> Offsets => _offsets;
            public FileStream? PackStream { get; set; }

            public MemoryResource? GetInternalResource(string path)
            {
                path = path.ToLower().Replace('\\', '/');
                if (path.StartsWith("./"))
                    path = path[2..];
                if (path.StartsWith("."))
                    path = path[1..];

                if (_resourceCache.TryGetValue(path, out var resource))
                    return resource;

                if (!_offsets.TryGetValue(path, out var offset))
                    return null;

                if (PackStream == null)
                    return null;

                using MemoryStream stream = new();
                PackStream.Seek(offset.Offset, SeekOrigin.Begin);
                Compression.DecompressTo(PackStream, 0, offset.UncompressedSize, stream);
                var memoryResource = new MemoryResource(stream.ToArray());
                _resourceCache.Add(path, memoryResource);

                return memoryResource;
            }
        }

        private static readonly Dictionary<string, Pack> _packs = new();
        private static readonly SortedList<int, string> _packPaths = new(new DescendingComparer<int>());
        //private static FileStream? _packStream;

        public static void UsePack(string path, int priority = 0)
        {
            if (!File.Exists(path))
            {
                LoggingService.Warn("Asset pack file \"{0}\" does not exist.", path);
                return;
            }
            var offsets = PackProber.Probe(path);
            var pack = new Pack();
            foreach (var offset in offsets)
            {
                var relativePath = offset.RelativePath.Replace('\\', '/');
                pack.Offsets.Add(relativePath, offset);
            }
            pack.PackStream = File.OpenRead(path);
            var packPath = pack.PackStream.Name;
            var fileInfo = new FileInfo(packPath);
            _packs.Add(packPath, pack);
            _packPaths.Add(priority, packPath);
            LoggingService.Info("Opened '{0}' pack file.", fileInfo.Name);
        }

        public static void ClosePack(string path)
        {
            var absolutePath = Path.GetFullPath(path);
            if (_packs.TryGetValue(absolutePath, out var pack))
            {
                pack.Offsets.Clear();
                foreach (var (_, resource) in pack.ResourceCache)
                {
                    resource.Dispose();
                }

                pack.ResourceCache.Clear();

                pack.PackStream?.Close();
                _packs.Remove(absolutePath);
                var pathIndex = _packPaths.IndexOfValue(absolutePath);
                if (pathIndex != -1)
                    _packPaths.RemoveAt(pathIndex);
                var fileInfo = new FileInfo(absolutePath);
                LoggingService.Info("Closed '{0}' pack file.", fileInfo.Name);
            }
        }

        public static void ClosePacks()
        {
            var packsSnapshot = _packs.Keys.ToArray();
            foreach (var absolutePath in packsSnapshot)
            {
                ClosePack(absolutePath);
            }
        }

        public static MemoryResource? GetResource(string path)
        {
            foreach (var (_, absolutePath) in _packPaths)
            {
                var pack = _packs[absolutePath];
                var memoryResource = pack.GetInternalResource(path);
                if (memoryResource != null)
                    return memoryResource;
            }

            return null;
        }
    }
}
