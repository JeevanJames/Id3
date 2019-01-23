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

using Id3.Files;

using Xunit;

namespace Id3.Net.Tests
{
    public sealed class FileNamerTests
    {
        [Theory]
        [InlineData(@"C:\Users\Jeevan\SkyDrive\Music\Roxette\Roxette - 2006 - One Wish")]
        public void Test(string mp3Directory)
        {
            var namer = new FileNamer(
                "{track}. [{album}] {Title}",
                "{Track}. {Title}",
                "{Track}. {Album}"
            );
            //namer.ResolveMissingData += Namer_ResolveMissingData;
            RenameSuggestions suggestions = namer.GetSuggestions(mp3Directory);
        }

        private void Namer_ResolveMissingData(object sender, ResolveMissingDataEventArgs e)
        {
            e.Value = "Untitled";
        }
    }
}
