# Developer Notes
Frames classes are the in-memory object representations of ID3 frames. There are agnostic to specific ID3 versions, and hence do not have the knowledge to serialize or deserialize themselves from an ID3 tag; this is the responsibility of the ID3 handler classes.

The Frames base folder contains:
* The `Id3Frame` class, which is the base class for all frames.
* The `Id3FrameList` and `Id3SyncFrameList` classes, which represent collections of frames (discussed in detail below).
* Abstract base classes of frame classes