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

namespace Id3
{
    public sealed class AudioStreamProperties
    {
        private readonly int _bitrate;
        private readonly TimeSpan _duration;
        private readonly int _frequency;
        private readonly AudioMode _mode;

        public AudioStreamProperties(int bitrate, int frequency, TimeSpan duration, AudioMode mode)
        {
            _bitrate = bitrate;
            _frequency = frequency;
            _duration = duration;
            _mode = mode;
        }

        public int Bitrate
        {
            get { return _bitrate; }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
        }

        public int Frequency
        {
            get { return _frequency; }
        }

        public AudioMode Mode
        {
            get { return _mode; }
        }
    }
}