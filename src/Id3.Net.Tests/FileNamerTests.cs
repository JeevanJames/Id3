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
