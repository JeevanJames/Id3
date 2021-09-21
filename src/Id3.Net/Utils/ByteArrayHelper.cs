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
using System.Collections.Generic;

namespace Id3
{
    internal static class ByteArrayHelper
    {
        internal static bool AreEqual(byte[] bytes1, byte[] bytes2)
        {
            if (ReferenceEquals(bytes1, bytes2))
                return true;
            if (bytes1 == null || bytes2 == null)
                return false;
            if (bytes1.Length != bytes2.Length)
                return false;
            for (var i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                    return false;
            }
            return true;
        }

        internal static int LocateSequence(byte[] bytes, int start, int count, byte[] sequence)
        {
            int sequenceIndex = 0;
            int endIndex = Math.Min(bytes.Length, start + count);
            for (int byteIdx = start; byteIdx < endIndex; byteIdx++)
            {
                if (bytes[byteIdx] == sequence[sequenceIndex])
                {
                    sequenceIndex++;
                    if (sequenceIndex >= sequence.Length)
                        return byteIdx - sequence.Length + 1;
                } else
                    sequenceIndex = 0;
            }
            return -1;
        }

        internal static bool CompareSequence(byte[] bytes, int start, byte[] sequence)
        {
            if ((start + sequence.Length) > bytes.Length)
                return false;
            for (int i = 0; i < sequence.Length; i++)
            {
                if (bytes[start + i] != sequence[i])
                    return false;
            }
            return true;
        }
    }
}