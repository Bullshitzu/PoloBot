using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoloniexAPI;
using System.Drawing;

namespace PoloniexBot.Data {
    abstract class Predictor : IDisposable {

        protected CurrencyPair pair;
        protected List<ResultSet> results;
        protected bool drawEnabled = true;
        public const long drawTimeframe = 1800;

        public Predictor (CurrencyPair pair) {
            this.pair = pair;
            this.results = new List<ResultSet>();
        }

        public virtual void Dispose () {
            if (results != null) {
                results.Clear();
                results = null;
            }
        }

        public ResultSet GetLastResult () {
            if (results == null || results.Count == 0) return null;
            return results.Last();
        }
        public ResultSet[] GetAllResults () {
            return results.ToArray();
        }

        public void SaveResult (ResultSet rs) {
            SignResult(rs);
            results.Add(rs);
            while (results.Count > 1000) results.RemoveAt(0);
        }

        public abstract void SignResult (ResultSet rs);

    }
}
