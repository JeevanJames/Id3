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

namespace Id3.Frames
{
    /// <summary>
    ///     Represents an ID3 frame that contains textual data.
    /// </summary>
    public abstract class TextFrameBase : Id3Frame
    {
        public sealed override bool Equals(Id3Frame other)
        {
            return other is TextFrameBase text &&
                text.TextValue == TextValue;
        }

        public override string ToString()
        {
            return (IsAssigned ? TextValue : string.Empty) ?? string.Empty;
        }

        public Id3TextEncoding EncodingType { get; set; }

        public override bool IsAssigned => !string.IsNullOrEmpty(TextValue);

        /// <summary>
        ///     Gets or sets the textual representation of the frame value. This is for internal usage
        ///     only; derived classes should override the getters and setters to get and set the natively-typed
        ///     value in the <see cref="TextFrameBase{TValue}.Value" /> property.
        /// </summary>
        internal abstract string TextValue { get; set; }
    }
}
