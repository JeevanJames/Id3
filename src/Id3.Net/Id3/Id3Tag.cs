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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Id3
{
    public sealed class Id3Tag : IEnumerable<Id3Frame>, IComparable<Id3Tag>, IEquatable<Id3Tag>
    {
        #region Private fields
        //Specific single instance frames
        private AlbumFrame _album;
        private ArtistsFrame _artists;
        private AudioFileUrlFrame _audioFileUrl;
        private AudioSourceUrlFrame _audioSourceUrl;
        private BandFrame _band;
        private BeatsPerMinuteFrame _beatsPerMinute;
        private ComposersFrame _composers;
        private ConductorFrame _conductor;
        private ContentGroupDescriptionFrame _contentGroupDescription;
        private CopyrightFrame _copyright;
        private CopyrightUrlFrame _copyrightUrl;
        private EncoderFrame _encoder;
        private EncodingSettingsFrame _encodingSettings;
        private FileOwnerFrame _fileOwner;
        private FileTypeFrame _fileType;
        private GenreFrame _genre;
        private LengthFrame _length;
        private LyricistsFrame _lyricists;
        private PaymentUrlFrame _paymentUrl;
        private PublisherFrame _publisher;
        private RecordingDateFrame _recordingDate;
        private SubtitleFrame _subtitle;
        private TitleFrame _title;
        private TrackFrame _track;
        private YearFrame _year;

        //Specific multiple instance frames
        private ArtistUrlFrameList _artistUrls;
        private CommentFrameList _comments;
        private CommercialUrlFrameList _commercialUrls;
        private Id3SyncFrameList<CustomTextFrame> _customTexts;
        private Id3SyncFrameList<LyricsFrame> _lyrics;
        private Id3SyncFrameList<PictureFrame> _pictures;
        private PrivateFrameList _privateData;
        #endregion

        public Id3Tag()
        {
            IsSupported = true;
            Frames = new Id3FrameList();
        }

        [OnDeserializing]
        private void OnDeserialized(StreamingContext context)
        {
            IsSupported = true;
            Frames = new Id3FrameList();
        }

        //Converts an ID3 tag to another version after resolving the differences between the two
        //versions. The resultant tag will have all the frames from the source tag, but those
        //frames not recognized in the new version will be treated as UnknownFrame objects.
        //Similarly, frames recognized in the output tag version, but not in the source version are
        //converted accordingly.
        public Id3Tag ConvertTo(int majorVersion, int minorVersion)
        {
            if (MajorVersion == majorVersion && MinorVersion == minorVersion)
                return this;
            RegisteredId3Handler sourceHandler = Mp3.RegisteredHandlers.GetHandler(MajorVersion, MinorVersion);
            if (sourceHandler == null)
                return null;
            RegisteredId3Handler destinationHandler = Mp3.RegisteredHandlers.GetHandler(majorVersion, minorVersion);
            if (destinationHandler == null)
                return null;
            Id3Tag destinationTag = destinationHandler.Handler.CreateTag();
            foreach (Id3Frame sourceFrame in Frames)
            {
                var unknownFrame = sourceFrame as UnknownFrame;
                if (unknownFrame != null)
                {
                    string frameId = unknownFrame.Id;
                    Id3Frame destinationFrame = destinationHandler.Handler.GetFrameFromFrameId(frameId);
                    destinationTag.Frames.Add(destinationFrame);
                } else
                    destinationTag.Frames.Add(sourceFrame);
            }
            return destinationTag;
        }

        public void MergeWith(params Id3Tag[] tags)
        {
            Array.Sort(tags);
            //TODO:
        }

        #region Metadata properties
        public Id3TagFamily Family { get; internal set; }

        public int MajorVersion { get; internal set; }

        public int MinorVersion { get; internal set; }

        public bool IsSupported { get; internal set; }

        public object AdditionalData { get; internal set; }
        #endregion

        #region Main frames list and associated operations
        internal Id3FrameList Frames { get; private set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Id3Frame>)this).GetEnumerator();
        }

        IEnumerator<Id3Frame> IEnumerable<Id3Frame>.GetEnumerator()
        {
            return Frames.Where(frame => frame.IsAssigned).GetEnumerator();
        }

        /// <summary>
        ///     Removes all unassigned frames from the tag.
        /// </summary>
        public void Cleanup()
        {
            for (int i = Frames.Count - 1; i >= 0; i--)
            {
                if (!Frames[i].IsAssigned)
                    Frames.RemoveAt(i);
            }
        }

        public int Clear()
        {
            int clearedCount = this.Count();
            Frames.Clear();
            return clearedCount;
        }

        public bool Contains<TFrame>(Expression<Func<Id3Tag, TFrame>> frameProperty) where TFrame : Id3Frame
        {
            if (frameProperty == null)
                throw new ArgumentNullException("frameProperty");

            var lambda = (LambdaExpression)frameProperty;
            var memberExpression = (MemberExpression)lambda.Body;
            var property = (PropertyInfo)memberExpression.Member;
            return this.Any(f => f.GetType() == property.PropertyType && f.IsAssigned);
        }

        public bool Remove<TFrame>() where TFrame : Id3Frame
        {
            for (int i = 0; i < Frames.Count; i++)
            {
                Id3Frame frame = Frames[i];
                if (frame.IsAssigned && frame.GetType() == typeof(TFrame))
                {
                    Frames.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public int RemoveAll<TFrame>(Func<TFrame, bool> predicate = null)
            where TFrame : Id3Frame
        {
            int removalCount = 0;
            for (int i = Frames.Count - 1; i >= 0; i--)
            {
                var frame = Frames[i] as TFrame;
                if (frame != null && (predicate == null || predicate(frame)))
                {
                    if (frame.IsAssigned)
                        removalCount++;
                    Frames.RemoveAt(i);
                }
            }
            return removalCount;
        }
        #endregion

        #region Frame properties
        public AlbumFrame Album
        {
            get { return GetSingleFrame(ref _album); }
        }

        public ArtistsFrame Artists
        {
            get { return GetSingleFrame(ref _artists); }
        }

        public ArtistUrlFrameList ArtistUrls
        {
            get { return _artistUrls ?? (_artistUrls = new ArtistUrlFrameList(Frames)); }
        }

        public AudioFileUrlFrame AudioFileUrl
        {
            get { return GetSingleFrame(ref _audioFileUrl); }
        }

        public AudioSourceUrlFrame AudioSourceUrl
        {
            get { return GetSingleFrame(ref _audioSourceUrl); }
        }

        public BandFrame Band
        {
            get { return GetSingleFrame(ref _band); }
        }

        public BeatsPerMinuteFrame BeatsPerMinute
        {
            get { return GetSingleFrame(ref _beatsPerMinute); }
        }

        public CommentFrameList Comments
        {
            get { return _comments ?? (_comments = new CommentFrameList(Frames)); }
        }

        public CommercialUrlFrameList CommercialUrls
        {
            get { return _commercialUrls ?? (_commercialUrls = new CommercialUrlFrameList(Frames)); }
        }

        public ComposersFrame Composers
        {
            get { return GetSingleFrame(ref _composers); }
        }

        public ConductorFrame Conductor
        {
            get { return GetSingleFrame(ref _conductor); }
        }

        public ContentGroupDescriptionFrame ContentGroupDescription
        {
            get { return GetSingleFrame(ref _contentGroupDescription); }
        }

        public CopyrightFrame Copyright
        {
            get { return GetSingleFrame(ref _copyright); }
        }

        public CopyrightUrlFrame CopyrightUrl
        {
            get { return GetSingleFrame(ref _copyrightUrl); }
        }

        public Id3SyncFrameList<CustomTextFrame> CustomTexts
        {
            get { return GetMultipleFrames(ref _customTexts); }
        }

        public EncoderFrame Encoder
        {
            get { return GetSingleFrame(ref _encoder); }
        }

        public EncodingSettingsFrame EncodingSettings
        {
            get { return GetSingleFrame(ref _encodingSettings); }
        }

        public FileOwnerFrame FileOwner
        {
            get { return GetSingleFrame(ref _fileOwner); }
        }

        public FileTypeFrame FileType
        {
            get { return GetSingleFrame(ref _fileType); }
        }

        public GenreFrame Genre
        {
            get { return GetSingleFrame(ref _genre); }
        }

        public LengthFrame Length
        {
            get { return GetSingleFrame(ref _length); }
        }

        public LyricistsFrame Lyricists
        {
            get { return GetSingleFrame(ref _lyricists); }
        }

        public Id3SyncFrameList<LyricsFrame> Lyrics
        {
            get { return GetMultipleFrames(ref _lyrics); }
        }

        public PaymentUrlFrame PaymentUrl
        {
            get { return GetSingleFrame(ref _paymentUrl); }
        }

        public PublisherFrame Publisher
        {
            get { return GetSingleFrame(ref _publisher); }
        }

        public Id3SyncFrameList<PictureFrame> Pictures
        {
            get { return GetMultipleFrames(ref _pictures); }
        }

        public PrivateFrameList PrivateData
        {
            get { return _privateData ?? (_privateData = new PrivateFrameList(Frames)); }
        }

        public RecordingDateFrame RecordingDate
        {
            get { return GetSingleFrame(ref _recordingDate); }
        }

        public SubtitleFrame Subtitle
        {
            get { return GetSingleFrame(ref _subtitle); }
        }

        public TitleFrame Title
        {
            get { return GetSingleFrame(ref _title); }
        }

        public TrackFrame Track
        {
            get { return GetSingleFrame(ref _track); }
        }

        public YearFrame Year
        {
            get { return GetSingleFrame(ref _year); }
        }
        #endregion

        #region IComparable<Id3Tag> and IEquatable<Id3Tag> implementations
        /// <summary>
        /// Compares two tags based on their version details.
        /// </summary>
        /// <param name="other">The tag instance to compare against.</param>
        /// <returns>TODO:</returns>
        public int CompareTo(Id3Tag other)
        {
            if (other == null)
                return 1;
            int majorComparison = MajorVersion.CompareTo(other.MajorVersion);
            int minorComparison = MinorVersion.CompareTo(other.MinorVersion);
            if (majorComparison == 0 && minorComparison == 0)
                return 0;
            return majorComparison != 0 ? majorComparison : minorComparison;
        }

        public bool Equals(Id3Tag other)
        {
            return MajorVersion == other.MajorVersion && MinorVersion == other.MinorVersion;
        }
        #endregion

        #region Private helper methods
        //Retrieves a single-occuring frame from the main frames list. This method is called from
        //the corresponding property getters.
        //Since each frame already has private field declared, we simply need to get a reference
        //to that field, instead of creating a new object. However, if the field is not available,
        //we create a new one with default values, which is then assigned to the private field.
        //Hence the use of a ref parameter.
        private TFrame GetSingleFrame<TFrame>(ref TFrame frame) where TFrame : Id3Frame, new()
        {
            if (frame != null)
                return frame;
            frame = Frames.OfType<TFrame>().FirstOrDefault();
            if (frame == null)
            {
                frame = new TFrame();
                Frames.Add(frame);
            }
            return frame;
        }

        private Id3SyncFrameList<TFrame> GetMultipleFrames<TFrame>(ref Id3SyncFrameList<TFrame> frames) where TFrame : Id3Frame
        {
            return frames ?? (frames = new Id3SyncFrameList<TFrame>(Frames));
        }
        #endregion

        #region Static members
        public static readonly Id3Tag[] Empty = new Id3Tag[0];

        public static Id3Tag Merge(params Id3Tag[] tags)
        {
            if (tags.Length == 0)
                throw new ArgumentNullException("tags", "Specify 2 or more tags to merge");

            if (tags.Length == 1)
                return tags[0];

            var tag = new Id3Tag();
            tag.MergeWith(tags);
            return tag;
        }
        #endregion
    }
}