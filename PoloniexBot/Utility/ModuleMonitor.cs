using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility {
    public static class ModuleMonitor {

        static Dictionary<string, Module> TrackedModules;

        public delegate void SimpleMethod ();

        struct Module {

            public string id;
            public long timeout;
            public long reportTime;
            public SimpleMethod method;

            public Module (string id, long timeout, SimpleMethod method) {
                this.id = id;
                this.timeout = timeout;
                this.method = method;
                this.reportTime = DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);
            }
        }

        public static void ReportAlive (string id, long timeout, SimpleMethod method) {
            if (TrackedModules == null) TrackedModules = new Dictionary<string, Module>();

            lock (TrackedModules) {
                TrackedModules.Remove(id);
                TrackedModules.Add(id, new Module(id, timeout, method));
            }
        }

        public static void CheckModules () {
            if (TrackedModules == null) return;

            lock (TrackedModules) {
                KeyValuePair<string, Module>[] modules = TrackedModules.ToArray();
                long currTimestamp = DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now);
                for (int i = 0; i < modules.Length; i++) {
                    long timespan = currTimestamp - modules[i].Value.reportTime;
                    if (timespan > modules[i].Value.timeout) {
                        modules[i].Value.method();
                        TrackedModules.Remove(modules[i].Key);
                    }
                }
            }
        }
    }
}
