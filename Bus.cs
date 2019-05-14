using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Messaging {
	public enum UpdateStage {
		Immediate,
		Update,
		LateUpdate,
		FixedUpdate,
	}

	public interface Handler {
		void handleMessage<T>(T m) where T: Message.IMessage;
	}

	public abstract class Handler<T>: Handler where T: Message.IMessage {
		Bus bus;
		public Handler(Bus bus = null) {
			this.bus = bus != null ? bus : Bus.main;
			this.bus.register<T>(this);
		}

		~Handler() {
			this.bus.deregister<T>(this);
		}

		public abstract void handleTypedMessage(T m);
		public virtual void handleMessage<X>(X m) where X: Message.IMessage {
			if (m is T) {
				this.handleTypedMessage(m as T);
			}
		}
	}

	public class Bus {
		static Bus _main;
		public static Bus main {
			get { 
				if (Bus._main == null) {
					Bus._main = new Bus();
				}
				return Bus._main;
			}
		}

		public List<System.Action<System.Exception, Message.IMessage>> errorHandlers;
		
		public System.Type[] messages;

		Bus() {
			this.errorHandlers = new List<System.Action<System.Exception, Message.IMessage>>();
			this.messages = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
						from assemblyType in domainAssembly.GetTypes()
						where typeof(Message.IMessage).IsAssignableFrom(assemblyType)
						&& !(assemblyType.ToString().Contains("IMessage") || 
							assemblyType.ToString().Contains("Base") ||
							assemblyType.ToString().Contains("AllType"))
						select assemblyType).ToArray();
		}

		public Dictionary<System.Type, List<Handler>> handlers = new Dictionary<System.Type, List<Handler>>();

		public void register<T>(Handler handler) where T: Message.IMessage {
			this.register(typeof(T), handler);
		}

		public void register(System.Type type, Handler handler) {
			if (!(type.IsSubclassOf(typeof(Message.IMessage)))) {
				throw new System.Exception();
			}
			if (!this.handlers.ContainsKey(type)) {
				this.handlers[type] = new List<Handler>();
			}
			this.handlers[type].Add(handler);
		}

		public void deregister<T>(Handler handler) where T: Message.IMessage {
			this.deregister(typeof(T), handler);
		}

		public void deregister(System.Type type, Handler handler) {
			if (!(type.IsSubclassOf(typeof(Message.IMessage)))) {
				throw new System.Exception();
			}
			if (this.handlers.ContainsKey(type)) {
				this.handlers[type].Remove(handler);
			}

		}

		public void emit(Message.IMessage m) {
			lock (this) {
				var stage = m.getUpdateStage();
				if (stage == UpdateStage.Immediate) {
					this._sendMessageToHandlers(m);
				} else {
					BusUpdater.AddMessage(m);
				}
			}
		}

		public void _sendMessageToHandlers(Message.IMessage m, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string funcName = "") {
			m.callerInfo = new Message.CallerInfo {
				line = line,
				file = file,
				funcName = funcName
			};
			System.Action<Handler> runHandler = (handler) => {
				try {
					handler.handleMessage(m);
				} catch (System.Exception e) {
					foreach (var errorHandler in this.errorHandlers) {
						errorHandler(e, m);
					}
				} // TODO: Catch errors
			};
			if (this.handlers.ContainsKey(m.GetType())) {
				foreach (Handler handler in this.handlers[m.GetType()]) {
					runHandler(handler);
				}
			}
			if (this.handlers.ContainsKey(typeof(Message.Any))) {
				foreach (Handler handler in this.handlers[typeof(Message.Any)]) {
					runHandler(handler);
				}
			}
		}
	}
}
