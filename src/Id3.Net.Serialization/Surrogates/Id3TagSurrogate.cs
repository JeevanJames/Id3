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

namespace Id3.Serialization.Surrogates
{
    internal sealed class Id3TagSurrogate : ISerializationSurrogate
    {
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var tag = (Id3Tag)obj;

            info.AddValue("Version", tag.Version);
            info.AddValue("Family", tag.Family);

            int frameCount = tag.GetCount(onlyAssignedFrames: false);
            info.AddValue("FrameCount", frameCount);

            var frameIndex = 0;
            foreach (Id3Frame frame in tag)
            {
                info.AddValue($"Frame{frameIndex}Type", frame.GetType().AssemblyQualifiedName);
                info.AddValue($"Frame{frameIndex}", frame);
                frameIndex++;
            }
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context,
            ISurrogateSelector selector)
        {
            var tag = (Id3Tag)obj;

            tag.Version = (Id3Version)info.GetByte("Version");
            tag.Family = (Id3TagFamily)info.GetByte("Family");

            int frameCount = info.GetInt32("FrameCount");
            for (var i = 0; i < frameCount; i++)
            {
                string frameTypeName = info.GetString($"Frame{i}Type");
                Type frameType = Type.GetType(frameTypeName, true, false);
                if (frameType == null)
                    continue;
                if (info.GetValue($"Frame{i}", frameType) is Id3Frame frame)
                    tag.AddUntypedFrame(frame);
            }

            return tag;
        }
    }
}
