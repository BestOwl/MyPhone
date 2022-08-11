using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoodTimeStudio.MyPhone.OBEX.Utilities
{
    public class DictionaryEqualityComparer<TKey, V> : IEqualityComparer<Dictionary<TKey, V>>
    {
        private static readonly DictionaryEqualityComparer<TKey, V> s_dictEqualityComparer = new DictionaryEqualityComparer<TKey, V>();
        public static DictionaryEqualityComparer<TKey, V> Default { get => s_dictEqualityComparer; }

        public bool Equals(Dictionary<TKey, V> x, Dictionary<TKey, V> y)
        {
            return x.Count == y.Count && !x.Except(y).Any();
        }

        public int GetHashCode(Dictionary<TKey, V> obj)
        {
            var hash = 13;
            var orderedKVPList = obj.OrderBy(kvp => kvp.Key);
            foreach (var kvp in orderedKVPList)
            {
                if (kvp.Key != null)
                {
                    hash = (hash * 7) + kvp.Key.GetHashCode();
                }
                if (kvp.Value != null)
                {
                    hash = (hash * 7) + kvp.Value.GetHashCode();
                }
            }
            return hash;
        }
    }
}
