using MiniEngine.Tools.Compression;
using MiniEngine.Utility;
using System.Collections.Generic;
using System.IO;

namespace MiniEngine
{
    public static class Resources
    {
        private static readonly Dictionary<string, MemoryResource> _resourceCache = new();
        private static readonly Dictionary<string, FileOffset> _offsets = new();
        private static FileStream? _packStream;

        public static void UsePack(string path)
        {
            if (!File.Exists(path))
            {
                LoggingService.Warn("Asset pack file \"{0}\" does not exist.", path);
                return;
            }
            ClosePack();
            var offsets = PackProber.Probe(path);
            foreach (var offset in offsets)
            {
                var relativePath = offset.RelativePath.Replace('\\', '/');
                _offsets.Add(relativePath, offset);
            }
            _packStream = File.OpenRead(path);
        }

        public static void ClosePack()
        {
            _offsets.Clear();
            foreach (var (_, resource) in _resourceCache)
            {
                resource.Dispose();
            }
            _resourceCache.Clear();

            _packStream?.Close();
        }

        public static MemoryResource? GetResource(string path)
        {
            if (_packStream == null)
                return null;
            path = path.ToLower().Replace('\\', '/');
            if (path.StartsWith("./"))
                path = path[2..];
            if (path.StartsWith("."))
                path = path[1..];

            if (_resourceCache.TryGetValue(path, out var resource))
                return resource;

            if (!_offsets.TryGetValue(path, out var offset))
                return null;

            using MemoryStream stream = new();
            _packStream.Seek(offset.Offset, SeekOrigin.Begin);
            Compression.DecompressTo(_packStream, 0, offset.UncompressedSize, stream);
            var memoryResource = new MemoryResource(stream.ToArray());
            _resourceCache.Add(path, memoryResource);

            return memoryResource;
        }
    }
}
