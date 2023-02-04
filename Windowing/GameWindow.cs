using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Windowing
{
    public class GameWindow
    {
        private Size _windowSize;
        private readonly IntPtr _windowPtr;
        private readonly IntPtr _rendererPtr;

        internal GameWindow(Size windowSize, IntPtr windowPtr, IntPtr rendererPtr)
        {
            _windowSize = windowSize;
            _windowPtr = windowPtr;
            _rendererPtr = rendererPtr;
        }
    }
}
