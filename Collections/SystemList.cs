using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MiniEngine.Collections
{
    public class SystemNode
    {
        public SystemNode? Previous { get; set; }
        public SystemNode? Next { get; set; }

        public System? Value { get; set; }
        public object? Argument { get; set; }

        public void InsertBefore(SystemNode node)
        {
            var prev = Previous;
            if (prev == null)
            {
                Previous = node;
                return;
            }

            prev.Next = node;
            node.Previous = prev;
            node.Next = this;
            Previous = node;
        }

        public void InsertAfter(SystemNode node)
        {
            var next = Next;
            if (next != null)
            {
                next.Previous = node;
                node.Next = next;
            }
            node.Previous = this;
            Next = node;

        }
        
        public static implicit operator (System, object?)(SystemNode node) => (node.Value!, node.Argument);
    }

    public class SystemListEnumerator : IEnumerator<(System, object?)>
    {
        protected SystemNode? _current;
        protected SystemNode? _head;
        private bool _disposedValue;

        public SystemListEnumerator(SystemNode? head)
        {
            _current = null;
            _head = head;
        }

        (System, object?) IEnumerator<(System, object?)>.Current => _current!;

        object IEnumerator.Current => _current!;

        bool IEnumerator.MoveNext()
        {
            switch (_current)
            {
                case { Next: null }:
                    return false;
                case null:
                    _current = _head;
                    return true;
                default:
                    _current = _current!.Next!;
                    return true;
            }
        }

        void IEnumerator.Reset()
        {
            _current = _head;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;
            _current = null;
            _head = null;
            _disposedValue = true;
        }

        void global::System.IDisposable.Dispose()
        {
            Dispose(disposing: true);
            global::System.GC.SuppressFinalize(this);
        }
    }

    public class SystemListNodeEnumerator : IEnumerator<SystemNode>
    {
        private SystemNode? _current;
        private SystemNode? _head;
        private bool _disposedValue;

        public SystemListNodeEnumerator(SystemNode? head)
        {
            _current = null;
            _head = head;
        }

        SystemNode IEnumerator<SystemNode>.Current => _current!;

        object IEnumerator.Current => _current!;

        bool IEnumerator.MoveNext()
        {
            switch (_current)
            {
                case { Next: null }:
                    return false;
                case null:
                    if (_head == null)
                        return false;
                    _current = _head;
                    return true;
                default:
                    _current = _current!.Next!;
                    return true;
            }
        }

        void IEnumerator.Reset()
        {
            _current = _head;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;
            _current = null;
            _head = null;
            _disposedValue = true;
        }

        void global::System.IDisposable.Dispose()
        {
            Dispose(disposing: true);
            global::System.GC.SuppressFinalize(this);
        }
    }

    public class SystemList : IEnumerable<(System, object?)>, IEnumerable<SystemNode>
    {
        private SystemNode? _head;

        IEnumerator<(System, object?)> IEnumerable<(System, object?)>.GetEnumerator()
        {
            return new SystemListEnumerator(_head);
        }

        IEnumerator<SystemNode> IEnumerable<SystemNode>.GetEnumerator()
        {
            return new SystemListNodeEnumerator(_head);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SystemListEnumerator(_head);
        }

        public bool ContainsSystemType<T>() where T : System
        {
            return FindSystemBySystemType<T>() != null;
        }

        public (System, object?)? FindSystemBySystemType<T>(object? data = null) where T : System
        {
            var type = typeof(T);
            return this.FirstOrDefault<(System, object?)>(system => system.GetType() == type && system.Item2 == data);
        }

        public SystemNode? FindFirstSystemNodeBySystemType<T>(object? data = null) where T : System
        {
            var type = typeof(T);
            return this.FirstOrDefault<SystemNode>(system => system?.Value?.GetType() == type && (system.Argument == data || system.Argument == null));
        }

        public SystemNode? FindLastSystemNodeBySystemType<T>(object? data = null) where T : System
        {
            var type = typeof(T);
            return this.LastOrDefault<SystemNode>(
                system => system.Value?.GetType() == type && (system.Argument == null || data == null || (ScriptEvent)system.Argument == (ScriptEvent)data));
        }

        public int Count()
        {
            var count = 0;
            var node = _head;
            while (node != null)
            {
                count++;
                node = node.Next;
            }

            return count;
        }

        public void Add(System system, object? argument = null)
        {
            if (_head is null)
            {
                _head = new SystemNode
                {
                    Value = system,
                    Argument = argument
                };
                return;
            }
            var lastNode = _head;
            while (lastNode.Next != null)
                lastNode = lastNode.Next;
            var newNode = new SystemNode
            {
                Value = system,
                Argument = argument,
                Previous = lastNode
            };
            lastNode.Next = newNode;
        }

        private List<SystemNode> ToNodeList()
        {
            return ((IEnumerable<SystemNode>)this).ToList();
        }

        public void Clear()
        {
            var list = ToNodeList();
            list.ForEach(x =>
            {
                x.Next = null;
                x.Previous = null;
            });
            _head = null;
        }
    }
}