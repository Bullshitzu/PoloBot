using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility {
    public class TSList<T> : List<T> {

        public TSList () : base() { }

        public TSList (T[] items) : base(items) { }

        public new void Add (T item) {
            lock (this) {
                base.Add(item);
            }
        }

        public new void AddRange (IEnumerable<T> items) {
            lock (this) {
                base.AddRange(items);
            }
        }

        public new void RemoveAt (int index) {
            lock (this) {
                base.RemoveAt(index);
            }
        }

        public new void Remove (T item) {
            lock (this) {
                base.Remove(item);
            }
        }

        public new void RemoveAll(Predicate<T> predicate) {
            lock (this) {
                base.RemoveAll(predicate);
            }
        }

        public new T[] ToArray () {
            lock (this) {
                return base.ToArray();
            }
        }
    }
}
