using System;
using System.Collections.Generic;

namespace CoreTechs.Logging
{
    internal class DelegateEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, int> _hc;
        private readonly Func<T, T, bool> _eq;

        public DelegateEqualityComparer(Func<T,T,bool> equalsFunc, Func<T,int> hashCodeFunc = null)
        {
            if (equalsFunc == null) throw new ArgumentNullException("equalsFunc");
        
            _eq = equalsFunc;
            _hc = hashCodeFunc ?? (x => 0);
        }

        public bool Equals(T x, T y)
        {
            return _eq(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _hc(obj);
        }
    }
}