# Changelog

## V0.1.4

- Added the ability for messages to require a listener.
- Replaced `getUpdateStage()` with a virtual property.
- Added the ability to add rate limits to each stage on `BusUpdater`.
- Messages are now associated with a default `Bus` and can emit themselves (the new recommended pattern).

## V0.1.3
 - Moved everything into a top level namespace, `Messaging`. Also renamed a few other things. Refer to the documentation for more details.
 - Typed handlers can now exist without a convenience class!
 - Messages now correctly record when they were emitted  

