using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static SDL2.SDL;

namespace MiniEngine
{
    public class MemoryResource : IDisposable
    {
        private GCHandle _handle;
        private readonly int _size;

        public int Size => Disposed ? 0 : _size;
        public IntPtr RwHandle { get; private set; }
        public bool Disposed { get; private set; }

        public MemoryResource(IReadOnlyCollection<byte> data)
        {
            _handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = _handle.AddrOfPinnedObject();
            _size = data.Count;
            RwHandle = SDL_RWFromMem(dataPtr, data.Count);
        }

        public void Dispose()
        {
            SDL_RWclose(RwHandle);
            RwHandle = IntPtr.Zero;
            _handle.Free();
            GC.SuppressFinalize(this);
            Disposed = true;
        }

        public string ReadAsText(Encoding? encoding = null)
        {
            if (_handle.Target is not IReadOnlyCollection<byte> data)
                return "";
            var encoder = encoding ?? Encoding.UTF8;
            return encoder.GetString(data.ToArray());
        }

        public Stream CreateStream()
        {
            return _handle.Target is not IReadOnlyCollection<byte> data
                ? Stream.Null
                : new MemoryStream(data.ToArray());
        }

        public override string ToString()
        {
            return Resources.GetFriendlyName(this);
        }

        public static MemoryResource FromStream(MemoryStream stream)
        {
            var data = stream.ToArray();
            return new MemoryResource(data);
        }

        public static implicit operator MemoryResource?(string resourcePath)
        {
            var resource = Resources.GetResource(resourcePath);
            if (resource == null)
            {
                LoggingService.Error("Resource '{0}' not found.", resourcePath);
            }

            return resource;
        }
    }
}
