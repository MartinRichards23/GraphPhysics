using GraphPhysics.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPhysics
{
    public class SelectedItemCollection<T> : IEnumerable<T> where T : GraphItem
    {
        readonly List<T> items = new List<T>();

        public void AddRange(IEnumerable<T> collection)
        {
            foreach(T item in collection)
            {
                Add(item);
            }
        }

        public void Add(T item)
        {
            if (item.IsSelected)
            {
                // already selected
                return;
            }

            item.IsSelected = true;
            items.Add(item);
        }

        public void Clear()
        {
            foreach(T item in items)
            {
                item.IsSelected = false;
            }

            items.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
