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

using System.Collections.Generic;
using System.IO;

namespace Id3.InfoFx
{
    /// <summary>
    ///     The inputs to the info provider.
    ///     <para/>
    ///     Depending on the specific info provider, none or more of the input properties specified in this class will
    ///     be needed. Read the info provider's documentation for more details or use the info provider's
    ///     <see cref="InfoProvider.Properties"/> property to understand the required inputs.
    /// </summary>
    public sealed class InfoProviderInputs
    {
        /// <summary>
        ///     An <see cref="Id3Tag"/> instance that acts as an input.
        /// </summary>
        public Id3Tag Tag { get; set; }

        /// <summary>
        ///     The file name that acts as an input.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     The MP3 stream data that acts as an input.
        /// </summary>
        public Stream Mp3Stream { get; set; }

        /// <summary>
        ///     Additional properties that may be needed by certain info providers.
        /// </summary>
        public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();
    }
}