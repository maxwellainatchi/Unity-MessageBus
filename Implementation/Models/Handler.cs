namespace Messaging {
	public interface IHandler {
		void handleMessage<T>(T msg) where T : Message.IMessage;
	}

	public interface IHandler<T> : IHandler where T : Message.IMessage {
		void handleMessage(T msg);
	}

	public abstract class Handler<T> : IHandler<T> where T : Message.IMessage {
		Bus bus;
		public Handler(Bus bus = null) {
			this.bus = bus ?? Bus.main;
			this.bus.register<T>(this);
		}

		~Handler() {
			this.bus.deregister<T>(this);
		}

		public abstract void handleMessage(T msg);
		public virtual void handleMessage<X>(X msg) where X : Message.IMessage {
			if (msg is T) {
				this.handleMessage(msg as T);
			}
		}
	}
}
