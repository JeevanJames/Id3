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
using System.IO;

namespace Id3
{
    //Represents an ID3 handler, which can manipulate ID3 tags in an MP3 file stream.
    //There is a derived class for each version of ID3 supported by this framework.
    internal abstract class Id3Handler
    {
        //Specifies the details of each frame supported by the handler, including information on how
        //to encode and decode them. This structure is built by derived handlers by overridding the
        //BuildFrameHandlers method.
        private FrameHandlers _frameHandlers;

        internal Id3Tag CreateTag()
        {
            var tag = new Id3Tag {
                MajorVersion = MajorVersion,
                MinorVersion = MinorVersion,
                Family = Family
            };
            return tag;
        }

        internal Id3Frame GetFrameFromFrameId(string frameId)
        {
            FrameHandler handler = FrameHandlers[frameId];
            if (handler != null)
                return (Id3Frame)Activator.CreateInstance(handler.Type);
            return new UnknownFrame {
                Id = frameId
            };
        }

        internal string GetFrameIdFromFrame(Id3Frame frame)
        {
            var unknownFrame = frame as UnknownFrame;
            if (unknownFrame != null)
                return unknownFrame.Id;

            Type frameType = frame.GetType();

            FrameHandler handler = FrameHandlers[frameType];
            return handler != null ? handler.FrameId : null;
        }

        //Stream-manipulation overrides for the handler
        internal abstract void DeleteTag(Stream stream);
        internal abstract byte[] GetTagBytes(Stream stream);
        internal abstract bool HasTag(Stream stream);
        internal abstract Id3Tag ReadTag(Stream stream);
        internal abstract bool WriteTag(Stream stream, Id3Tag tag);

        //ID3 tag properties for the handler
        internal abstract Id3TagFamily Family { get; }
        internal abstract int MajorVersion { get; }
        internal abstract int MinorVersion { get; }

        //Override this in each derived handler to specify the valid frame types for the handler and
        //the frame IDs to which they correspond.
        protected abstract void BuildFrameHandlers(FrameHandlers mappings);

        protected FrameHandlers FrameHandlers
        {
            get
            {
                if (_frameHandlers == null)
                {
                    _frameHandlers = new FrameHandlers();
                    BuildFrameHandlers(_frameHandlers);
                }
                return _frameHandlers;
            }
        }
    }
}