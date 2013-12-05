using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CoreTechs.Logging
{
    /// <summary>
    /// Thread safe collection.
    /// </summary>
    internal class ConcurrentCollection<T> : ICollection<T>
    {
        private readonly ConcurrentDictionary<Guid, T> _dict = new ConcurrentDictionary<Guid, T>();

        public ConcurrentCollection()
        {
        }

        public ConcurrentCollection(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            foreach (var item in items)
                Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dict.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (!_dict.TryAdd(Guid.NewGuid(), item))
                // this should never happen because keys will always be unique
                throw new Exception("Couldn't add the item");
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(T item)
        {
            return _dict.Values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _dict.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            T t;
            return _dict.Where(x => x.Value.Equals(item))
                .Select(x => _dict.TryRemove(x.Key, out t))
                .FirstOrDefault();
        }

        public int Count
        {
            get { return _dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}