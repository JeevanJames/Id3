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

using System.Runtime.Serialization;
using Id3.Frames;

namespace Id3.Serialization.Surrogates
{
    internal sealed class TextFrameSurrogate : Id3FrameSurrogate<TextFrameBase>
    {
        protected override void GetFrameData(TextFrameBase frame, SerializationInfo info, StreamingContext context)
        {
            info.AddValue("EncodingType", frame.EncodingType);
            info.AddValue("TextValue", frame.TextValue);
        }

        protected override TextFrameBase SetObjectData(TextFrameBase frame, SerializationInfo info, StreamingContext context,
            ISurrogateSelector selector)
        {
            frame.EncodingType = (Id3TextEncoding)info.GetByte("EncodingType");
            frame.TextValue = info.GetString("TextValue");
            return frame;
        }
    }
}