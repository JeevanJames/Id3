#region --- License & Copyright Notice ---
/*
Copyright (c) 2005-2018 Jeevan James
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

using System;
using System.IO;
using Id3.Frames;
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
                Track = new TrackFrame(3, 10) { Padding = 3 },
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
