using System.Runtime.CompilerServices;

namespace Messaging.Message {
	public abstract class IMessage {
		public enum RequireListenerOption { No, Typed, Untyped, Any }
		public CallerInfo callerInfo;

		// These options can be overridden in derived classes to change how the message behaves by default
		public virtual UpdateStage updateStage { get { return UpdateStage.Update; } }
		public virtual RequireListenerOption requireListener { get { return RequireListenerOption.Typed; } }
		public virtual Bus defaultBus { get { return Bus.main; } }

		public void emitSelf(Bus bus = null, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string funcName = "") {
			bus = bus ?? this.defaultBus;
			this._registerCallerInfo(line, file, funcName);
			bus._emit(this);
		}

		// MARK: Utility

		override public string ToString() {
			string str = $"[On <i>{this.updateStage}</i>] {this.GetType()}";
			if (callerInfo != null) {
				str = callerInfo.ToString() + ": " + str;
			}
			return str;
		}

		// MARK: Internal methods

		/** Note: This is public simply to allow the message bus emit to emit messages
		 *  while correctly registering where they were emitted. This should be considered private for all
		 *  intents and purposes.
		 */
		public void _registerCallerInfo(int line, string file, string funcName) {
			this.callerInfo = new Message.CallerInfo {
				line = line,
				file = file,
				funcName = funcName,
				emittedAt = System.DateTime.Now
			};
		}
	}

	// placeholder type
	// TODO: replace with hierarchical listens
	public class Any : IMessage { }
}
