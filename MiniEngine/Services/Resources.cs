using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Tools.Compression;
using MiniEngine.Utility;

namespace MiniEngine
{
    public static class Resources
    {
        public class Resource : IDisposable
        {
            public MemoryResource MemoryResource { get; set; }
            public int AccessCount { get; private set; } = 1;

            public Resource(MemoryResource memoryResource)
            {
                MemoryResource = memoryResource;
            }

            public void Dispose()
            {
                MemoryResource.Dispose();
            }

            public void IncrementAccessCount()
                => AccessCount++;
        }

        private static readonly Dictionary<string, Resource> _resourceCache = new();
        private static FileStream? _packStream;
        private static IEnumerable<FileOffset> _offsets = Array.Empty<FileOffset>();

        public static void UsePack(string path)
        {
            if (!File.Exists(path))
            {
                LoggingService.Warn("Asset pack file \"{0}\" does not exist.", path);
                return;
            }
            ClosePack();
            _offsets = PackProber.Probe(path);
            _packStream = File.OpenRead(path);
        }

        public static void ClosePack()
        {
            _offsets = Array.Empty<FileOffset>();
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
            {
                resource.IncrementAccessCount();
                return resource.MemoryResource;
            }

            var offset = _offsets.FirstOrDefault(offset => offset?.RelativePath.Replace('\\', '/') == path, null);
            if (offset == null)
                return null;

            using MemoryStream stream = new();
            Compression.DecompressTo(_packStream, offset.Offset, offset.UncompressedSize, stream);
            var memoryResource = new MemoryResource(stream.ToArray());
            resource = new Resource(memoryResource);
            _resourceCache.Add(path, resource);

            return resource?.MemoryResource;
        }
    }
}
