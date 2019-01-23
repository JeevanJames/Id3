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

using System;
using System.Runtime.Serialization;
using Id3.Frames;
using Id3.Serialization.Surrogates;

namespace Id3.Serialization
{
    public static class SerializationExtensions
    {
        /// <summary>
        ///     Registers all serialization surrogates defined in this assembly with the specified formatter.
        /// </summary>
        /// <typeparam name="TFormatter"> The type of formatter to register for. </typeparam>
        /// <param name="formatter"> The formatter to register for.</param>
        /// <returns> An instance of the formatter. </returns>
        public static TFormatter IncludeId3SerializationSupport<TFormatter>(this TFormatter formatter)
            where TFormatter : IFormatter
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var selector = new SurrogateSelector();

            //Id3Tag
            selector.AddSurrogate(typeof(Id3Tag), new StreamingContext(StreamingContextStates.All),
                new Id3TagSurrogate());

            //TextFrame-derived classes
            selector.AddFrameSurrogates<TextFrameSurrogate>(
                typeof(AlbumFrame),
                typeof(BandFrame),
                typeof(ConductorFrame),
                typeof(ContentGroupDescriptionFrame),
                typeof(CopyrightFrame),
                typeof(CustomTextFrame),
                typeof(EncoderFrame),
                typeof(EncodingSettingsFrame),
                typeof(FileOwnerFrame),
                typeof(GenreFrame),
                typeof(PublisherFrame),
                typeof(SubtitleFrame),
                typeof(TitleFrame)
            );

            //NumericFrame-derived classes
            selector.AddFrameSurrogates<TextFrameSurrogate>(
                typeof(BeatsPerMinuteFrame),
                typeof(YearFrame)
            );

            //DateTimeFrame-derived classes
            selector.AddFrameSurrogates<TextFrameSurrogate>(typeof(RecordingDateFrame));

            //ListTextFrame-derived classes
            selector.AddFrameSurrogates<TextFrameSurrogate>(
                typeof(ArtistsFrame),
                typeof(ComposersFrame),
                typeof(LyricistsFrame)
            );

            //Other TextFrameBase<>-derived classes
            selector.AddFrameSurrogates<TextFrameSurrogate>(
                typeof(FileTypeFrame),
                typeof(LengthFrame),
                typeof(TrackFrame)
            );

            //UrlLinkFrame-derived classes
            selector.AddFrameSurrogates<UrlLinkFrameSurrogate>(
                typeof(ArtistUrlFrame),
                typeof(AudioFileUrlFrame),
                typeof(AudioSourceUrlFrame),
                typeof(CommercialUrlFrame),
                typeof(CopyrightUrlFrame),
                typeof(CustomUrlLinkFrame),
                typeof(PaymentUrlFrame)
            );

            //All other frames
            selector.AddFrameSurrogate<CommentFrame, CommentFrameSurrogate>();
            selector.AddFrameSurrogate<LyricsFrame, LyricsFrameSurrogate>();
            selector.AddFrameSurrogate<PictureFrame, PictureFrameSurrogate>();
            selector.AddFrameSurrogate<PrivateFrame, PrivateFrameSurrogate>();
            selector.AddFrameSurrogate<UnknownFrame, UnknownFrameSurrogate>();

            formatter.SurrogateSelector = selector;
            return formatter;
        }
    }

    internal static class SurrogateSelectorExtensions
    {
        internal static SurrogateSelector AddFrameSurrogate<TFrame, TSurrogate>(this SurrogateSelector selector)
            where TFrame : Id3Frame
            where TSurrogate : ISerializationSurrogate, new()
        {
            selector.AddSurrogate(typeof(TFrame), new StreamingContext(StreamingContextStates.All), new TSurrogate());
            return selector;
        }

        internal static void AddFrameSurrogates<TSurrogate>(this SurrogateSelector selector, params Type[] frameTypes)
            where TSurrogate : ISerializationSurrogate, new()
        {
            foreach (Type frameType in frameTypes)
                selector.AddSurrogate(frameType, new StreamingContext(StreamingContextStates.All), new TSurrogate());
        }
    }
}
