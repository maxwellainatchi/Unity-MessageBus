using System.Runtime.CompilerServices;

namespace Messaging.Message {
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

		public void _registerCallerInfo(int line, string file, string funcName) {
			this.callerInfo = new Message.CallerInfo {
				line = line,
				file = file,
				funcName = funcName,
				emittedAt = System.DateTime.Now
			};
		}

		public void emitSelf(Bus bus = null, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string funcName = "") {
			if (bus == null) {
				bus = this.getDefaultBus();
			}
			this._registerCallerInfo(line, file, funcName);
			bus._emit(this);
		}
	}
	public class Any : IMessage { }

	namespace Info {
		public abstract class InfoBase : IMessage {
			override public UpdateStage getUpdateStage() { return UpdateStage.Immediate; }
			public int level { get; }
			public object caller;
			public string message;
			public object associatedValue;
			public InfoBase(int level, object caller, string message, object value = null) {
				this.level = level;
				this.caller = caller;
				this.message = message;
				this.associatedValue = value;
			}
			override public string ToString() {
				return $"{base.ToString()} '{this.message}': {this.associatedValue}\nFrom {this.caller}";
			}
		}
		public class Verbose : InfoBase { public Verbose(object caller, string message, object value = null) : base(0, caller, message, value) { } }
		public class Debug : InfoBase { public Debug(object caller, string message, object value = null) : base(1, caller, message, value) { } }
		public class Warning : InfoBase { public Warning(object caller, string message, object value = null) : base(2, caller, message, value) { } }
		public class Error : InfoBase { public Error(object caller, string message, object value = null) : base(3, caller, message, value) { } }
	}
} // End Namespaces
