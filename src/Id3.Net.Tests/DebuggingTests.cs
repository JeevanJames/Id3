using System;
using System.IO;

using Xunit;

namespace Id3.Net.Tests
{
    public sealed class DebuggingTests : IDisposable
    {
        private readonly Mp3 _mp3;

        public DebuggingTests()
        {
            var stream = new MemoryStream();
            _mp3 = new Mp3(stream, Mp3Permissions.ReadWrite);
        }

        [Fact]
        public void DebugTest()
        {
            var tag1 = new Id3Tag {
                Track = new TrackFrame(3, 10) { Padding = 3 }
            };
            _mp3.WriteTag(tag1, Id3Version.V23);

            Id3Tag tag2 = _mp3.GetTag(Id3Version.V23);
            tag2.Track.Padding = 4;
            Assert.Equal(Id3Version.V23, tag2.Version);
            Assert.Equal(Id3TagFamily.Version2X, tag2.Family);
            Assert.Equal("0003/0010", tag2.Track);
        }

        void IDisposable.Dispose()
        {
            _mp3.Dispose();
        }
    }
}
