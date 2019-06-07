`Unity-MessageBus` is a powerful message bus for Unity.

# To install

Add `"com.maxainatchi.messagebus": "https://github.com/maxwellainatchi/Unity-MessageBus.git"` to your `manifest.json` file for the latest version.

To install a specific version, use `"com.maxainatchi.messagebus": "https://github.com/maxwellainatchi/Unity-MessageBus.git#0.1.2"` where `0.1.2` is the version you want to pin to.

# Contributing

I'm always happy to receive suggestions/issues/contributions. For bug reports and suggestions, please open an issue on GitHub. For contributions, open a PR with a description of your changes and I'll look at it ASAP.

# Documentation

There are examples in the "examples" folder.

## BusUpdater

Somewhere in the scene, `Messaging.BusUpdater` must be added to a `GameObject` and enabled. This allows the main message bus (which is not a `MonoBehaviour`) to send messages in `Update`, `FixedUpdate`, and `LateUpdate`. Allowing `BusUpdater` to support message buses other than main is planned for a future update.

### Rate-limiting messages

`BusUpdater` has support for rate-limiting messages in `Update`, `FixedUpdate`, and `LateUpdate` by number of messages or by time (or both).  To use from the inspector, you can expand the `Time Limits` or `Count Limits` section and press the `+` button on the right side. Note that the `+` button will only appear if the section is expanded. **Time limits are in milliseconds**.

Shoutout to the great [Serializable Dictionary Asset](https://assetstore.unity.com/packages/tools/integration/serializabledictionary-90477) by Mathieu Le Ber for allowing the dictionaries to be shown in the inspector!

## Namespaces

The main namespace is `Messaging`. Everything is within this namespace.

`Messaging.Messages` contains the `IMessage` interface, and a few other generic messages.

`Messaging.Extras` contains some convenience classes such as `MessageLogger`.

## Emitting messages

There are two ways of emitting a message. The recommended way is to emit the message itself. (`MessageType` is the type of the message you want to emit).

```C#
new MessageType().emitSelf();
```

This will emit the message on its default bus.

You can also override which bus it's being emitted from by passing it as a parameter. (`someBus` is an instantiated message bus).

```C#
new MessageType().emitSelf(bus: someBus);
```

The other way is to use `someBus.emit(new MessageType())` . This is not recommended and may be removed in a future release.

## Creating new messages

The most basic message you can create is just to notify that some event happened. Here's what that message looks like:

```c#
public class SomeMessage: Messaging.Message.IMessage {}
```

### Adding data

You can also add associated data type-safely.

```C#
public class MessageWithData: Messaging.Message.IMessage {
    public int someInt;
    public MessageWithData(int someInt) {
        this.someInt = someInt;
    }
}
```

Emitting this message would look like this: `Messaging.Bus.main.emit(new MessageWithData(5));`

### Changing when the message is emitted

You can set the message to be emitted immediately, on `Update`, `FixedUpdate`, or `LateUpdate`. This is specific to the message, not to the emission. This defaults to `Update`.

```C#
public class LateUpdateMessage {
    override public Messaging.UpdateStage updateStage { get { return Messaging.UpdateStage.LateUpdate; } }
}
```

Messages sent on `Update`, `LateUpdate`, or `FixedUpdate` will preserve order of emission. 

**IMPORTANT**: Messages sent on `Immediate` will be sent _synchronously_ and _on the same thread they were emitted from_!

### Changing the default message bus

When a message is emitted with `message.emitSelf()`, it's emitted on the default bus for the message (which defaults to `Bus.main`). You can override that per message type by overriding the `defaultBus` property.

```C#
public class OtherMessageBusMessage {
    override public Messaging.Bus defaultBus { get { return someOtherBus; } }
}
```

### Requiring listeners on messages

A message can require that a handler is listening to it. If there is no listener, it will pass an error to the error handlers on emit. There are 3 different types of requirements: `RequireListenerOption.Typed`. `ReqiuireListenerOption.Untyped`, and `RequireListenerOption.Any`. `Any` just ensures that there is something listening to it. `Typed` ensures that there's something listening for that specific message type, and `Untyped` ensures that there's something (e.g. a message logger) listening for ANY type.

```C#
public class RequireListenerMessage {
    override public Messaging.RequireListenerOption requireListener { get { return Messaging.RequireListenerOption.Any; } }
}
```

## Handling messages

There are 2 ways of handling a message, typed or untyped. 

### Typed message handlers

This is the recommended way.

If you know which messages you want to handle at compile-time, typed message handlers provide additional stability and less boilerplate to access the data contained within the message.

```C#
public class MessageWithDataHandler1: Messaging.IHandler<MessageWithData> {
    public void handleMessage(MessageWithData msg) {
        Debug.Log(msg.someValue);
    }
}
```

One class can have many different message handlers conforming to it; there must be a separate `handleMessage` for each type. This can also be defined on a `MonoBehaviour` safely. 

#### `Messaging.Handler<T>`

This is a convenience class that implements `Messaging.IHandler<T>`. 

This will automatically manage registering and deregistering on the main message bus during the lifetime of the handler, which means that if you want to remove/replace the message handler, you can safely just replace the variable it's stored in and garbage collection will do the rest.

```C#
public class MessageWithDataHandler: Messaging.Handler<MessageWithData> {
    override public void handleMessage(MessageWithData msg) {
        Debug.Log(msg.someValue);
    }
}

public SomeClassListeningToMessageWithData: MonoBehaviour {
    MessageWithDataHandler messageHandler = new MessageWithDataHandler();
}
```

#### Using a custom message bus with `Messaging.Handler<T>`

Warning: This is not recommended, as it hasn't yet been tested and won't work with `MessageBusUpdater`. **Use with caution**.

```C#
public class CustomMessageBusHandler: Messaging.Handler<MessageWithData> {
    public CustomMessageBusHandler(Messaging.Bus arbitrary): base(arbitrary) {}
    override public void handleTypedMessage(MessageWithData msg) {
        Debug.Log(msg.someValue);
    }
}

public CustomMessageBusExample: MonoBehaviour {
    Messaging.Bus custom = new Messaging.Bus();
    CustomMessageBusHandler messageHandler;
    
    void Awake() {
        this.messageHandler = new CustomMessageBusHandler(this.custom);
    }
}
```

### Untyped message handlers

Sometimes it's inconvenient to handle each message separately. Maybe you're writing a handler that has common code for handling multiple messages. In this case, you should use an untyped message handler. This can handle multiple messages, and is an interface so you can conform it to `MonoBehaviour`. 

However, you will have to do type checking/casting yourself if you want to access properties on the message. Unlike with typed message handlers, there is no convenience class to automatically register/deregister the listeners. You also will have to use `registerUnsafely` to register the messages.

```C#
public class ClassHandlingMultipleMessages: MonoBehaviour, Messaging.IHandler {
    void OnEnable() {
       	Messaging.Bus.main.registerUnsafely<SomeMessage>(this);
        Messaging.Bus.main.registerUnsafely<MessageWithData>(this);
    }
    
    void OnDisable() {
        Messaging.Bus.main.deregisterUnsafely<SomeMessage>(this);
        Messaging.Bus.main.deregisterUnsafely<MessageWithData>(this);
    }
    
    override public void handleMessage<T>(T msg) where T: Messaging.Message.IMessage {
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

**NOTE**: you can not have one class be both a `Messaging.IHandler<T>` and a `Messaging.IHandler`! It _WILL_ break the typed messages.

### Listening for arbitrary messages

There is a special type, `Messaging.Message.Any`, which you can register to listen for all incoming messages. This will likely be removed once hierarchical message listens are enabled.

```C#
Messaging.Bus.main.register<Messaging.Message.Any>(someMessageHandler);
// someMessageHandler will now receive all messages
```

### Registering/deregistering a message handler

Any message handler must conform to `Messaging.IHandler`.  More about message handlers in [the next section](#Handling messages).

To begin receiving messages on `someMessageHandler`:

```C#
Messaging.Bus.main.register<SomeMessage>(someMessageHandler);
```

To stop receiving messages:

```C#
Messaging.Bus.main.deregister<SomeMessage>(someMessageHandler);
```

There are also a few variants: `registerUnsafely<T>(IHandler handler)` and `deregisterUnsafely<T>(IHandler handler)` allow you to register/deregister an [untyped message handler](#Untyped message handlers), and `registerUnsafely(Type type, IHandler handler)` and `registerUnsafely(Type type, IHandler handler)` allow you to register message types dynamically.

## Handling errors

Documentation TBD.

## Logging messages

For debugging purposes, logging messages can be handy. `Messaging.MessageLogger` is a `MonoBehaviour` that logs all messages to the Unity console.

To use it, simply add it to somewhere in the scene. If it is disabled/inactive, it will stop listening/logging messages.
