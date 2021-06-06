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
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Id3.Frames;

namespace Id3.v1
{
    internal sealed class Id3V1Handler : Id3Handler
    {
        internal override async Task DeleteTag(Stream stream)
        {
            if (await HasTag(stream).ConfigureAwait(false))
            {
                stream.SetLength(stream.Length - 128);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        internal override async Task<byte[]> GetTagBytes(Stream stream)
        {
            if (!await HasTag(stream).ConfigureAwait(false))
                return null;

            stream.Seek(-128, SeekOrigin.End);
            byte[] tagBytes = new byte[128];
            await stream.ReadAsync(tagBytes, 0, 128).ConfigureAwait(false);
            return tagBytes;
        }

        internal override async Task<bool> HasTag(Stream stream)
        {
            if (stream.Length < 128)
                return false;

            stream.Seek(-128, SeekOrigin.End);
            byte[] magicBytes = new byte[3];
            await stream.ReadAsync(magicBytes, 0, 3).ConfigureAwait(false);
            string magic = TextEncodingHelper.GetDefaultString(magicBytes, 0, 3);
            return magic == "TAG";
        }

        internal override async Task<(Id3Tag Tag, object AdditionalData)> ReadTagWithAdditionalData(Stream stream)
        {
            if (!await HasTag(stream).ConfigureAwait(false))
                return (null, null);

            stream.Seek(-125, SeekOrigin.End);
            byte[] tagBytes = new byte[125];
            await stream.ReadAsync(tagBytes, 0, 125).ConfigureAwait(false);

            Id3Tag tag = CreateTag();
            tag.Title.Value = ReadTagString(tagBytes, 0, 30);
            tag.Artists.TextValue = ReadTagString(tagBytes, 30, 30);
            tag.Album.Value = ReadTagString(tagBytes, 60, 30);
            tag.Year.TextValue = ReadTagString(tagBytes, 90, 4);
            tag.Genre.Value = ReadTagString(tagBytes, 124, 1);
            string comment;
            if (tagBytes[122] == 0 && tagBytes[123] != 0)
            {
                comment = ReadTagString(tagBytes, 94, 28);
                tag.Track.Value = tagBytes[123];
            }
            else
            {
                comment = ReadTagString(tagBytes, 94, 30);
                tag.Track.Value = -1;
            }

            if (!string.IsNullOrEmpty(comment))
                tag.Comments.Add(new CommentFrame { Comment = comment });

            return (tag, null);
        }

        internal override async Task<bool> WriteTag(Stream stream, Id3Tag tag)
        {
            Encoding encoding = TextEncodingHelper.GetDefaultEncoding();

            byte[] bytes = new byte[128];
            encoding.GetBytes("TAG").CopyTo(bytes, 0);

            byte[] itemBytes;
            if (!string.IsNullOrEmpty(tag.Title.Value))
            {
                itemBytes = encoding.GetBytes(tag.Title.Value);
                Array.Copy(itemBytes, 0, bytes, 3, Math.Min(30, itemBytes.Length));
            }

            if (!string.IsNullOrEmpty(tag.Artists.TextValue))
            {
                itemBytes = encoding.GetBytes(tag.Artists.TextValue);
                Array.Copy(itemBytes, 0, bytes, 33, Math.Min(30, itemBytes.Length));
            }

            if (!string.IsNullOrEmpty(tag.Album.Value))
            {
                itemBytes = encoding.GetBytes(tag.Album.Value);
                Array.Copy(itemBytes, 0, bytes, 63, Math.Min(30, itemBytes.Length));
            }

            if (!string.IsNullOrEmpty(tag.Year.TextValue))
            {
                itemBytes = encoding.GetBytes(tag.Year.TextValue);
                Array.Copy(itemBytes, 0, bytes, 93, Math.Min(4, itemBytes.Length));
            }

            if (tag.Comments.Count > 0)
            {
                itemBytes = encoding.GetBytes(tag.Comments[0].Comment);
                int maxCommentLength = tag.Track.Value == -1 ? 30 : 28;
                Array.Copy(itemBytes, 0, bytes, 97, Math.Min(maxCommentLength, itemBytes.Length));
            }

            if (tag.Track.Value >= 0)
                bytes[126] = (byte)tag.Track.Value;

            if (await HasTag(stream).ConfigureAwait(false))
                stream.Seek(-128, SeekOrigin.End);
            else
                stream.Seek(0, SeekOrigin.End);

            await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

            return true;
        }

        internal override Id3TagFamily Family => Id3TagFamily.Version1X;

        internal override Id3Version Version => Id3Version.V1X;

        protected override void BuildFrameHandlers(FrameHandlers mappings)
        {
            mappings.Add<AlbumFrame>("Album", null, null);
            mappings.Add<ArtistsFrame>("Artist", null, null);
            mappings.Add<TitleFrame>("Title", null, null);
            mappings.Add<CommentFrame>("Comment", null, null);
            mappings.Add<TrackFrame>("Track", null, null);
            mappings.Add<YearFrame>("Year", null, null);
        }

        private static string ReadTagString(byte[] bytes, int index, int length)
        {
            int endIndex = ByteArrayHelper.LocateSequence(bytes, index, length, new byte[] { 0 });
            if (endIndex == -1 || endIndex <= index)
                endIndex = index + length;
            string tagString = TextEncodingHelper.GetDefaultString(bytes, index, endIndex - index).Trim();
            return tagString;
        }
    }
}
