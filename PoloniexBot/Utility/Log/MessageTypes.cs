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
            return Time.ToString() + " - " + (Sent ? "SENT" : "RECEIVED") + " - " + Message;
        }
    }

    class ErrorMessage {

        public DateTime Time;
        public string Message;
        public string StackTrace = "";
        private bool hasStackTrace = false;

        public ErrorMessage (string Message) {
            this.Time = DateTime.Now;
            this.Message = Message;
        }
        public ErrorMessage (string Message, string StackTrace) {
            this.Time = DateTime.Now;
            this.Message = Message;
            this.StackTrace = StackTrace;
            this.hasStackTrace = true;
        }

        public override string ToString () {
            return Time.ToString() + " - " + Message + (hasStackTrace ? " - " + StackTrace : "");
        }
    }
}
