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

namespace Id3.Frames
{
    public sealed class RecordingDateFrame : DateTimeFrame
    {
        public RecordingDateFrame()
        {
        }

        public RecordingDateFrame(DateTime value) : base(value)
        {
        }

        protected override string DateTimeFormat => "ddMM";

        public static implicit operator RecordingDateFrame(DateTime value) => new RecordingDateFrame(value);
    }
}