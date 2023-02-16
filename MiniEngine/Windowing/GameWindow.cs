using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Windowing
{
    public class GameWindow
    {
        public Size WindowSize { get; }
        public IntPtr WindowPtr { get; }
        public IntPtr RendererPtr { get; }

        internal GameWindow(Size windowSize, IntPtr windowPtr, IntPtr rendererPtr)
        {
            WindowSize = windowSize;
            WindowPtr = windowPtr;
            RendererPtr = rendererPtr;
        }
    }
}
