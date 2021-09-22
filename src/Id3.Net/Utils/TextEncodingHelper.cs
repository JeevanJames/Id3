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
using System.Text;

// from: https://id3.org/id3v2.3.0#ID3v2_frame_overview
// If nothing else is said a string is represented as ISO-8859-1 characters in the range $20 - $FF.
// Such strings are represented as <text string>, or<full text string> if newlines are allowed, in the frame descriptions.
// All Unicode strings use 16-bit unicode 2.0 (ISO/IEC 10646-1:1993, UCS-2). 
// Unicode strings must begin with the Unicode BOM($FF FE or $FE FF) to identify the byte order.
// 
// All numeric strings and URLs are always encoded as ISO-8859-1. 
// Terminated strings are terminated with $00 if encoded with ISO-8859-1 and $00 00 if encoded as unicode.
// If nothing else is said newline character is forbidden.
// In ISO-8859-1 a new line is represented, when allowed, with $0A only.
// Frames that allow different types of text encoding have a text encoding description byte directly after the frame size. 
// If ISO-8859-1 is used this byte should be $00, if Unicode is used it should be $01. 
// Strings dependent on encoding is represented as <text string according to encoding>, or<full text string according to encoding> if newlines are allowed.
// Any empty Unicode strings which are NULL-terminated may have the Unicode BOM followed by a Unicode NULL ($FF FE 00 00 or $FE FF 00 00). 

namespace Id3
{
    internal static class TextEncodingHelper
    {
        //Gets the default encoding, which is ISO-8859-1
        internal static Encoding GetDefaultEncoding()
        {
            return GetEncoding(Id3TextEncoding.Iso8859_1);
        }

        internal static string GetDefaultString(byte[] bytes, int start, int count)
        {
            return GetDefaultEncoding().GetString(bytes, start, count);
        }

        internal static Encoding GetEncoding(Id3TextEncoding encodingType)
        {
            if (encodingType == Id3TextEncoding.Iso8859_1)
                return Encoding.GetEncoding("iso-8859-1");
            if (encodingType == Id3TextEncoding.Unicode)
                return Encoding.Unicode;
            Debug.Assert(false, "Invalid Encoding type specified");
            return null;
        }

        // decode a string according to the encoding until a termination bytes $00 ($00) is found or end of buffer is reached.
        // respect the byte count according the encoding so the correct termination bytes are recognized.
        internal static string DecodeString(byte[] bytes, ref int currentPos, Id3TextEncoding encodingType)
        {
            int startIndex = currentPos;
            int endIndex = currentPos;
            int charBytes = GetBytesPerCharacter(encodingType);
            if (charBytes < 1)
                return null;   // invalid encodingType, cannot process

            byte[] terminationBytes = GetTerminationBytes(encodingType);
            while (endIndex < bytes.Length && !ByteArrayHelper.CompareSequence(bytes, endIndex, terminationBytes))
                endIndex += charBytes;

            //if (endIndex >= bytes.Length)
            //    return null;   // termination sequence not found within remaining bytes

            // endIndex points to the first termination byte (or behind the last byte)
            currentPos = endIndex + terminationBytes.Length;
            int byteCount = endIndex - startIndex;

            if (byteCount <= 0)
                return null;    // empty string, return as no-string

            Encoding encoding = GetEncoding(encodingType);
            if (encodingType == Id3TextEncoding.Iso8859_1)
            {
                return encoding.GetString(bytes, startIndex, byteCount);
            }
            if (encodingType == Id3TextEncoding.Unicode)
            {
                string result;
                // test BOM for little-endian-order
                Debug.Assert(encoding == Encoding.Unicode, "Wrong encoding for little-endian-order");
                if (TryDecodeStringWithBom(bytes, startIndex, byteCount, encoding, out result))
                    return result;
                // test BOM for big-endian-order
                if (TryDecodeStringWithBom(bytes, startIndex, byteCount, Encoding.BigEndianUnicode, out result))
                    return result;
                Debug.Assert(false, "Could not detect BOM for unicode decoding");
                return null;
            }
            Debug.Assert(false, "Invalid encoding type specified");
            return null;
        }

        private static bool TryDecodeStringWithBom(byte[] bytes, int startIndex, int byteCount, Encoding encoding, out string result)
        {
            result = null;
            byte[] preamble = encoding.GetPreamble();
            if (preamble.Length == 0 || ByteArrayHelper.CompareSequence(bytes, startIndex, preamble))
            {
                // skip the preamble
                startIndex += preamble.Length;
                byteCount -= preamble.Length;
                if (byteCount <= 0)
                    return true;
                result = encoding.GetString(bytes, startIndex, byteCount);
                return true;
            }
            return false;
        }

        internal static byte[] GetTerminationBytes(Id3TextEncoding encodingType)
        {
            if (encodingType == Id3TextEncoding.Iso8859_1)
                return terminationBytesIso8859;
            if (encodingType == Id3TextEncoding.Unicode)
                return terminationBytesUnicode;
            Debug.Assert(false, "Invalid encoding type specified");
            return null;
        }

        private static int GetBytesPerCharacter(Id3TextEncoding encodingType)
        {
            if (encodingType == Id3TextEncoding.Iso8859_1)
                return 1;
            if (encodingType == Id3TextEncoding.Unicode)
                return 2;
            Debug.Assert(false, "Invalid encoding type specified");
            return -1;
        }

        private static readonly byte[] terminationBytesIso8859 = new byte[] { 0 };
        private static readonly byte[] terminationBytesUnicode = new byte[] { 0, 0 };
    }
}