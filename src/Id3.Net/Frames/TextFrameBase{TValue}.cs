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

using System.Diagnostics;

namespace Id3.Frames
{
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public abstract class TextFrameBase<TValue> : TextFrameBase
    {
        private TValue _value = default!;

        protected TextFrameBase()
        {
        }

        protected TextFrameBase(TValue value)
        {
            Value = value;
        }

        /// <summary>
        ///     Gets or sets the natively-typed value of the frame. Derived classes will override the
        ///     <see cref="TextFrameBase.TextValue" /> to get and set this value.
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

        /// <summary>
        ///     Deriving classes can override this method to validate the native <paramref name="value"/>
        ///     being set.
        ///     <para />
        ///     If the value is invalid, the method should throw an <see cref="Id3Exception"/> exception.
        /// </summary>
        /// <remarks>
        ///     Note that in a lot of cases, a native value of null or something that translates to
        ///     an empty string is considered valid. In such cases, the frame may be unassigned, but
        ///     the value should still be allowed.
        /// </remarks>
        /// <param name="value">The native value being set.</param>
        /// <exception cref="Id3Exception">
        ///     Thrown if the specified native <paramref name="value"/> is invalid.
        /// </exception>
        protected virtual void ValidateValue(TValue value)
        {
        }

        public static implicit operator TValue(TextFrameBase<TValue> frame)
        {
            return frame.Value;
        }
    }
}
