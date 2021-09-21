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

using System;
using System.Collections.Generic;
using System.Text;
using Id3.Frames;

namespace Id3.v2
{
    internal sealed partial class Id3V23Handler
    {
        // from: https://id3.org/id3v2.3.0#Text_information_frames
        // <Header for 'Text information frame', ID: "T000" - "TZZZ", excluding "TXXX" described in 4.2.2.>
        // Text encoding    $xx
        // Information      <text string according to encoding>

        private static TFrame DecodeText<TFrame>(byte[] data)
            where TFrame : TextFrameBase, new()
        {
            var frame = new TFrame();

            byte encodingByte = data[0];
            string value;
            if (encodingByte == 0 || encodingByte == 1)
            {
                frame.EncodingType = (Id3TextEncoding) encodingByte;
                int currentPos = 1;
                value = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);
            } else
            {
                frame.EncodingType = Id3TextEncoding.Iso8859_1;
                int currentPos = 0;
                value = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);
            }

            frame.TextValue = value;

            return frame;
        }

        private static byte[] EncodeText<TFrame>(Id3Frame id3Frame)
            where TFrame : TextFrameBase
        {
            var frame = (TFrame) id3Frame;
            Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            byte[] preamble = encoding.GetPreamble();
            byte[] textBytes = encoding.GetBytes(frame.TextValue);
            var data = new byte[1 + preamble.Length + textBytes.Length];
            data[0] = (byte) frame.EncodingType;
            preamble.CopyTo(data, 1);
            textBytes.CopyTo(data, preamble.Length + 1);
            return data;
        }


        // from: https://id3.org/id3v2.3.0#User_defined_text_information_frame
        // <Header for 'User defined text information frame', ID: "TXXX">
        // Text encoding    $xx
        // Description      <text string according to encoding> $00 (00)
        // Value            <text string according to encoding>

        private static CustomTextFrame DecodeCustomText(byte[] data)
        {
            var frame = new CustomTextFrame { EncodingType = (Id3TextEncoding)data[0] };

            int currentPos = 1;
            frame.Description = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);
            frame.TextValue = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);

            return frame;
        }

        private static byte[] EncodeCustomText(Id3Frame id3Frame)
        {
            var frame = (CustomTextFrame)id3Frame;

            var bytes = new List<byte> {
                (byte) frame.EncodingType
            };

            Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Description))
                bytes.AddRange(encoding.GetBytes(frame.Description));
            bytes.AddRange(TextEncodingHelper.GetTerminationBytes(frame.EncodingType));
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.TextValue))
                bytes.AddRange(encoding.GetBytes(frame.TextValue));

            return bytes.ToArray();
        }


        // from: https://id3.org/id3v2.3.0#URL_link_frames
        // <Header for 'URL link frame', ID: "W000" - "WZZZ", excluding "WXXX" described in 4.3.2.>
        // URL      <text string>

        private static TFrame DecodeUrlLink<TFrame>(byte[] data)
            where TFrame : UrlLinkFrame, new()
        {
            int currentPos = 0;
            var frame = new TFrame {Url = TextEncodingHelper.DecodeString(data, ref currentPos, Id3TextEncoding.Iso8859_1)};
            return frame;
        }

        private static byte[] EncodeUrlLink<TFrame>(Id3Frame id3Frame)
            where TFrame : UrlLinkFrame
        {
            var frame = (TFrame) id3Frame;
            return frame.Url != null ? TextEncodingHelper.GetDefaultEncoding().GetBytes(frame.Url) : new byte[0];
        }


        // from: https://id3.org/id3v2.3.0#Comments
        // <Header for 'Comment', ID: "COMM">
        // Text encoding            $xx
        // Language                 $xx xx xx
        // Short content descrip.   <text string according to encoding> $00 (00)
        // The actual text          <full text string according to encoding>

        private static Id3Frame DecodeComment(byte[] data)
        {
            var frame = new CommentFrame {EncodingType = (Id3TextEncoding) data[0]};

            string language = TextEncodingHelper.GetDefaultEncoding().GetString(data, 1, 3).ToLowerInvariant();
            if (!Enum.IsDefined(typeof(Id3Language), language))
                frame.Language = Id3Language.eng;
            else
                frame.Language = (Id3Language) Enum.Parse(typeof(Id3Language), language, true);

            int currentPos = 4;
            frame.Description = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);
            frame.Comment = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);

            return frame;
        }

        private static byte[] EncodeComment(Id3Frame id3Frame)
        {
            var frame = (CommentFrame) id3Frame;

            var bytes = new List<byte> {
                (byte) frame.EncodingType
            };

            bytes.AddRange(TextEncodingHelper.GetDefaultEncoding().GetBytes(frame.Language.ToString()));

            Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Description))
                bytes.AddRange(encoding.GetBytes(frame.Description));
            bytes.AddRange(TextEncodingHelper.GetTerminationBytes(frame.EncodingType));
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Comment))
                bytes.AddRange(encoding.GetBytes(frame.Comment));

            return bytes.ToArray();
        }


        // from: https://id3.org/id3v2.3.0#User_defined_URL_link_frame
        // <Header for 'User defined URL link frame', ID: "WXXX">
        // Text encoding    $xx
        // Description      <text string according to encoding> $00 (00)
        // URL              <text string>

        private static Id3Frame DecodeCustomUrlLink(byte[] data)
        {
            var frame = new CustomUrlLinkFrame {EncodingType = (Id3TextEncoding) data[0]};

            int currentPos = 1;
            frame.Description = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);
            frame.Url = TextEncodingHelper.DecodeString(data, ref currentPos, Id3TextEncoding.Iso8859_1);

            return frame;
        }

        private static byte[] EncodeCustomUrlLink(Id3Frame id3Frame)
        {
            var frame = (CustomUrlLinkFrame) id3Frame;

            var bytes = new List<byte> {
                (byte) frame.EncodingType
            };

            Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Description))
                bytes.AddRange(encoding.GetBytes(frame.Description));
            bytes.AddRange(TextEncodingHelper.GetTerminationBytes(frame.EncodingType));
            if (frame.Url != null)
                bytes.AddRange(TextEncodingHelper.GetDefaultEncoding().GetBytes(frame.Url));

            return bytes.ToArray();
        }


        // from: https://id3.org/id3v2.3.0#Unsychronised_lyrics.2Ftext_transcription
        // <Header for 'Unsynchronised lyrics/text transcription', ID: "USLT">
        // Text encoding        $xx
        // Language             $xx xx xx
        // Content descriptor   <text string according to encoding> $00 (00)
        // Lyrics/text          <full text string according to encoding>

        private static Id3Frame DecodeLyrics(byte[] data)
        {
            var frame = new LyricsFrame {EncodingType = (Id3TextEncoding) data[0]};

            string language = TextEncodingHelper.GetDefaultEncoding().GetString(data, 1, 3).ToLowerInvariant();
            if (!Enum.IsDefined(typeof(Id3Language), language))
                frame.Language = Id3Language.eng;
            else
                frame.Language = (Id3Language) Enum.Parse(typeof(Id3Language), language, true);

            int currentPos = 4;
            frame.Description = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);
            frame.Lyrics = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);

            return frame;
        }

        private static byte[] EncodeLyrics(Id3Frame id3Frame)
        {
            var frame = (LyricsFrame)id3Frame;

            var bytes = new List<byte> {
                (byte) frame.EncodingType
            };

            bytes.AddRange(TextEncodingHelper.GetDefaultEncoding().GetBytes(frame.Language.ToString()));

            Encoding encoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Description))
                bytes.AddRange(encoding.GetBytes(frame.Description));
            bytes.AddRange(TextEncodingHelper.GetTerminationBytes(frame.EncodingType));
            bytes.AddRange(encoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Lyrics))
                bytes.AddRange(encoding.GetBytes(frame.Lyrics));

            return bytes.ToArray();
        }


        // from: https://id3.org/id3v2.3.0#Attached_picture
        // <Header for 'Attached picture', ID: "APIC">
        // Text encoding    $xx
        // MIME type        <text string> $00
        // Picture type     $xx
        // Description      <text string according to encoding> $00 (00)
        // Picture data     <binary data>

        private static Id3Frame DecodePicture(byte[] data)
        {
            var frame = new PictureFrame {EncodingType = (Id3TextEncoding) data[0]};

            int currentPos = 1;
            frame.MimeType = TextEncodingHelper.DecodeString(data, ref currentPos, Id3TextEncoding.Iso8859_1);
            if (frame.MimeType == null)
            {
                frame.MimeType = "image/";
                return frame;
            }

            frame.PictureType = (PictureType) data[currentPos];
            currentPos++;

            frame.Description = TextEncodingHelper.DecodeString(data, ref currentPos, frame.EncodingType);

            if (currentPos < data.Length)
            {
                frame.PictureData = new byte[data.Length - currentPos];
                Array.Copy(data, currentPos, frame.PictureData, 0, frame.PictureData.Length);
            }

            return frame;
        }

        private static byte[] EncodePicture(Id3Frame id3Frame)
        {
            var frame = (PictureFrame) id3Frame;

            var bytes = new List<byte> {
                (byte) frame.EncodingType
            };

            Encoding defaultEncoding = TextEncodingHelper.GetDefaultEncoding();
            bytes.AddRange(!string.IsNullOrEmpty(frame.MimeType)
                ? defaultEncoding.GetBytes(frame.MimeType)
                : defaultEncoding.GetBytes("image/"));

            bytes.Add(0);
            bytes.Add((byte) frame.PictureType);

            Encoding descriptionEncoding = TextEncodingHelper.GetEncoding(frame.EncodingType);
            bytes.AddRange(descriptionEncoding.GetPreamble());
            if (!string.IsNullOrEmpty(frame.Description))
                bytes.AddRange(descriptionEncoding.GetBytes(frame.Description));
            bytes.AddRange(TextEncodingHelper.GetTerminationBytes(frame.EncodingType));

            if (frame.PictureData != null && frame.PictureData.Length > 0)
                bytes.AddRange(frame.PictureData);

            return bytes.ToArray();
        }


        // from: https://id3.org/id3v2.3.0#Private_frame
        // <Header for 'Private frame', ID: "PRIV">
        // Owner identifier     <text string> $00
        // The private data     <binary data>

        private static Id3Frame DecodePrivate(byte[] data)
        {
            var frame = new PrivateFrame();

            int currentPos = 0;
            frame.OwnerId = TextEncodingHelper.DecodeString(data, ref currentPos, Id3TextEncoding.Iso8859_1);

            frame.Data = new byte[data.Length - currentPos];
            Array.Copy(data, currentPos, frame.Data, 0, frame.Data.Length);

            return frame;
        }

        private static byte[] EncodePrivate(Id3Frame id3Frame)
        {
            var frame = (PrivateFrame) id3Frame;

            var bytes = new List<byte>();
            bytes.AddRange(TextEncodingHelper.GetEncoding(Id3TextEncoding.Iso8859_1).GetBytes(frame.OwnerId));
            bytes.AddRange(TextEncodingHelper.GetTerminationBytes(Id3TextEncoding.Iso8859_1));
            bytes.AddRange(frame.Data ?? new byte[0]);
            return bytes.ToArray();
        }
    }
}
