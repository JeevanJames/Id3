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
using System.Collections.ObjectModel;
using System.Linq;

namespace Id3
{
    /// <summary>
    ///     Represents the details of a frame and how it can be encoded or decoded.
    ///     Handlers use this information to process frames.
    /// </summary>
    internal sealed class FrameHandler
    {
        internal FrameHandler(string frameId, Type type, Func<Id3Frame, byte[]> encoder, Func<byte[], Id3Frame> decoder)
        {
            FrameId = frameId;
            Type = type;
            Encoder = encoder;
            Decoder = decoder;
        }

        internal string FrameId { get; }

        internal Type Type { get; }

        internal Func<Id3Frame, byte[]> Encoder { get; }

        internal Func<byte[], Id3Frame> Decoder { get; }
    }

    internal sealed class FrameHandlers : Collection<FrameHandler>
    {
        internal void Add<TFrame>(string frameId, Func<Id3Frame, byte[]> encoder, Func<byte[], Id3Frame> decoder)
            where TFrame : Id3Frame
        {
            Add(new FrameHandler(frameId, typeof(TFrame), encoder, decoder));
        }

        internal FrameHandler this[string frameId]
        {
            get { return this.FirstOrDefault(mapping => mapping.FrameId == frameId); }
        }

        internal FrameHandler this[Type type]
        {
            get { return this.FirstOrDefault(mapping => mapping.Type == type); }
        }
    }
}
