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
using System.Linq;

namespace Id3
{
    /// <summary>
    /// Compares two tags for equality.
    /// </summary>
    public sealed class TagComparer : IEqualityComparer<Id3Tag>
    {
        bool IEqualityComparer<Id3Tag>.Equals(Id3Tag tag1, Id3Tag tag2)
        {
            if (ReferenceEquals(tag1, tag2))
                return true;
            if (tag1 == null || tag2 == null)
                return false;
            if (tag1.Count() != tag2.Count())
                return false;

            //TODO: Compare frames

            return true;
        }

        int IEqualityComparer<Id3Tag>.GetHashCode(Id3Tag tag)
        {
            return tag.GetHashCode();
        }
    }
}
