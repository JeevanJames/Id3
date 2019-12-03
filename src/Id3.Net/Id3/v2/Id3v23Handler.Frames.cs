#region --- License & Copyright Notice ---
/*
Copyright (c) 2005-2019 Jeevan James
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
using System.Collections.Generic;
using System.Text;

namespace Id3.v2
{
    internal sealed partial class Id3V23Handler
    {
        private static TFrame DecodeText<TFrame>(byte[] data)
            where TFrame : TextFrameBase, new()
        {
            var frame = new TFrame();

            byte encodingByte = data[0];
            string value;
            if (encodingByte == 0 || encodingByte == 1)
            {
                frame.EncodingType = (Id3TextEncoding)encodingByte;
                Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
                value = encoding.GetString(data, 1, data.Length - 1);
                if (value.Length > 0 && frame.EncodingType == Id3TextEncoding.Unicode &&
                    (value[0] == '\xFFFE' || value[0] == '\xFEFF'))
                    value = value.Remove(0, 1);
            }
            else
            {
                frame.EncodingType = Id3TextEncoding.Iso8859_1;
                Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
                value = encoding.GetString(data, 0, data.Length);
            }

            frame.TextValue = value;

            return frame;
        }

        private static byte[] EncodeText<TFrame>(Id3Frame id3Frame)
            where TFrame : TextFrameBase
        {
            var frame = (TFrame)id3Frame;
            Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            byte[] preamble = encoding.GetPreamble();
            byte[] textBytes = encoding.GetBytes(frame.TextValue);
            var data = new byte[1 + preamble.Length + textBytes.Length];
            data[0] = (byte)frame.EncodingType;
            preamble.CopyTo(data, 1);
            textBytes.CopyTo(data, preamble.Length + 1);
            return data;
        }

        private static TFrame DecodeUrlLink<TFrame>(byte[] data)
            where TFrame : UrlLinkFrame, new()
        {
            var frame = new TFrame { Url = TextEncodingHelper.GetDefaultString(data, 0, data.Length) };
            return frame;
        }

        private static byte[] EncodeUrlLink<TFrame>(Id3Frame id3Frame)
            where TFrame : UrlLinkFrame
        {
            var frame = (TFrame)id3Frame;
            return frame.Url != null ? TextEncodingHelper.GetDefaultEncoding().GetBytes(frame.Url) : new byte[0];
        }

        private static Id3Frame DecodeComment(byte[] data)
        {
            var frame = new CommentFrame { EncodingType = (Id3TextEncoding)data[0] };

            string language = TextEncodingHelper.GetDefaultEncoding().GetString(data, 1, 3).ToLowerInvariant();
            if (!Enum.IsDefined(typeof(Id3Language), language))
                frame.Language = Id3Language.eng;
            else
                frame.Language = (Id3Language)Enum.Parse(typeof(Id3Language), language, true);

            string[] splitStrings = TextEncodingHelper.GetSplitStrings(data, 4, data.Length - 4, frame.EncodingType);
            if (splitStrings.Length > 1)
            {
                frame.Description = splitStrings[0];
                frame.Comment = splitStrings[1];
            }
            else if (splitStrings.Length == 1)
                frame.Comment = splitStrings[0];

            return frame;
        }

        private static byte[] EncodeComment(Id3Frame id3Frame)
        {
            var frame = (CommentFrame)id3Frame;

            var bytes = new List<byte> {
                (byte) frame.EncodingType
            };

            bytes.AddRange(TextEncodingHelper.GetDefaultEncoding().GetBytes(frame.Language.ToString()));

            Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Description))
                bytes.AddRange(encoding.GetBytes(frame.Description));
            bytes.AddRange(TextEncodingHelper.GetSplitterBytes(frame.EncodingType));
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Comment))
                bytes.AddRange(encoding.GetBytes(frame.Comment));

            return bytes.ToArray();
        }

        private static Id3Frame DecodeCustomUrlLink(byte[] data)
        {
            var frame = new CustomUrlLinkFrame { EncodingType = (Id3TextEncoding)data[0] };

            byte[][] splitBytes = ByteArrayHelper.SplitBySequence(data, 1, data.Length - 1,
                TextEncodingHelper.GetSplitterBytes(frame.EncodingType));
            string url = null;
            if (splitBytes.Length > 1)
            {
                frame.Description =
                    TextEncodingHelper.GetString(splitBytes[0], 0, splitBytes[0].Length, frame.EncodingType);
                url = TextEncodingHelper.GetDefaultString(splitBytes[1], 0, splitBytes[1].Length);
            }
            else if (splitBytes.Length == 1)
                url = TextEncodingHelper.GetDefaultString(splitBytes[0], 0, splitBytes[0].Length);

            frame.Url = url;

            return frame;
        }

        private static byte[] EncodeCustomUrlLink(Id3Frame id3Frame)
        {
            var frame = (CustomUrlLinkFrame)id3Frame;

            var bytes = new List<byte> {
                (byte) frame.EncodingType
            };

            Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Description))
                bytes.AddRange(encoding.GetBytes(frame.Description));
            bytes.AddRange(TextEncodingHelper.GetSplitterBytes(frame.EncodingType));
            if (frame.Url != null)
                bytes.AddRange(TextEncodingHelper.GetDefaultEncoding().GetBytes(frame.Url));

            return bytes.ToArray();
        }

        private static Id3Frame DecodeLyrics(byte[] data)
        {
            var frame = new LyricsFrame { EncodingType = (Id3TextEncoding)data[0] };

            string language = TextEncodingHelper.GetDefaultEncoding().GetString(data, 1, 3).ToLowerInvariant();
            if (!Enum.IsDefined(typeof(Id3Language), language))
                frame.Language = Id3Language.eng;
            else
                frame.Language = (Id3Language)Enum.Parse(typeof(Id3Language), language, true);

            string[] splitStrings = TextEncodingHelper.GetSplitStrings(data, 4, data.Length - 4, frame.EncodingType);
            if (splitStrings.Length > 1)
            {
                frame.Description = splitStrings[0];
                frame.Lyrics = splitStrings[1];
            }
            else if (splitStrings.Length == 1)
                frame.Lyrics = splitStrings[0];

            return frame;
        }

        private static byte[] EncodeLyrics(Id3Frame id3Frame)
        {
            throw new NotImplementedException();
        }

        private static Id3Frame DecodePicture(byte[] data)
        {
            var frame = new PictureFrame { EncodingType = (Id3TextEncoding)data[0] };

            byte[] mimeType = ByteArrayHelper.GetBytesUptoSequence(data, 1, new byte[] { 0x00 });
            if (mimeType == null)
            {
                frame.MimeType = "image/";
                return frame;
            }

            frame.MimeType = TextEncodingHelper.GetDefaultString(mimeType, 0, mimeType.Length);

            int currentPos = mimeType.Length + 2;
            frame.PictureType = (PictureType)data[currentPos];

            currentPos++;
            byte[] description = ByteArrayHelper.GetBytesUptoSequence(data, currentPos,
                TextEncodingHelper.GetSplitterBytes(frame.EncodingType));
            if (description == null)
                return frame;
            frame.Description = TextEncodingHelper.GetString(description, 0, description.Length, frame.EncodingType);

            currentPos += description.Length + TextEncodingHelper.GetSplitterBytes(frame.EncodingType).Length;
            frame.PictureData = new byte[data.Length - currentPos];
            Array.Copy(data, currentPos, frame.PictureData, 0, frame.PictureData.Length);

            return frame;
        }

        private static byte[] EncodePicture(Id3Frame id3Frame)
        {
            var frame = (PictureFrame)id3Frame;

            var bytes = new List<byte> {
                (byte) frame.EncodingType
            };

            Encoding defaultEncoding = TextEncodingHelper.GetDefaultEncoding();
            bytes.AddRange(!string.IsNullOrEmpty(frame.MimeType)
                ? defaultEncoding.GetBytes(frame.MimeType)
                : defaultEncoding.GetBytes("image/"));

            bytes.Add(0);
            bytes.Add((byte)frame.PictureType);

            Encoding descriptionEncoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            bytes.AddRange(descriptionEncoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Description))
                bytes.AddRange(descriptionEncoding.GetBytes(frame.Description));
            bytes.AddRange(TextEncodingHelper.GetSplitterBytes(frame.EncodingType));

            if (frame.PictureData != null && frame.PictureData.Length > 0)
                bytes.AddRange(frame.PictureData);

            return bytes.ToArray();
        }

        private static Id3Frame DecodePrivate(byte[] data)
        {
            var frame = new PrivateFrame();
            byte[] splitterSequence = TextEncodingHelper.GetSplitterBytes(Id3TextEncoding.Iso8859_1);
            byte[] ownerIdBytes = ByteArrayHelper.GetBytesUptoSequence(data, 0, splitterSequence);
            frame.OwnerId =
                TextEncodingHelper.GetString(ownerIdBytes, 0, ownerIdBytes.Length, Id3TextEncoding.Iso8859_1);
            frame.Data = new byte[data.Length - ownerIdBytes.Length - splitterSequence.Length];
            Array.Copy(data, ownerIdBytes.Length + splitterSequence.Length, frame.Data, 0, frame.Data.Length);
            return frame;
        }

        private static byte[] EncodePrivate(Id3Frame id3Frame)
        {
            var frame = (PrivateFrame)id3Frame;

            var bytes = new List<byte>();
            bytes.AddRange(TextEncodingHelper.GetEncoding(Id3TextEncoding.Iso8859_1).GetBytes(frame.OwnerId));
            bytes.AddRange(TextEncodingHelper.GetSplitterBytes(Id3TextEncoding.Iso8859_1));
            bytes.AddRange(frame.Data ?? new byte[0]);
            return bytes.ToArray();
        }

        //todo shouldn't these methods be in their own class??
        private static Id3Frame DecodePopularimeter(byte[] data)
        {
            //todo review/revisit
            //seems like there's no way to encode the type in this frame
            //var frame = new PopularimeterFrame { EncodingType = (Id3TextEncoding)data[0] };

            //going to hard code the encoding type until I know more
            var frame = new PopularimeterFrame();// { EncodingType = Id3TextEncoding.Iso8859_1 };

            //todo what's the "right" want to do this???
            //read until we read the delimiter byte, 0, right?
            var startIndex = 1; //was 1 when using first byte for encoding type
            startIndex = 0;

            //optional email
            var emailBytes = ByteArrayHelper.GetBytesUptoSequence(data, startIndex, new byte[] { 0x00 });
            if (emailBytes.Length > 0)
                frame.Email = TextEncodingHelper.GetDefaultEncoding()
                    .GetString(emailBytes);

            //rating index
            var ratingIndex = startIndex + emailBytes.Length + 1; //+1 is for the field 'delimiter'
            frame.Rating = (Rating)data[ratingIndex];

            //optional player count
            if (data.Length > (ratingIndex + 1))
            {
                var bytesRemaining = data.Length - ratingIndex - 1;
                if (bytesRemaining == 4)
                    frame.PlayCounter = BitConverter.ToInt32(data, ratingIndex + 1);
                else if (bytesRemaining == 8)
                    frame.PlayCounter = BitConverter.ToInt64(data, ratingIndex + 1);
                else
                    throw new Exception($"unknown number of bytes to process: {bytesRemaining}");
            }

            return frame;
        }

        private static byte[] EncodePopularimeter(Id3Frame id3Frame)
        {
            var frame = (PopularimeterFrame)id3Frame;

            var bytes = new List<byte>();
            //{
            //    (byte) frame.EncodingType
            //};

            //Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType)
            //    ?? throw new Exception($"Unknown encoding type: {frame.EncodingType}");
            var encoding = TextEncodingHelper.GetEncoding(Id3TextEncoding.Iso8859_1);

            if (!string.IsNullOrEmpty(frame.Email))
                bytes.AddRange(encoding.GetBytes(frame.Email));

            //todo what is the point of preambles???
            //var debug = encoding.GetPreamble();
            //bytes.AddRange(encoding.GetPreamble());
            bytes.Add(0);

            bytes.Add(frame.RatingRaw);

            //advanced play counter
            //When the counter reaches all one's, one byte is
            //inserted in front of the counter thus making the counter eight bits
            //bigger in the same away as the play counter("PCNT")

            //4.16.   Play counter  PCNT
            // When the counter reaches all one's, one byte is inserted in front of the
            //counter thus making the counter eight bits bigger.  The counter must
            //be at least 32 - bits long to begin with.
            if(frame.PlayCounter <= int.MaxValue)
                //todo is this the "standard" way to convert from int -> bytes in this project???
                bytes.AddRange(BitConverter.GetBytes((int)frame.PlayCounter));
            else
                //todo is this the "standard" way to convert from long -> bytes in this project???
                bytes.AddRange(BitConverter.GetBytes(frame.PlayCounter));

            return bytes.ToArray();
        }
    }
}
