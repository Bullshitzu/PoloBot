using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Log.MessageTypes {

    public enum MessageDrawTypes {
        Event,
        Error
    }

    public class Message {

        public DateTime Time;
        public string Text;

        public bool DrawInHistory;
        public MessageDrawTypes DrawType;

        private const char Separator = '=';

        public Message (string message) {
            this.Time = DateTime.Now;
            this.Text = message;
            this.DrawInHistory = true;
            this.DrawType = MessageDrawTypes.Event;
        }

        public string ToStringFile () {
            string line = Utility.DateTimeHelper.DateTimeToUnixTimestamp(this.Time).ToString();
            
            line += Separator;
            line += this.Text;

            return line;
        }

        public static Message Parse (string line) {
            string[] parts = line.Split(Separator);

            long timestamp = long.Parse(parts[0]);
            string message = parts[1];

            Message tempMsg = new Message(message);
            tempMsg.Time = Utility.DateTimeHelper.UnixTimestampToDateTime(timestamp);

            return tempMsg;
        }
    }


    public class NetMessage : Message {

        public bool Sent;

        public NetMessage (bool Sent, string Message) : base(Message) {
            this.Sent = Sent;
            this.DrawInHistory = false;
        }

        public override string ToString () {
            return Time.ToString() + " - " + (Sent ? "SENT" : "RECEIVED") + " - " + Text;
        }
    }

    public class ErrorMessage : Message {

        public string StackTrace = "";
        private bool hasStackTrace = false;

        private const char Separator = '=';

        public ErrorMessage (string Message) : base(Message) {
            this.DrawInHistory = true;
            this.DrawType = MessageDrawTypes.Error;
        }
        public ErrorMessage (string Message, string StackTrace) : base(Message) {
            this.StackTrace = StackTrace;
            this.hasStackTrace = true;
            this.DrawInHistory = true;
            this.DrawType = MessageDrawTypes.Error;
        }

        public override string ToString () {
            return Time.ToString() + " - " + Text + (hasStackTrace ? " - " + StackTrace : "");
        }

        public static ErrorMessage Parse (string line) {
            string[] parts = line.Split(Separator);

            long timestamp = long.Parse(parts[0]);
            string message = parts[1];

            ErrorMessage tempMsg = new ErrorMessage(message);
            tempMsg.Time = Utility.DateTimeHelper.UnixTimestampToDateTime(timestamp);

            return tempMsg;
        }
    }
}
