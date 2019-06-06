using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Messaging {
	public partial class Bus {
		public static readonly Bus main = new Bus();

		public List<System.Action<System.Exception, Message.IMessage>> errorHandlers = new List<System.Action<System.Exception, Message.IMessage>>();
		public Dictionary<System.Type, List<IHandler>> handlers = new Dictionary<System.Type, List<IHandler>>();

		// MARK: register/unregister

		public void register<T>(IHandler<T> handler) where T : Message.IMessage {
			this.registerUnsafely(typeof(T), handler);
		}

		public void registerUnsafely<T>(IHandler handler) where T : Message.IMessage {
			this.registerUnsafely(typeof(T), handler);
		}

		public void registerUnsafely(System.Type type, IHandler handler) {
			if (!(type.IsSubclassOf(typeof(Message.IMessage)))) {
				throw new System.Exception("Can't register to a type that's not a subclass of Message.IMessage!");
			}
			if (!this.handlers.ContainsKey(type)) {
				this.handlers[type] = new List<IHandler>();
			}
			this.handlers[type].Add(handler);
		}

		public void deregister<T>(IHandler<T> handler) where T : Message.IMessage {
			this.deregisterUnsafely(typeof(T), handler);
		}

		public void deregisterUnsafely<T>(IHandler handler) where T : Message.IMessage {
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

		public void emit(Message.IMessage msg, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", [CallerMemberName] string funcName = "") {
			msg._registerCallerInfo(line, file, funcName);
			this._emit(msg);
		}

		// MARK: Internal methods

		/** Note: This is public simply to allow messages to emit themselves
		 *  while correctly registering where they were emitted. This should be considered private for all
		 *  intents and purposes.
		 */
		public void _emit(Message.IMessage msg) {
			var stage = msg.getUpdateStage();
			if (stage == UpdateStage.Immediate) {
				this._sendMessageToHandlers(msg);
			} else {
				BusUpdater.AddMessage(msg);
			}
		}

		/** Note: this method is public simply to allow access from BusUpdater.
		 *  This should be considered private for all intents and purposes.
		 */
		public void _sendMessageToHandlers(Message.IMessage msg) {
			msg.callerInfo.sentAt = System.DateTime.Now;
			System.Action<System.Exception> errorHandler = (exception) => {
				foreach (var handler in this.errorHandlers) {
					handler(exception, msg);
				} // TODO: Handle errors better
			};

			System.Action<IHandler> runHandler = (handler) => {
				try {
					handler.handleMessage(msg);
				} catch (System.Exception err) {
					errorHandler(err);
				} // TODO: Catch errors
			};

			bool didHaveHandler = false;
			if (this.handlers.ContainsKey(msg.GetType())) {
				didHaveHandler = true;
				foreach (IHandler handler in this.handlers[msg.GetType()]) {
					runHandler(handler);
				}
			} else if (msg.requireListener() == Message.IMessage.RequireListenerOption.Typed) {
				errorHandler(new System.Exception("No specific listener for message " + msg.GetType().Name));
			}
			if (this.handlers.ContainsKey(typeof(Message.Any))) {
				didHaveHandler = true;
				foreach (IHandler handler in this.handlers[typeof(Message.Any)]) {
					runHandler(handler);
				}
			} else if (msg.requireListener() == Message.IMessage.RequireListenerOption.Untyped) {
				errorHandler(new System.Exception("No generic listener for message " + msg.GetType().Name));
			}

			if (msg.requireListener() == Message.IMessage.RequireListenerOption.Any && !didHaveHandler) {
				errorHandler(new System.Exception("No listener for message " + msg.GetType().Name));
			}
		}
	}
}
