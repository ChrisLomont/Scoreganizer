using System;
using System.Collections.Generic;
using System.Text;
using MvvmCross.ViewModels;

namespace Lomont.Scoreganizer.Core.Model
{
    public class MostRecentlyUsed<T>
    {
        // todo - hide this? make enumerable?
        public MvxObservableCollection<T> UsedItems { get;  } = new MvxObservableCollection<T>();

        /// <summary>
        /// Set most recently used item
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item, int sizeMax = Int32.MaxValue)
        {
            if (UsedItems.Contains(item))
                UsedItems.Remove(item);
            if (UsedItems.Count == sizeMax)
                UsedItems.RemoveAt(sizeMax-1);
            UsedItems.Insert(0,item);
        }
    }
}
