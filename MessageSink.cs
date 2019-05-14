using System.Collections.Generic;

public interface MessageSink {
    void outputMessage(string message);
}

[System.Serializable]
public class MessageSinkHandler: MessageHandler {
    MessageSink sink;

    public MessageSinkHandler(MessageSink sink) {
        this.sink = sink;
        MessageBus.main.register<Message.AllType>(this);
    }

    ~MessageSinkHandler() {
        MessageBus.main.deregister<Message.AllType>(this);
    }

    public void handleMessage<T>(T m) where T: Message.IMessage {
        this.sink.outputMessage(m.ToString());
    }
}

public class MessageErrorSinkHandler {
    MessageSink sink;

    public MessageErrorSinkHandler(MessageSink sink) {
        this.sink = sink;
        MessageBus.main.errorHandlers.Add(this.outputMessageHandlerError);
    }

    ~MessageErrorSinkHandler() {
        MessageBus.main.errorHandlers.Remove(this.outputMessageHandlerError);
    }

    public void outputMessageHandlerError(System.Exception e, Message.IMessage message) {
        this.sink.outputMessage($"Error handling {message.ToString()}: ${e.ToString()}");
    }
}
