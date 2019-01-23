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
using System.Runtime.Serialization.Formatters.Binary;
using Id3.Frames;
using Id3.Serialization;

using Xunit;

namespace Id3.Net.Tests
{
    public sealed class SerializationTests
    {
        [Fact]
        public void Serialize_deserialize_test()
        {
            var tag = new Id3Tag {
                Title = "There Will Never Be Another Tonight",
                Album = "Waking up the neighbors",
                Track = new TrackFrame(9, 15),
                Year = 1991,
                Genre = "Hard Rock",
                Publisher = "A&M",
                RecordingDate = new DateTime(1991, 03, 01)
            };
            tag.Artists.Value.Add("Bryan Adams");
            tag.Composers.Value.Add("Bryan Adams");
            tag.Composers.Value.Add("Robert Lange");
            tag.Composers.Value.Add("James Vallance");

            var stream = new MemoryStream();

            var serializer = new BinaryFormatter();
            serializer.IncludeId3SerializationSupport();
            serializer.Serialize(stream, tag);

            stream.Seek(0, SeekOrigin.Begin);

            var deserializer = new BinaryFormatter();
            deserializer.IncludeId3SerializationSupport();
            var clonedTag = (Id3Tag)deserializer.Deserialize(stream);

            Assert.NotSame(tag, clonedTag);

            Assert.Equal(tag.Title.Value, clonedTag.Title.Value);
            Assert.Equal(tag.Album.Value, clonedTag.Album.Value);
            Assert.Equal(tag.Track.Value, clonedTag.Track.Value);
            Assert.Equal(tag.Year.Value, clonedTag.Year.Value);
            Assert.Equal(tag.Genre.Value, clonedTag.Genre.Value);
            Assert.Equal(tag.Publisher.Value, clonedTag.Publisher.Value);
            Assert.Equal(tag.RecordingDate.Value, clonedTag.RecordingDate.Value);

            Assert.Equal(tag.Artists.Value.Count, clonedTag.Artists.Value.Count);
            for (var i = 0; i < tag.Artists.Value.Count; i++)
                Assert.Equal(tag.Artists.Value[i], clonedTag.Artists.Value[i]);

            Assert.Equal(tag.Composers.Value.Count, clonedTag.Composers.Value.Count);
            for (var i = 0; i < tag.Composers.Value.Count; i++)
                Assert.Equal(tag.Composers.Value[i], clonedTag.Composers.Value[i]);
        }
    }
}
