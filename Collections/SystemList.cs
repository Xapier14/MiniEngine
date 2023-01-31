using System.Collections;
using System.Collections.Generic;

namespace MiniEngine.Collections
{
    public class SystemNode
    {
        public SystemNode? Previous { get; set; }
        public SystemNode? Next { get; set; }

        public System? Value { get; set; }
        
        public static implicit operator System(SystemNode node) => node.Value!;
    }
    public class SystemListEnumerator : IEnumerator<System>
    {
        private SystemNode? _current;
        private SystemNode? _head {get; init;}
        private bool disposedValue;

        public SystemListEnumerator(SystemNode? head)
        {
            _current = head;
            _head = head;
        }

        System IEnumerator<System>.Current => _current!;

        object IEnumerator.Current => _current!;

        bool IEnumerator.MoveNext()
        {
            if (_current?.Next != null)
                return false;
            _current = _current!.Next!;
            return true;
        }

        void IEnumerator.Reset()
        {
            _current = _head;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SystemListEnumerator()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void global::System.IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            global::System.GC.SuppressFinalize(this);
        }
    }
    public class SystemList : IEnumerable<System>
    {
        private SystemNode? _head = null;

        IEnumerator<System> IEnumerable<System>.GetEnumerator()
        {
            return new SystemListEnumerator(_head);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SystemListEnumerator(_head);
        }
    }
}