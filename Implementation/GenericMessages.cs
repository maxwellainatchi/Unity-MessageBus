using System.Runtime.CompilerServices;

namespace Messaging.Message {
    public static class MessageExtensionCaller {
        public static T registerLine<T>(this T t, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string funcName = "") where T: IMessage {
            t.callerInfo = new CallerInfo {
                line = line,
                file = file,
                funcName = funcName
            };
            return t;
        }
    }

    public class CallerInfo {
        public int line;
        public string file;
        public string funcName;
        public System.DateTime emittedAt;
        public System.DateTime sentAt;

        public override string ToString() {
            return $"<b>{System.IO.Path.GetFileName(file)}@{line} (in {funcName})</b>";
        }
    }
    public abstract class IMessage {
        public enum RequireListenerOption { No, Typed, Untyped, Any }
        public CallerInfo callerInfo;

        public virtual UpdateStage getUpdateStage() { return UpdateStage.Update; }
        public virtual RequireListenerOption requireListener() { return RequireListenerOption.Typed; }
        public virtual Bus getDefaultBus() { return Bus.main; }

        override public string ToString() {
            string str = $"[On <i>{this.getUpdateStage()}</i>] {this.GetType()}";
            if (callerInfo != null) {
                str = callerInfo.ToString() + ": " + str;
            }
            return str;
        }

        public void emitSelf(Bus bus = null, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string funcName = "") {
            if (bus == null) {
                bus = this.getDefaultBus();
            }
            bus.emit(this, line, file, funcName);
        }
    }
    public class Any: IMessage { }

    namespace Info {
        public abstract class InfoBase: IMessage {
            override public UpdateStage getUpdateStage() { return UpdateStage.Immediate; }
            public int level { get; protected set; }
            public object caller;
            public string message;
            public object associatedValue; 
            override public string ToString() {
                return $"{base.ToString()} '{this.message}': {this.associatedValue}\nFrom {this.caller}";
            }
        }
        public class Verbose: InfoBase { public Verbose(object caller, string message, object value = null) { this.level = 0; this.caller = caller; this.message = message; this.associatedValue = value; } }
        public class Debug: InfoBase { public Debug(object caller, string message, object value = null) { this.level = 1; this.caller = caller; this.message = message; this.associatedValue = value; } }
        public class Warning: InfoBase { public Warning(object caller, string message, object value = null) { this.level = 2; this.caller = caller; this.message = message; this.associatedValue = value; } }
        public class Error: InfoBase { public Error(object caller, string message, object value = null) { this.level = 3; this.caller = caller; this.message = message; this.associatedValue = value; } }
    }
} // End Namespaces
