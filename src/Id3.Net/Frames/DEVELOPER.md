# Developer Notes
Frames classes are the in-memory object representations of ID3 frames. There are agnostic to specific ID3 versions, and hence do not have the knowledge to serialize or deserialize themselves from an ID3 tag; this is the responsibility of the ID3 handler classes.

The Frames folder contains:
* The `Id3Frame` class, which is the base class for all frames. This class has:
    * An `IsAssigned` property, which indicates whether the frame contains valid data. Derived frames classes can override this property to add their custom logic.
* Abstract base classes for the various categories of frames:
    * `TextFrameBase<TValue>` for frames that store textual data.
    * 

## When adding new frame types
The following areas need to be updated:
* `Id3.Net`
    * Add a property for it in the `Id3Tag` class
    * Handle it in all available ID3 handlers.
* `Id3.Net.Serialization`: Add a serialization surrogate for the frame (or reuse an existing one) and register it in the `SerializationExtensions.IncludeId3SerializationSupport` method.
* `Id3.Net.Files`: Check whether the new frame can be used as a placeholder in a file naming pattern for the `FileNamer` class. If so, add it to the `FileNamer._allowedNames` static field.