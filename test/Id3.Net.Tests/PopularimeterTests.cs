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

using Id3.Frames;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Id3.Net.Tests
{
    public sealed class PopularimeterTests : IDisposable
    {
        private readonly Mp3 mp3;
        private readonly ITestOutputHelper output;

        public PopularimeterTests(ITestOutputHelper output)
        {
            var stream = new MemoryStream();
            mp3 = new Mp3(stream, Mp3Permissions.ReadWrite);
            this.output = output;
        }

        void IDisposable.Dispose()
        {
            mp3.Dispose();
        }

        [Fact]
        public void RatingTest()
        {
            //arrange
            var tag = new Id3Tag
            {
                Popularimeter = new PopularimeterFrame(Rating.ThreeStars)
                //Album = new AlbumFrame("temp for debugging")
            };

            //act 
            mp3.WriteTag(tag, Id3Version.V23);

            //assert
            var actualTag = mp3.GetTag(Id3TagFamily.Version2X);
            Assert.Equal(Rating.ThreeStars, actualTag.Popularimeter.Rating);
        }

        [Fact]
        public void EmailTest()
        {
            //arrange
            var tag = new Id3Tag
            {
                Popularimeter = new PopularimeterFrame(Rating.ThreeStars)
                {
                    Email = "Hello World"
                }
            };

            //act 
            mp3.WriteTag(tag, Id3Version.V23);

            //assert
            var actualTag = mp3.GetTag(Id3TagFamily.Version2X);
            Assert.Equal("Hello World", actualTag.Popularimeter.Email);
        }

        [Fact]
        public void PlayCounterTest()
        {
            //arrange
            var tag = new Id3Tag
            {
                Popularimeter = new PopularimeterFrame(Rating.ThreeStars)
                {
                    PlayCounter = 2325
                }
            };

            //act 
            mp3.WriteTag(tag, Id3Version.V23);

            //assert
            var actualTag = mp3.GetTag(Id3TagFamily.Version2X);
            Assert.Equal(2325, actualTag.Popularimeter.PlayCounter);
        }

        [Fact]
        public void FullPopularimeterTest()
        {
            //arrange
            var tag = new Id3Tag
            {
                Popularimeter = new PopularimeterFrame(Rating.ThreeStars)
                {
                    PlayCounter = 23223,
                    Email = "unit-testing@fakedomain.com",
                    Rating = Rating.TwoStars
                }
            };

            //act 
            mp3.WriteTag(tag, Id3Version.V23);

            //assert
            var actualTag = mp3.GetTag(Id3TagFamily.Version2X);
            Assert.Equal(23223, actualTag.Popularimeter.PlayCounter);
            Assert.Equal("unit-testing@fakedomain.com", actualTag.Popularimeter.Email);
            Assert.Equal(Rating.TwoStars, actualTag.Popularimeter.Rating);
        }

        [Fact]
        public void OverIntMaxPlayCounterTest()
        {
            //arrange
            var tag = new Id3Tag
            {
                Popularimeter = new PopularimeterFrame(Rating.ThreeStars)
                {
                    PlayCounter = (long)int.MaxValue + 1
                }
            };

            //act 
            mp3.WriteTag(tag, Id3Version.V23);

            //assert
            var actualTag = mp3.GetTag(Id3TagFamily.Version2X);
            Assert.Equal((long)int.MaxValue + 1, actualTag.Popularimeter.PlayCounter);
        }
    }
}
