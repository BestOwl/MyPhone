using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoodTimeStudio.MyPhone.OBEX.Utilities
{
    public class DictionaryEqualityComparer<K, V> : IEqualityComparer<Dictionary<K, V>>
    {
        private static readonly DictionaryEqualityComparer<K, V> s_dictEqualityComparer = new DictionaryEqualityComparer<K, V>();
        public static DictionaryEqualityComparer<K, V> Default { get => s_dictEqualityComparer; }

        public bool Equals(Dictionary<K, V> x, Dictionary<K, V> y)
        {
            return x.Count == y.Count && !x.Except(y).Any();
        }

        public int GetHashCode(Dictionary<K, V> obj)
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
