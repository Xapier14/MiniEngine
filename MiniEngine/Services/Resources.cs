using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Tools.Compression;
using MiniEngine.Utility;

using static SDL2.SDL;
using static SDL2.SDL_image;

namespace MiniEngine
{
    public static class Resources
    {
        private class Resource : IDisposable
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
        
        private static readonly Dictionary<MemoryResource, IntPtr> _textureCache = new();
        private static readonly Dictionary<string, Resource> _resourceCache = new();
        private static FileStream? _packStream;
        private static IEnumerable<FileOffset> _offsets = Array.Empty<FileOffset>();
        private static IntPtr? _missingTexture;

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

        internal static IntPtr GetTexture(MemoryResource? textureResource)
        {
            if (Graphics.RendererPtr == null)
                throw new NoWindowInstanceException();
            if (!_missingTexture.HasValue)
            {
                var missingTexture = SDL_CreateTexture(Graphics.RendererPtr.Value, SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC, 2, 2);
                var perPixelSize = SDL_BYTESPERPIXEL(SDL_PIXELFORMAT_RGBA8888);
                var pixelData = new byte[]
                {
                    255, 200, 0, 255, 255, 0, 0, 0,
                    255, 0, 0, 0, 255, 200, 0, 255
                };
                var handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);

                var result = SDL_UpdateTexture(missingTexture, IntPtr.Zero, handle.AddrOfPinnedObject(), perPixelSize * 2);
                if (result != 0)
                {
                    LoggingService.Fatal("Error creating missing texture. {0}", SDL_GetError());
                    GameEngine.FatalExit(-1);
                }
                handle.Free();
                _missingTexture = missingTexture;
            }

            if (textureResource is null)
                return _missingTexture.Value;

            if (_textureCache.TryGetValue(textureResource, out var texture))
                return texture;

            var surface = IMG_Load_RW(textureResource.RwHandle, 0);
            if (surface == IntPtr.Zero)
            {
                LoggingService.Fatal(SDL_GetError());
                GameEngine.FatalExit(-200);
            }
            texture = SDL_CreateTextureFromSurface(Graphics.RendererPtr.Value, surface);
            if (texture == IntPtr.Zero)
            {
                LoggingService.Fatal(SDL_GetError());
                GameEngine.FatalExit(-201);
            }

            _textureCache.Add(textureResource, texture);

            return texture;
        }
    }
}
