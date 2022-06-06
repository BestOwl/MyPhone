using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Utilities
{
    public static class ObservableCollectionExtensions
    {
        public static int Remove<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            var itemsToRemove = collection.Where(predicate).ToList();
            foreach (T item in itemsToRemove)
            {
                collection.Remove(item);
            }
            return itemsToRemove.Count;
        }
    }
}
