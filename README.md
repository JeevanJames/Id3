# ID3.NET
ID3.NET is a set of libraries for reading, modifying and writing ID3 and Lyrics3 tags in MP3 audio files.
The core library supports netstandard20 and the full .NET 4.x frameworks.

ID3.NET also provides an extensible metadata discovery framework that can discover specific ID3 frame data (like album art, lyrics, etc.) from various online services such as Amazon, ChartLyrics.com, Discogs and more.

## ID3 Support
Currently, ID3.NET supports the two most popular ID3 versions, v1.x and v2.3.

The v1.x support applies to both v1.0 and v1.1, as the difference between the two versions is very small.

While ID3.NET can read v2.3 tags, it does not recognize all v2.3 frames yet. Unrecognized frames are stored in a special `UnknownFrame` class and do not raise exceptions. Most of the commonly-used frames such as title, album, artist, etc. are implemented and we are actively working on completing the remaining frame support. This will be done in the ID3.NET v0.x version range, culminating in a v1.0 release with full ID3 v2.3 frame support.
You can track the progress of frame implementation and see the list of currently supported frames [here](v23SupportedFrames).

We will start on ID3 v2.2 and v2.4 tag support as soon as the v2.3 codebase is done.
Please see the [project roadmap](Roadmap) for more details.

## Example
```cs
string[] musicFiles = Directory.GetFiles(@"C:\Music", "*.mp3");
foreach (string musicFile in musicFiles)
{
    using (var mp3 = new Mp3(musicFile))
    {
        Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
        Console.WriteLine("Title: {0}", tag.Title.Value);
        Console.WriteLine("Artist: {0}", tag.Artists.Value);
        Console.WriteLine("Album: {0}", tag.Album.Value);
    }
}
```
