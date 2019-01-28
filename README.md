# ID3.NET
[![Build status](https://img.shields.io/appveyor/ci/JeevanJames/Id3.svg)](https://ci.appveyor.com/project/JeevanJames/id3/branch/master) [![Test status](https://img.shields.io/appveyor/tests/JeevanJames/id3.svg)](https://ci.appveyor.com/project/JeevanJames/id3/branch/master) [![codecov](https://codecov.io/gh/JeevanJames/Id3/branch/master/graph/badge.svg)](https://codecov.io/gh/JeevanJames/Id3) [![NuGet Version](http://img.shields.io/nuget/v/Id3.svg?style=flat)](https://www.nuget.org/packages/Id3/) [![NuGet Downloads](https://img.shields.io/nuget/dt/Id3.svg)](https://www.nuget.org/packages/Id3/)

ID3.NET is a set of libraries for reading, modifying and writing ID3 and Lyrics3 tags in MP3 audio files.
The core library supports netstandard20 and the full .NET 4.x frameworks.

ID3.NET also provides an extensible metadata discovery framework that can discover specific ID3 frame data (like album art, lyrics, etc.) from various online services such as Amazon, ChartLyrics.com, Discogs and more.

## ID3 Support
Currently, ID3.NET supports the two most popular ID3 versions, v1.x and v2.3.

The v1.x support applies to both v1.0 and v1.1, as the difference between the two versions is very small.

While ID3.NET can read v2.3 tags, it does not recognize all v2.3 frames yet. Unrecognized frames are stored in a special `UnknownFrame` class and do not raise exceptions. Most of the commonly-used frames such as title, album, artist, etc. are implemented and we are actively working on completing the remaining frame support. This will be done in the ID3.NET v0.x version range, culminating in a v1.0 release with full ID3 v2.3 frame support.
You can track the progress of frame implementation and see the list of currently supported frames [here](https://github.com/JeevanJames/Id3/wiki/Supported-ID3-v2.3-frames).

We will start on ID3 v2.2 and v2.4 tag support as soon as the v2.3 codebase is done.
Please see the [project roadmap](https://github.com/JeevanJames/Id3/wiki/Project-Roadmap) for more details.

## Examples

### Reading ID3 tags
Reads all .mp3 files in a directory and outputs their title, artist and album details to the console.
```cs
string[] musicFiles = Directory.GetFiles(@"C:\Music", "*.mp3");
foreach (string musicFile in musicFiles)
{
    using (var mp3 = new Mp3(musicFile))
    {
        Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
        Console.WriteLine("Title: {0}", tag.Title);
        Console.WriteLine("Artist: {0}", tag.Artists);
        Console.WriteLine("Album: {0}", tag.Album);
    }
}
```

Method to enumerate theough the specified MP3 files and return those from the 1980's.
```cs
IEnumerable<string> GetMusicFrom80s(IEnumerable<string> mp3FilePaths)
{
    foreach (var mp3FilePath in mp3FilePaths)
    {
        using (var mp3 = new Mp3(mp3FilePath))
        {
            Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
            if (!tag.Year.HasValue)
                continue;
            if (tag.Year >= 1980 && tag.Year < 1990)
                yield return mp3FilePath;
        }
    }
}
```

### Writing ID3 tags
Method to write a generic copyright message to the ID3 tag, if one does not exist.
```cs
void SetCopyright(string mp3FilePath)
{
    using (var mp3 = new Mp3(mp3FilePath, Mp3Permissions.ReadWrite))
    {
        Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
        if (!tag.Copyright.IsAssigned)
        {
            int year = tag.Year.GetValueOrDefault(2000);
            string artists = tag.Artists.ToString();
            tag.Copyright = $"{year} {artists}";
            mp3.WriteTag(tag, WriteConflictAction.Replace);
        }
    }
}
```
