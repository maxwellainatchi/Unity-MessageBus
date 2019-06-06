using System.Collections.Generic;

namespace Messaging.Utilities {
	public interface MessageSink {
		void outputMessage(string message);
	}

	[System.Serializable]
	public class MessageSinkHandler : Messaging.IHandler {
		MessageSink sink;

		public MessageSinkHandler(MessageSink sink) {
			this.sink = sink;
			Messaging.Bus.main.registerUnsafely<Message.Any>(this);
		}

		~MessageSinkHandler() {
			Messaging.Bus.main.deregisterUnsafely<Message.Any>(this);
		}

		public void handleMessage<T>(T m) where T : Message.IMessage {
			this.sink.outputMessage(m.ToString());
		}
	}

	public class MessageErrorSinkHandler {
		MessageSink sink;

		public MessageErrorSinkHandler(MessageSink sink) {
			this.sink = sink;
			Messaging.Bus.main.errorHandlers.Add(this.outputMessageHandlerError);
		}

		~MessageErrorSinkHandler() {
			Messaging.Bus.main.errorHandlers.Remove(this.outputMessageHandlerError);
		}

		public void outputMessageHandlerError(System.Exception e, Message.IMessage message) {
			this.sink.outputMessage($"Error handling {message.ToString()}: ${e.ToString()}");
		}
	}
}
