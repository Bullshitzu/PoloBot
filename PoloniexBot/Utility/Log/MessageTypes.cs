using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Log.MessageTypes {
    class NetMessage {

        public DateTime Time;
        public bool Sent;
        public string Message;

        public NetMessage (bool Sent, string Message) {
            this.Time = DateTime.Now;
            this.Sent = Sent;
            this.Message = Message;
        }

        public override string ToString () {
            return Time.ToString() + " - " + (Sent ? "OUTGOING" : "INCOMING") + " - " + Message;
        }

    }
}
