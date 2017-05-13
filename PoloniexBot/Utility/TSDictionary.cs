using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility {
    public class TSDictionary<Key, Value> : Dictionary<Key, Value> {

        public TSDictionary () : base() { }

        public new void Add (Key key, Value value) {
            lock (this) {
                base.Add(key, value);
            }
        }

        public new bool Remove (Key key) {
            lock (this) {
                return base.Remove(key);
            }
        }

        public new void Clear () {
            lock (this) {
                base.Clear();
            }
        }

        public new bool TryGetValue (Key key, out Value value) {
            lock (this) {
                return base.TryGetValue(key, out value);
            }
        }

        public new Value[] ValuesToArray () {
            lock (this) {
                return base.Values.ToArray();
            }
        }

    }
}
