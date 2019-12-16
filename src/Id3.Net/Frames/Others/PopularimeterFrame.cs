using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Id3.Frames
{
    public sealed class PopularimeterFrame : Id3Frame
    {
        public override bool IsAssigned => RatingRaw != 0;

        public byte RatingRaw => (byte)Rating;

        public Rating Rating { get; set; }

        public string Email { get; set; }

        public long PlayCounter { get; set; }

        public PopularimeterFrame() { }

        public PopularimeterFrame(Rating rating)
        {
            Rating = rating;
        }
    }

    public enum Rating
    {
        NotRated = 0,
        OneStar = 1,
        TwoStars = 64,
        ThreeStars = 128,
        FourStars = 196,
        FiveStars = 255
    }
}
