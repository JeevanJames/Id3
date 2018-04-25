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

using System.Diagnostics;

namespace Id3
{
    /// <summary>
    ///     Represents an ID3 frame that contains textual data
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public abstract class TextFrameBase : Id3Frame
    {
        public sealed override bool Equals(Id3Frame other)
        {
            return base.Equals(other) &&
                other is TextFrameBase text &&
                text.TextValue == TextValue;
        }

        public sealed override int GetHashCode()
        {
            return TextValue.GetHashCode();
        }

        public override string ToString()
        {
            return IsAssigned ? TextValue : string.Empty;
        }

        public Id3TextEncoding EncodingType { get; set; }

        public sealed override bool IsAssigned => !string.IsNullOrEmpty(TextValue);

        /// <summary>
        ///     Textual representation of the frame value. This is for internal usage only; derived classes should override the
        ///     getters and setters to get and set the natively-typed value in the <see cref="TextFrameBase{TValue}.Value" />
        ///     property.
        /// </summary>
        internal abstract string TextValue { get; set; }
    }

    public abstract class TextFrameBase<TValue> : TextFrameBase
    {
        private TValue _value;

        protected TextFrameBase()
        {
        }

        protected TextFrameBase(TValue value)
        {
            Value = value;
        }

        /// <summary>
        ///     Natively-typed value of the frame. Derived classes will override the <see cref="TextFrameBase.TextValue" /> to get
        ///     and set this value.
        /// </summary>
        public TValue Value
        {
            get => _value;
            set
            {
                ValidateValue(value);
                _value = value;
            }
        }

        protected virtual void ValidateValue(TValue value)
        {
        }

        public static implicit operator TValue(TextFrameBase<TValue> frame)
        {
            return frame.Value;
        }
    }
}
