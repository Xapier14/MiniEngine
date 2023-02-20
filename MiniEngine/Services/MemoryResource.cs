using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace MiniEngine
{
    public class MemoryResource : IDisposable
    {
        private GCHandle _handle;
        private IntPtr _rwops;
        public IntPtr RWHandle => _rwops;
        public bool Disposed { get; private set; }

        public MemoryResource(byte[] data)
        {
            _handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = _handle.AddrOfPinnedObject();
            _rwops = SDL_RWFromMem(dataPtr, data.Length);
        }

        public void Dispose()
        {
            SDL_RWclose(_rwops);
            _rwops = IntPtr.Zero;
            _handle.Free();
            GC.SuppressFinalize(this);
            Disposed = true;
        }

        public static MemoryResource FromStream(MemoryStream stream)
        {
            var data = stream.ToArray();
            return new MemoryResource(data);
        }
    }
}
