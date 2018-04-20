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

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Id3
{
    public abstract class ListTextFrame : TextFrameBase<IList<string>>
    {
        private const char Separator = '/';

        protected ListTextFrame()
        {
            Value = new List<string>();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Value = new List<string>();
        }

        public new IList<string> Value { get; private set; }

        internal sealed override string TextValue
        {
            get
            {
                var sb = new StringBuilder();
                foreach (string value in Value)
                    sb.Append(value + Separator);
                if (sb.Length > 0)
                    sb.Remove(sb.Length - 1, 1);
                return sb.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Value.Clear();
                else
                {
                    string[] breakup = value.Split(Separator);
                    foreach (string s in breakup)
                        Value.Add(s);
                }
            }
        }
    }
}