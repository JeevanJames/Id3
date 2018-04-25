# Developer Notes
Frames classes are the in-memory object representations of ID3 frames. There are agnostic to specific ID3 versions, and hence do not have the knowledge to serialize or deserialize themselves from an ID3 tag; this is the responsibility of the ID3 handler classes.

The Frames folder contains:
* The `Id3Frame` class, which is the base class for all frames. This class has:
    * An `IsAssigned` property, which indicates whether the frame contains valid data. Derived frames classes can override this property to add their custom logic.
* Abstract base classes for the various categories of frames:
    * `TextFrameBase<TValue>` for frames that store textual data.
    * 