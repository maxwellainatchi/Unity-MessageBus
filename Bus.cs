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

	public interface IHandler {
		void handleMessage<T>(T msg) where T: Message.IMessage;
	}

	public interface IHandler<T>: IHandler where T: Message.IMessage {
		void handleMessage(T msg);
	}

	public abstract class Handler<T>: IHandler<T> where T: Message.IMessage {
		Bus bus;
		public Handler(Bus bus = null) {
			this.bus = bus != null ? bus : Bus.main;
			this.bus.register<T>(this);
		}

		~Handler() {
			this.bus.deregister<T>(this);
		}

		public abstract void handleMessage(T msg);
		public virtual void handleMessage<X>(X msg) where X: Message.IMessage {
			if (msg is T) {
				this.handleMessage(msg as T);
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
		
		// public System.Type[] messages;

		Bus() {
			this.errorHandlers = new List<System.Action<System.Exception, Message.IMessage>>();
			// this.messages = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
			// 			from assemblyType in domainAssembly.GetTypes()
			// 			where typeof(Message.IMessage).IsAssignableFrom(assemblyType)
			// 			&& !(assemblyType.ToString().Contains("IMessage") || 
			// 				assemblyType.ToString().Contains("Base") ||
			// 				assemblyType.ToString().Contains("AllType"))
			// 			select assemblyType).ToArray();
		}

		public Dictionary<System.Type, List<IHandler>> handlers = new Dictionary<System.Type, List<IHandler>>();

		public void register<T>(IHandler<T> handler) where T: Message.IMessage {
			this.registerUnsafely(typeof(T), handler);
		}

		public void registerUnsafely<T>(IHandler handler) where T: Message.IMessage {
			this.registerUnsafely(typeof(T), handler);
		}

		public void registerUnsafely(System.Type type, IHandler handler) {
			if (!(type.IsSubclassOf(typeof(Message.IMessage)))) {
				throw new System.Exception();
			}
			if (!this.handlers.ContainsKey(type)) {
				this.handlers[type] = new List<IHandler>();
			}
			this.handlers[type].Add(handler);
		}

		public void deregister<T>(IHandler<T> handler) where T: Message.IMessage {
			this.deregisterUnsafely(typeof(T), handler);
		}

		public void deregisterUnsafely<T>(IHandler handler) where T: Message.IMessage {
			this.deregisterUnsafely(typeof(T), handler);
		}

		public void deregisterUnsafely(System.Type type, IHandler handler) {
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
			System.Action<IHandler> runHandler = (handler) => {
				try {
					handler.handleMessage(m);
				} catch (System.Exception e) {
					foreach (var errorHandler in this.errorHandlers) {
						errorHandler(e, m);
					}
				} // TODO: Catch errors
			};
			if (this.handlers.ContainsKey(m.GetType())) {
				foreach (IHandler handler in this.handlers[m.GetType()]) {
					runHandler(handler);
				}
			}
			if (this.handlers.ContainsKey(typeof(Message.Any))) {
				foreach (IHandler handler in this.handlers[typeof(Message.Any)]) {
					runHandler(handler);
				}
			}
		}
	}
}
