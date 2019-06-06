namespace Messaging.Message {
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
}
