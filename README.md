# Unity-MessageBus

A powerful message bus for Unity.

## To Install

Add `"com.maxainatchi.messagebus": "https://github.com/maxwellainatchi/Unity-MessageBus.git"` to your `manifest.json` file for the latest version.

To install a specific version, use `"com.maxainatchi.messagebus": "https://github.com/maxwellainatchi/Unity-MessageBus.git#0.1.2"` where `0.1.2` is the version you want to pin to.

## Contributing

I'm always happy to receive suggestions/issues/contributions. For bug reports and suggestions, please open an issue on GitHub. For contributions, open a PR with a description of your changes and I'll look at it ASAP.

## Documentation

Somewhere in the scene, `MessageBusUpdater` must be added and enabled. This allows the main message bus to send messages in `Update`, `FixedUpdate`, and `LateUpdate`.

To send a message, use `MessageBus.main.emit(new MessageType())` where `MessageType` is the type of your message.

### Creating new messages

The most basic message you can create is just to notify that some event happened. Here's what that message looks like:

```c#
public class SomeMessage: Message.IMessage {}
```

#### Adding Data

You can also add associated data type-safely.

```C#
public class MessageWithData: Message.IMessage {
    public int someInt;
    public MessageWithData(int someInt) {
        this.someInt = someInt;
    }
}
```

Emitting this message would look like this: `MessageBus.main.emit(new MessageWithData(5));`

#### Changing when the message is emitted

You can set the message to be emitted immediately, on `Update`, `FixedUpdate`, or `LateUpdate`. This is specific to the message, not to the emission. This defaults to `Update`.

```C#
public class LateUpdateMessage {
    override public UpdateStage getUpdateStage() { return UpdateStage.LateUpdate; }
}
```

Messages sent on `Update`, `LateUpdate`, or `FixedUpdate` will preserve order of emission. 

**IMPORTANT**: Messages sent on `Immediate` will be sent _synchronously_!

### Registering/Deregistering a message handler

Any message handler must conform to `MessageHandler`.  More about message handlers in the next section.

To begin receiving messages on `someMessageHandler`:

```C#
MessageBus.main.register<SomeMessage>(someMessageHandler);
```

To stop receiving messages:

```C#
MessageBus.main.deregister<SomeMessage>(someMessageHandler);
```

### Handling messages

There are 2 ways of handling a message, typed or untyped. 

#### Typed message handlers

This is the recommended way.

If you want to handle messages individually, the best way to do that is to make a typed message handler. This will automatically manage registering and deregistering on the main message bus during the lifetime of the handler, which means that if you want to remove/replace the message handler, you can safely just replace the variable it's stored in and garbage collection will do the rest.

```C#
public class MessageWithDataHandler: MessageHandler<MessageWithData> {
    override public void handleTypedMessage(MessageWithData msg) {
        Debug.Log(msg.someValue);
    }
}

public SomeClassListeningToMessageWithData: MonoBehaviour {
    MessageWithDataHandler messageHandler = new MessageWithDataHandler();
}
```

##### Using a custom message bus with typed message handlers

Warning: This is not recommended, as it hasn't yet been tested and won't work with `MessageBusUpdater`. **Use with caution**.

```C#
public class CustomMessageBusHandler: MessageHandler<MessageWithData> {
    public CustomMessageBusHandler(MessageBus arbitrary): base(arbitrary) {}
    override public void handleTypedMessage(MessageWithData msg) {
        Debug.Log(msg.someValue);
    }
}

public CustomMessageBusExample: MonoBehaviour {
    MessageBus custom = new MessageBus();
    CustomMessageBusHandler messageHandler;
    
    void Awake() {
        this.messageHandler = new CustomMessageBusHandler(this.custom);
    }
}
```



#### Untyped message handlers

Sometimes it's inconvenient to handle each message separately. Maybe you're writing a handler that has common code for handling multiple messages. In this case, you should use an untyped message handler. This can handle multiple messages, and is an interface so you can conform it to `MonoBehaviour`. 

However, you will have to do type checking/casting yourself if you want to access properties on the message, and you need to register/deregister the listener yourself.

```C#
public class ClassHandlingMultipleMessages: MonoBehaviour, MessageHandler {
    void OnEnable() {
       	MessageBus.main.register<SomeMessage>(this);
        MessageBus.main.register<MessageWithData>(this);
    }
    
    void OnDisable() {
        MessageBus.main.register<SomeMessage>(this);
        MessageBus.main.deregister<SomeMessage>(this);
    }
    
    override public void handleMessage<T>(T msg) where T: Message.IMessage {
        Debug.Log("About to handle a message");
        if (msg is SomeMessage) {
            Debug.Log("Got a regular message");
        } else if (msg is MessageWithData) {
            MessageWithData dataMsg = (msg as MessageWithData);
            Debug.Log($"Got a message with data: {dataMsg.someValue}")
        }
        Debug.Log("Just handled the message");
    }
}
```

#### Listening for arbitrary messages

There is a special type, `Message.AllType`, which you can register to listen for all incoming messages. This will likely be removed once hierarchical message listens are enabled.

```C#
MessageBus.main.register<Message.AllType>(someMessageHandler);
// someMessageHandler will now receive all messages
```

### Logging Messages

For debugging purposes, logging messages can be handy. `MessageLogger` is a`MonoBehaviour` that logs all messages to the Unity console.

To use it, simply add it to somewhere in the scene. If it is disabled/inactive, it will stop listening/logging messages.

## To-Do

- [ ] Rename `Message` namespace to `MessageBus`, and move all classes into it
- [ ] Make emission be a property of the message, so that messages can be associated with given message buses.
- [ ] Allow hierarchical message listens, so if you have `ChildMessage` extending from `BaseMessage` extending from `Message.IMessage`, you can register a listener on `BaseMessage` and get notifications for all `ChildMessage` and `BaseMessage` events.
- [ ] Possible: Allow update stages to be overridden on emit for an individual emission.
- [ ] Add better error handling
- [ ] Add message responses