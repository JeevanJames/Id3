#region --- License & Copyright Notice ---
/*
Copyright (c) 2005-2012 Jeevan James
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

namespace Id3.v2
{
    internal sealed partial class Id3V23Handler
    {
        #region Text and derived frame handling
        private Id3Frame DecodeAlbum(byte[] data)
        {
            return DecodeText<AlbumFrame>(data);
        }

        private static byte[] EncodeAlbum(Id3Frame frame)
        {
            return EncodeText<AlbumFrame>(frame);
        }

        private Id3Frame DecodeArtists(byte[] data)
        {
            return DecodeText<ArtistsFrame>(data);
        }

        private static byte[] EncodeArtists(Id3Frame frame)
        {
            return EncodeText<ArtistsFrame>(frame);
        }

        private Id3Frame DecodeBand(byte[] data)
        {
            return DecodeText<BandFrame>(data);
        }

        private static byte[] EncodeBand(Id3Frame frame)
        {
            return EncodeText<BandFrame>(frame);
        }

        private Id3Frame DecodeBeatsPerMinute(byte[] data)
        {
            return DecodeText<BeatsPerMinuteFrame>(data);
        }

        private static byte[] EncodeBeatsPerMinute(Id3Frame frame)
        {
            return EncodeText<BeatsPerMinuteFrame>(frame);
        }

        private Id3Frame DecodeComposers(byte[] data)
        {
            return DecodeText<ComposersFrame>(data);
        }

        private static byte[] EncodeComposers(Id3Frame frame)
        {
            return EncodeText<ComposersFrame>(frame);
        }

        private Id3Frame DecodeConductor(byte[] data)
        {
            return DecodeText<ConductorFrame>(data);
        }

        private static byte[] EncodeConductor(Id3Frame frame)
        {
            return EncodeText<ConductorFrame>(frame);
        }

        private static Id3Frame DecodeContentGroupDescription(byte[] data)
        {
            return DecodeText<ContentGroupDescriptionFrame>(data);
        }

        private static byte[] EncodeContentGroupDescription(Id3Frame frame)
        {
            return EncodeText<ContentGroupDescriptionFrame>(frame);
        }

        private Id3Frame DecodeCustomText(byte[] data)
        {
            return DecodeText<CustomTextFrame>(data);
        }

        private static byte[] EncodeCustomText(Id3Frame frame)
        {
            return EncodeText<CustomTextFrame>(frame);
        }

        private Id3Frame DecodeEncoder(byte[] data)
        {
            return DecodeText<EncoderFrame>(data);
        }

        private static byte[] EncodeEncoder(Id3Frame frame)
        {
            return EncodeText<EncoderFrame>(frame);
        }

        private static Id3Frame DecodeEncodingSettings(byte[] data)
        {
            return DecodeText<EncodingSettingsFrame>(data);
        }

        private static byte[] EncodeEncodingSettings(Id3Frame frame)
        {
            return EncodeText<EncodingSettingsFrame>(frame);
        }

        private Id3Frame DecodeFileOwner(byte[] data)
        {
            return DecodeText<FileOwnerFrame>(data);
        }

        private static byte[] EncodeFileOwner(Id3Frame frame)
        {
            return EncodeText<FileOwnerFrame>(frame);
        }

        private Id3Frame DecodeGenre(byte[] data)
        {
            return DecodeText<GenreFrame>(data);
        }

        private static byte[] EncodeGenre(Id3Frame frame)
        {
            return EncodeText<GenreFrame>(frame);
        }

        private Id3Frame DecodePublisher(byte[] data)
        {
            return DecodeText<PublisherFrame>(data);
        }

        private static byte[] EncodePublisher(Id3Frame frame)
        {
            return EncodeText<PublisherFrame>(frame);
        }

        private Id3Frame DecodeRecordingDate(byte[] data)
        {
            return DecodeText<RecordingDateFrame>(data);
        }

        private static byte[] EncodeRecordingDate(Id3Frame frame)
        {
            return EncodeText<RecordingDateFrame>(frame);
        }

        private static Id3Frame DecodeSubtitle(byte[] data)
        {
            return DecodeText<SubtitleFrame>(data);
        }

        private static byte[] EncodeSubtitle(Id3Frame frame)
        {
            return EncodeText<SubtitleFrame>(frame);
        }

        private Id3Frame DecodeTitle(byte[] data)
        {
            return DecodeText<TitleFrame>(data);
        }

        private static byte[] EncodeTitle(Id3Frame frame)
        {
            return EncodeText<TitleFrame>(frame);
        }

        private Id3Frame DecodeTrack(byte[] data)
        {
            return DecodeText<TrackFrame>(data);
        }

        private static byte[] EncodeTrack(Id3Frame frame)
        {
            return EncodeText<TrackFrame>(frame);
        }

        private static Id3Frame DecodeYear(byte[] data)
        {
            return DecodeText<YearFrame>(data);
        }

        private static byte[] EncodeYear(Id3Frame frame)
        {
            return EncodeText<YearFrame>(frame);
        }

        private static TFrame DecodeText<TFrame>(byte[] data)
            where TFrame : TextFrameBase, new()
        {
            var frame = new TFrame();

            byte encodingByte = data[0];
            string value;
            if (encodingByte == 0 || encodingByte == 1)
            {
                frame.EncodingType = (Id3TextEncoding) encodingByte;
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
        #endregion

        #region URL link and derived frame handling
        private static Id3Frame DecodeArtistUrl(byte[] data)
        {
            return DecodeUrlLink<ArtistUrlFrame>(data);
        }

        private static byte[] EncodeArtistUrl(Id3Frame frame)
        {
            return EncodeUrlLink<ArtistUrlFrame>(frame);
        }

        private static Id3Frame DecodeAudioFileUrl(byte[] data)
        {
            return DecodeUrlLink<AudioFileUrlFrame>(data);
        }

        private static byte[] EncodeAudioFileUrl(Id3Frame frame)
        {
            return EncodeUrlLink<AudioFileUrlFrame>(frame);
        }

        private static Id3Frame DecodeAudioSourceUrl(byte[] data)
        {
            return DecodeUrlLink<AudioSourceUrlFrame>(data);
        }

        private static byte[] EncodeAudioSourceUrl(Id3Frame frame)
        {
            return EncodeUrlLink<AudioSourceUrlFrame>(frame);
        }

        private static Id3Frame DecodeCommercialUrl(byte[] data)
        {
            return DecodeUrlLink<CommercialUrlFrame>(data);
        }

        private static byte[] EncodeCommercialUrl(Id3Frame frame)
        {
            return EncodeUrlLink<CommercialUrlFrame>(frame);
        }

        private static Id3Frame DecodeCopyrightUrl(byte[] data)
        {
            return DecodeUrlLink<CopyrightUrlFrame>(data);
        }

        private static byte[] EncodeCopyrightUrl(Id3Frame frame)
        {
            return EncodeUrlLink<CopyrightUrlFrame>(frame);
        }

        private static Id3Frame DecodePaymentUrl(byte[] data)
        {
            return DecodeUrlLink<PaymentUrlFrame>(data);
        }

        private static byte[] EncodePaymentUrl(Id3Frame frame)
        {
            return EncodeUrlLink<PaymentUrlFrame>(frame);
        }

        private static TFrame DecodeUrlLink<TFrame>(byte[] data)
            where TFrame : UrlLinkFrame, new()
        {
            var frame = new TFrame {Url = TextEncodingHelper.GetDefaultString(data, 0, data.Length)};
            return frame;
        }

        private static byte[] EncodeUrlLink<TFrame>(Id3Frame id3Frame)
            where TFrame : UrlLinkFrame
        {
            var frame = (TFrame) id3Frame;
            return frame.Url != null ? TextEncodingHelper.GetDefaultEncoding().GetBytes(frame.Url) : new byte[0];
        }
        #endregion

        private static Id3Frame DecodeComment(byte[] data)
        {
            var frame = new CommentFrame {EncodingType = (Id3TextEncoding) data[0]};

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
                (byte)frame.EncodingType
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
            var frame = new CustomUrlLinkFrame();

            frame.EncodingType = (Id3TextEncoding)data[0];
            byte[][] splitBytes = ByteArrayHelper.SplitBySequence(data, 1, data.Length - 1,
                TextEncodingHelper.GetSplitterBytes(frame.EncodingType));
            string url = null;
            if (splitBytes.Length > 1)
            {
                frame.Description = TextEncodingHelper.GetString(splitBytes[0], 0, splitBytes[0].Length, frame.EncodingType);
                url = TextEncodingHelper.GetDefaultString(splitBytes[1], 0, splitBytes[1].Length);
            } else if (splitBytes.Length == 1)
                url = TextEncodingHelper.GetDefaultString(splitBytes[0], 0, splitBytes[0].Length);
            frame.Url = url;

            return frame;
        }

        private static byte[] EncodeCustomUrlLink(Id3Frame id3Frame)
        {
            var frame = (CustomUrlLinkFrame)id3Frame;

            var bytes = new List<byte> {
                (byte)frame.EncodingType
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
            var frame = new LyricsFrame();

            frame.EncodingType = (Id3TextEncoding)data[0];

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
            var frame = new PictureFrame();

            frame.EncodingType = (Id3TextEncoding)data[0];

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
            byte[] description = ByteArrayHelper.GetBytesUptoSequence(data, currentPos, TextEncodingHelper.GetSplitterBytes(frame.EncodingType));
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
            var frame = (PictureFrame) id3Frame;

            var bytes = new List<byte> {
                (byte)frame.EncodingType
            };

            Encoding defaultEncoding = TextEncodingHelper.GetDefaultEncoding();
            bytes.AddRange(!string.IsNullOrEmpty(frame.MimeType) ? defaultEncoding.GetBytes(frame.MimeType) : defaultEncoding.GetBytes("image/"));

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
            frame.OwnerId = TextEncodingHelper.GetString(ownerIdBytes, 0, ownerIdBytes.Length, Id3TextEncoding.Iso8859_1);
            frame.Data = new byte[data.Length - ownerIdBytes.Length - splitterSequence.Length];
            Array.Copy(data, ownerIdBytes.Length + splitterSequence.Length, frame.Data, 0, frame.Data.Length);
            return frame;
        }

        private static byte[] EncodePrivate(Id3Frame id3Frame)
        {
            var frame = (PrivateFrame) id3Frame;

            var bytes = new List<byte>();
            bytes.AddRange(TextEncodingHelper.GetEncoding(Id3TextEncoding.Iso8859_1).GetBytes(frame.OwnerId));
            bytes.AddRange(TextEncodingHelper.GetSplitterBytes(Id3TextEncoding.Iso8859_1));
            bytes.AddRange(frame.Data ?? new byte[0]);
            return bytes.ToArray();
        }
    }
}