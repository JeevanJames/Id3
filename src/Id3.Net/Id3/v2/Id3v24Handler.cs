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
using System.IO;
using System.Text;

namespace Id3.v2
{
    internal sealed class Id3V24Handler : Id3V2Handler
    {
        internal override void DeleteTag(Stream stream)
        {
            throw new NotSupportedException("ID3 v2.4 is not yet supported in the ID3.NET library.");
        }

        internal override byte[] GetTagBytes(Stream stream)
        {
            throw new NotSupportedException("ID3 v2.4 is not yet supported in the ID3.NET library.");
        }

        internal override bool HasTag(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var headerBytes = new byte[5];
            stream.Read(headerBytes, 0, 5);

            string magic = Encoding.ASCII.GetString(headerBytes, 0, 3);
            return magic == "ID3" && headerBytes[3] == 4;
        }

        internal override Id3Tag ReadTag(Stream stream)
        {
            var tag = new Id3Tag {
                MajorVersion = 2,
                MinorVersion = 4,
                Family = Id3TagFamily.Version2X,
                IsSupported = false
            };
            return tag;
        }

        internal override bool WriteTag(Stream stream, Id3Tag tag)
        {
            throw new NotSupportedException("ID3 v2.4 is not yet supported in the ID3.NET library.");
        }

        internal override int MinorVersion => 4;

        protected override void BuildFrameHandlers(FrameHandlers mappings)
        {
            throw new NotSupportedException("ID3 v2.4 is not yet supported in the ID3.NET library.");
        }
    }
}