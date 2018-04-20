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

using System.Globalization;
using System.Text.RegularExpressions;

namespace Id3
{
    public sealed class TrackFrame : TextFrameBase<int>
    {
        public int TrackCount { get; set; }

        internal override string TextValue
        {
            get
            {
                if (Value <= 0 && TrackCount <= 0)
                    return null;
                if (TrackCount <= 0)
                    return Value.ToString(CultureInfo.InvariantCulture);
                return Value <= 0 ? $"0/{TrackCount}" : $"{Value}/{TrackCount}";
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Value = 0;
                    TrackCount = 0;
                } else
                {
                    Match match = TrackPattern.Match(value);
                    if (!match.Success)
                    {
                        Value = 0;
                        TrackCount = 0;
                    } else
                    {
                        Value = int.Parse(match.Groups[1].Value);
                        TrackCount = !string.IsNullOrEmpty(match.Groups[2].Value)
                            ? int.Parse(match.Groups[2].Value)
                            : 0;
                    }
                }
            }
        }

        private static readonly Regex TrackPattern = new Regex(@"^(\d+)(?:/(\d+))?$");
    }
}
