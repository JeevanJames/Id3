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

using Id3.Frames;
using System.Runtime.Serialization;

namespace Id3.Serialization.Surrogates
{
    internal sealed class PopularimeterFrameSurrogate : Id3FrameSurrogate<PopularimeterFrame>
    {
        protected override void GetFrameData(PopularimeterFrame frame, SerializationInfo info, StreamingContext context)
        {
            //info.AddValue("EncodingType", frame.EncodingType);
            info.AddValue(nameof(PopularimeterFrame.RatingRaw), frame.RatingRaw);
            info.AddValue(nameof(PopularimeterFrame.Email), "ID3.NET");  //todo revisit
            info.AddValue(nameof(PopularimeterFrame.PlayCounter), frame.PlayCounter);
        }

        protected override PopularimeterFrame SetObjectData(PopularimeterFrame frame, SerializationInfo info, StreamingContext context,
            ISurrogateSelector selector)
        {
            //frame.EncodingType = (Id3TextEncoding)info.GetByte("EncodingType");
            frame.Rating = (Rating)info.GetByte(nameof(PopularimeterFrame.RatingRaw));
            frame.PlayCounter = info.GetInt32(nameof(PopularimeterFrame.PlayCounter));
            frame.Email = info.GetString(nameof(PopularimeterFrame.Email));
            return frame;
        }
    }
}