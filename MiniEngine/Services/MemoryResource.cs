using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Utility;
using static SDL2.SDL;

namespace MiniEngine
{
    public class MemoryResource : IDisposable
    {
        private GCHandle _handle;
        public IntPtr RwHandle { get; private set; }
        public bool Disposed { get; private set; }

        public MemoryResource(IReadOnlyCollection<byte> data)
        {
            _handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = _handle.AddrOfPinnedObject();
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
