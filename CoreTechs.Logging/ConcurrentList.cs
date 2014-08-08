using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CoreTechs.Logging
{
    /// <summary>
    /// Thread safe collection.
    /// </summary>
    internal class ConcurrentList<T> : ICollection<T>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly List<T> _list = new List<T>();

        public ConcurrentList()
        {
        }

        public ConcurrentList(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            foreach (var item in items)
                Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _lock.Write(() => _list.Add(item));
        }

        public void Clear()
        {
            _lock.Write(_list.Clear);
        }

        public bool Contains(T item)
        {
            return _lock.Read(() => _list.Contains(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.Read(() => _list.CopyTo(array, arrayIndex));
        }

        public bool Remove(T item)
        {
            return _lock.Write(() => _list.Remove(item));
        }

        public int Count
        {
            get { return _lock.Read(() => _list.Count); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }

    
}