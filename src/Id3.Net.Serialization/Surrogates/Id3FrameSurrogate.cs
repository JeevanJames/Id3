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
    internal abstract class Id3FrameSurrogate<TFrame> : ISerializationSurrogate
        where TFrame : Id3Frame
    {
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            GetFrameData((TFrame)obj, info, context);
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return SetObjectData((TFrame)obj, info, context, selector);
        }

        protected abstract void GetFrameData(TFrame frame, SerializationInfo info, StreamingContext context);

        protected abstract TFrame SetObjectData(TFrame frame, SerializationInfo info, StreamingContext context, ISurrogateSelector selector);
    }
}