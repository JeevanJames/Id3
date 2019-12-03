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

        //todo temp integrtion test
        [Fact]
        public void TodoTempVerifyActualFile()
        {
            //arrange
            var path = @"C:\_Temp\rating test\01~K.A.N. - Rating.mp3";
            var mp3 = new Mp3(path, Mp3Permissions.ReadWrite);
            var tag = mp3.GetTag(Id3TagFamily.Version2X);

            //act 
            var email = tag.Popularimeter.Email;
            var playCount = tag.Popularimeter.PlayCounter;
            tag.Popularimeter.Rating = Rating.FiveStars;
            mp3.WriteTag(tag, Id3Version.V23);
            mp3.Dispose();

            //assert
            var mp3Actual = new Mp3(path, Mp3Permissions.Read);
            var tagActual = mp3Actual.GetTag(Id3TagFamily.Version2X);
            Assert.Equal(Rating.FiveStars, tagActual.Popularimeter.Rating);
            Assert.Equal(playCount, tagActual.Popularimeter.PlayCounter);
            Assert.Equal(email, tag.Popularimeter.Email);
        }

        //[Fact]
        //public void ReadABunchOfMp3sAndSeeIfItFails()
        //{
        //    //arrange
        //    var start = @"C:\Users\mdepouw\OneDrive\Music\DJ Freckles\DJ Freckles\DJ Freckles 2pac's GH";
        //    start = @"C:\Users\mdepouw\OneDrive\Music\_me music attempt 2\DMX\_..And Then There Was X";
        //    start = @"C:\Users\mdepouw\OneDrive\Music\_me music attempt 2\";

        //    string[] musicFiles = Directory.GetFiles(start, "*.mp3", SearchOption.AllDirectories);
        //    foreach (string musicFile in musicFiles)
        //    {
        //        try
        //        {
        //            //act
        //            using (var mp3 = new Mp3(musicFile, Mp3Permissions.Read))
        //            {
        //                var tag = mp3.GetTag(Id3TagFamily.Version2X);
        //                if (tag == null)
        //                    continue;

        //                Func<Rating, string> starRating = (rating) =>
        //                {
        //                    var map = new Dictionary<Rating, string>()
        //                    {
        //                    { Rating.OneStar,    "⭐" },
        //                    { Rating.TwoStars,   "⭐⭐" },
        //                    { Rating.ThreeStars, "⭐⭐⭐" },
        //                    { Rating.FourStars,  "⭐⭐⭐⭐" },
        //                    { Rating.FiveStars,  "⭐⭐⭐⭐⭐" },
        //                    };
        //                    return map.TryGetValue(rating, out var stars) ? stars : "Not Rated";
        //                };
        //                //output.WriteLine($"Rating: {starRating(tag.Popularimeter.Rating)}");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            output.WriteLine($"Error processing: {musicFile} - error: {ex.Message}");
        //        }
        //    }

        //    //assert
        //    Assert.True(true);
        //}
        ////todo temp integrtion test
        //[Fact]
        //public void TodoTempFreshStart()
        //{
        //    //arrange
        //    var path = @"C:\_Temp\rating test\01~K.A.N. - Rating - Copy.mp3";
        //    var mp3 = new Mp3(path, Mp3Permissions.ReadWrite);
        //    var tag = mp3.GetTag(Id3TagFamily.Version2X);

        //    //act 
        //    tag.Popularimeter = new PopularimeterFrame(Rating.FiveStars);
        //    mp3.WriteTag(tag, Id3Version.V23);

        //    //assert
        //    Assert.True(false);
        //}

        //[Fact]
        //public void TodoTempAfterFreshStart()
        //{
        //    //arrange
        //    var path = @"C:\_Temp\rating test\01~K.A.N. - Rating - Copy - Copy.mp3";
        //    var mp3 = new Mp3(path, Mp3Permissions.ReadWrite);
        //    var tag = mp3.GetTag(Id3TagFamily.Version2X);

        //    //act 
        //    tag.Popularimeter.Rating = Rating.FiveStars;
        //    mp3.WriteTag(tag, Id3Version.V23);

        //    //assert
        //    Assert.True(false);
        //}
    }
}
