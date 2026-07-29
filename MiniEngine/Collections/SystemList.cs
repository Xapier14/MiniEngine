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

        public static implicit operator (System, object?)(SystemNode node) => (node.Value!, node.Argument);
    }

    public class SystemListEnumerator : IEnumerator<(System, object?)>
    {
        protected SystemNode? Current;
        protected SystemNode? Head;
        private bool _disposedValue;

        public SystemListEnumerator(SystemNode? head)
        {
            Current = null;
            Head = head;
        }

        (System, object?) IEnumerator<(System, object?)>.Current => Current!;

        object IEnumerator.Current => Current!;

        bool IEnumerator.MoveNext()
        {
            switch (Current)
            {
                case { Next: null }:
                    return false;
                case null:
                    if (Head == null)
                        return false;
                    Current = Head;
                    return true;
                default:
                    Current = Current!.Next!;
                    return true;
            }
        }

        void IEnumerator.Reset()
        {
            Current = Head;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;
            Current = null;
            Head = null;
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

    public class SystemList : IEnumerable<(System, object?)>, IEnumerable<SystemNode>, IEnumerable
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
            // Must go through the node finder: SystemNode is a reference type, so its
            // FirstOrDefault genuinely yields null when there is no match. The tuple
            // sequence below cannot express "not found" that way.
            return FindFirstSystemNodeBySystemType<T>() != null;
        }

        public (System, object?)? FindSystemBySystemType<T>(object? data = null) where T : System
        {
            var type = typeof(T);
            foreach (var node in (IEnumerable<SystemNode>)this)
            {
                if (node.Value?.GetType() == type && node.Argument == data)
                    return (node.Value, node.Argument);
            }

            return null;
        }

        public SystemNode? FindFirstSystemNodeBySystemType<T>(object? data = null) where T : System
        {
            var type = typeof(T);
            return this.FirstOrDefault<SystemNode>(system => system.Value?.GetType() == type && (system.Argument == data || system.Argument == null));
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

        /// <summary>
        /// Links <paramref name="node"/> immediately before <paramref name="target"/>,
        /// promoting it to head when <paramref name="target"/> is the current head.
        /// </summary>
        public void InsertBefore(SystemNode target, SystemNode node)
        {
            var previous = target.Previous;
            node.Previous = previous;
            node.Next = target;
            target.Previous = node;
            if (previous == null)
                _head = node;
            else
                previous.Next = node;
        }

        /// <summary>
        /// Links <paramref name="node"/> immediately after <paramref name="target"/>.
        /// </summary>
        public void InsertAfter(SystemNode target, SystemNode node)
        {
            var next = target.Next;
            node.Previous = target;
            node.Next = next;
            target.Next = node;
            if (next != null)
                next.Previous = node;
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

        public IEnumerator GetEnumerator()
        {
            return new SystemListNodeEnumerator(_head);
        }
    }
}