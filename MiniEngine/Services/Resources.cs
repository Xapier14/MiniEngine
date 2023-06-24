using MiniEngine.Tools.Compression;
using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace MiniEngine
{
    public static class Resources
    {
        private static readonly Dictionary<MemoryResource, IntPtr> _textureCache = new();
        private static readonly Dictionary<string, MemoryResource> _resourceCache = new();
        private static readonly Dictionary<string, FileOffset> _offsets = new();
        private static FileStream? _packStream;
        private static IntPtr? _missingTexture;

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
