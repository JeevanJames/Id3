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
    public abstract class NumericFrame : TextFrameBase<int?>
    {
        protected NumericFrame()
        {
        }

        protected NumericFrame(int? value)
            : base(value)
        {
        }

        internal sealed override string TextValue
        {
            get => Value?.ToString();
            set
            {
                if (string.IsNullOrEmpty(value))
                    Value = null;
                else
                    Value = !int.TryParse(value, out int asInt) ? (int?)null : asInt;
            }
        }
    }
}
